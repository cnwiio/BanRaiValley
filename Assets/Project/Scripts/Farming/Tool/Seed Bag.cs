using UnityEngine;

public enum SeedBagState
{
    Idle,
    Farm,
    Planting
}

public class SeedBag : FarmingToolBase
{
    [SerializeField] private Animator seedBagAnimator;
    [SerializeField] private GameObject hologramPrefab;
    [SerializeField] private GameObject plantPrefab;
    
    private SeedBagState _currentState = SeedBagState.Farm;

    public SeedBagState CurrentState
    {
        get => _currentState;
        set
        {
            if (_currentState == value) return;

            // on exit state
            if (_currentState == SeedBagState.Farm)
            {
                if (value != SeedBagState.Planting)
                    EndPreviewNow();
            }

            if (value == SeedBagState.Planting)
            {
                EventBus<OnStartPlantingEvent>.Raise(new OnStartPlantingEvent());
            }
            _currentState = value;
        }
    }
    
    private const int SEED_BAG_RANGE = 10;
    private Vector3 _plantingPos;
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
        CurrentState = SeedBagState.Farm;
    }
    
    void OnChangeActionMap(ChangeActionMap evt)
    {
        if (evt.MapType != ActionMapType.Player)
        {
            CurrentState = SeedBagState.Idle;
        }
        else
        {
            CurrentState = SeedBagState.Farm;
        }
    }

    private void StartPlanting()
    {
        CurrentState = SeedBagState.Planting;
        seedBagAnimator.SetTrigger("Sowing");
    }

    public void OnPlantingAnimationFinished()
    {
        if (grid.TryPlanting(_plantingPos, out var cellPos))
        {
            EventBus<OnPlantingEvent>.Raise(new OnPlantingEvent() { Prefab = plantPrefab, Position = _plantingPos, CellPos = cellPos});
            EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { Position = _plantingPos, IsValid = false, YRotation = 0 });
        }

        CurrentState = SeedBagState.Farm;
    }
    
    protected override void PrimaryAction()
    {
        if (!TryGetGrid()) return;

        if (grid.IsPlantable(_hit.point, out var cellWorldPos))
        {
            _plantingPos = cellWorldPos;
            StartPlanting();
        }
    }

    protected override void SecondaryAction()
    {
        // throw new System.NotImplementedException();
    }
    
    void Update()
    {
        if (CurrentState != SeedBagState.Farm) return;
        if (!TryGetGrid()) return;

        RunPreviewUpdate(SEED_BAG_RANGE, hologramPrefab, PreviewState.Planting, grid.IsPlantable, 0f);
    }
}
