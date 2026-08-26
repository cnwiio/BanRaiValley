using Unity.VisualScripting;
using UnityEditor.Toolbars;
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

    public void OnWaterinAnimationFinished()
    {
        if (grid.TryWatering(_wateringPos, out var cellPos))
        {
            EventBus<OnWateringEvent>.Raise(new OnWateringEvent() { CellPos = cellPos});
            EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { Position = _lastCellWorldPos, IsValid = false, YRotation = 0 });
        }

        CurrentState = WaterCanState.Farm;
    }

    protected override void PrimaryAction()
    {
        // watering logic goes here later
        if (!TryGetGrid()) return;

        if (grid.IsWaterable(_hit.point, out var cellWorldPos))
        {
            _wateringPos = cellWorldPos;
            StartWatering();
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

        RunPreviewUpdate(WateringCanRange, hologramPrefabs, PreviewState.Watering, grid.IsWaterable, 0f);
    }
}