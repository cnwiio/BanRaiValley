using System.Collections.Generic;
using UnityEngine;

public enum WaterCanState
{
    Idle,
    Farm,
    Watering
}

public class WateringCan : FarmingToolBase
{
    [SerializeField] private Animator wateringCanAnimator;
    [SerializeField] private GameObject hologramPrefabs;
    [SerializeField] private Material WateringMaterial;

    private readonly List<Vector3Int> _pendingWateringCells = new List<Vector3Int>(MAX_TOOL_CELLS);

    private WaterCanState _currentState = WaterCanState.Farm;
    private WaterCanState CurrentState
    {
        get => _currentState;
        set
        {
            if (_currentState == value) return;

            // on exit state
            if (_currentState == WaterCanState.Farm)
            {
                if (value != WaterCanState.Watering)
                    EndPreviewNow();
            }

            if (value == WaterCanState.Watering)
            {
                EventBus<OnStartWateringEvent>.Raise(new OnStartWateringEvent());
            }

            _currentState = value;
        }
    }

    private const int WateringCanRange = 10;

    protected override void Awake()
    {
        base.Awake();
        if (toolData.patternDimension <= 0)
        {
            toolData = ItemToolData.DefaultWateringCan;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EventBus<ChangeActionMap>.Subscribe(OnChangeActionMap);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventBus<ChangeActionMap>.Unsubscribe(OnChangeActionMap);
        
        EndPreviewNow();
        CurrentState = WaterCanState.Farm;
    }
    
    void OnChangeActionMap(ChangeActionMap evt)
    {
        if (evt.MapType != ActionMapType.Player)
        {
            CurrentState = WaterCanState.Idle;
        }
        else
        {
            CurrentState = WaterCanState.Farm;
        }
    }
    
    private void StartWatering()
    {
        CurrentState = WaterCanState.Watering;
        wateringCanAnimator.SetTrigger("watering");
    }

    public void OnWaterinAnimationFinished()
    {
        for (int i = 0; i < _pendingWateringCells.Count; i++)
        {
            Vector3 cellWorldPos = grid.GetCellCenterWorld(_pendingWateringCells[i]);
            if (grid.TryWatering(cellWorldPos, out var cellPos))
            {
                EventBus<OnWateringEvent>.Raise(new OnWateringEvent
                {
                    CellPos = cellPos,
                    Material = WateringMaterial
                });
            }
        }

        _pendingWateringCells.Clear();
        CurrentState = WaterCanState.Farm;
    }

    protected override void PrimaryAction()
    {
        if (!TryGetGrid()) return;

        Vector3Int centerCell = grid.WorldToCell(_hit.point);
        int cellCount = ToolAreaCalculator.CalculatePatternCells(
            centerCell,
            Vector3Int.forward,
            toolData.patternShape,
            toolData.patternDimension,
            _cachedPatternCells
        );

        _pendingWateringCells.Clear();
        for (int i = 0; i < cellCount; i++)
        {
            Vector3 cellWorldPos = grid.GetCellCenterWorld(_cachedPatternCells[i]);
            if (grid.IsWaterable(cellWorldPos, out _))
            {
                _pendingWateringCells.Add(_cachedPatternCells[i]);
            }
        }

        if (_pendingWateringCells.Count > 0)
        {
            StartWatering();
        }
    }

    protected override void SecondaryAction()
    {
        // Reserved for secondary tool actions
    }

    void Update()
    {
        if (CurrentState != WaterCanState.Farm) return;
        if (!TryGetGrid()) return;

        RunMultiPreviewUpdate(WateringCanRange, hologramPrefabs, PreviewState.Watering, grid.IsWaterable, 0f);
    }
}