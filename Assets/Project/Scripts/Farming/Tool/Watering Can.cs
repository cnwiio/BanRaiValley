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
    
    private readonly List<Vector3Int> _pendingWateringCells = new List<Vector3Int>(MAX_TOOL_CELLS);
    private const int WateringCanRange = 10;
    private Vector3 _wateringPos;

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

    public void OnWateringAnimationFinished()
    {
        for (int i = 0; i < _pendingWateringCells.Count; i++)
        {
            Vector3 cellWorldPos = grid.GetCellCenterWorld(_pendingWateringCells[i]);
            if (grid.TryWatering(cellWorldPos, out var cellPos))
            {
                EventBus<OnWateringEvent>.Raise(new OnWateringEvent
                {
                    CellPos = cellPos
                });
                
            }
            
        }

        for (int i = 0; i < _cachedPatternCellCount; i++)
        {
            _cachedPreviewData[i] = new PreviewTileData()
            {
                Position = _cachedPreviewData[i].Position,
                IsValid = false
            };
        }
        EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { PreviewTileData = _cachedPreviewData, YRotation = 0 });
        CurrentState = WaterCanState.Farm;
    }

    protected override void PrimaryAction()
    {
        if (!TryGetGrid()) return;
        if (CurrentState == WaterCanState.Farm)
        {
            if (!_isHit) return;
            Vector3Int centerCell = grid.WorldToCell(_hit.point);
            Vector3 lookDir = sceneCamera.transform.forward;
            Vector3Int cardinalDir = ToolAreaCalculator.GetCardinalFacingDirection(lookDir);
            
            int cellCount = ToolAreaCalculator.CalculatePatternCells(
                centerCell,
                cardinalDir,
                toolData.toolPattern,
                toolData.patternDimension,
                _cachedPatternCells
            );
            
            _pendingWateringCells.Clear();
            for (int i = 0; i < _cachedPatternCellCount; i++)
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
    }

    protected override void SecondaryAction()
    {
        //CurrentState = CurrentState == WaterCanState.Farm ? WaterCanState.Idle : WaterCanState.Farm;
    }

    void Update()
    {
        if (CurrentState != WaterCanState.Farm) return;
        if (!TryGetGrid()) return;

        if (toolData.toolPattern == ToolAreaPattern.Single || toolData.patternDimension <= 1)
        {
            RunPreviewUpdate(WateringCanRange, hologramPrefabs, PreviewState.Watering, grid.IsWaterable, 0f);
        }
        else
        {
            RunMultiPreviewUpdate(WateringCanRange, hologramPrefabs, PreviewState.Watering, grid.IsWaterable, 0f);
        }
    }
}