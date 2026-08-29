using UnityEngine;

public enum HoeState
{
    Idle,
    Farming,
    Tilling,
    Deleting,
    Attacking
}

public class Hoe : FarmingToolBase
{
    [SerializeField] private GameObject dirtHologramPrefabs;
    [SerializeField] private GameObject deleteHologramPrefabs;
    [SerializeField] private GameObject dirtPrefabs;
    [SerializeField] private Animator hoeAnimator;

    private HoeState _currentState = HoeState.Idle;
    public HoeState CurrentState
    {
        get => _currentState;
        set
        {
            if (_currentState != value)
            {
                // on exit state
                switch (_currentState)
                {
                    case HoeState.Idle:
                        break;
                    case HoeState.Farming:
                        if (value != HoeState.Tilling)
                            EndPreviewNow();
                        break;
                    case HoeState.Tilling:
                        EndPreviewNow();
                        break;
                    case HoeState.Deleting:
                        EndPreviewNow();
                        break;
                }
                // on enter state
                switch (value)
                {
                    case HoeState.Tilling:
                        EventBus<OnStartTillingEvent>.Raise(new OnStartTillingEvent() { });
                        break;
                }
            }
            
            // Debug.Log(value);
            _currentState = value;
        }
    }

    private ComboAttackController combo;
    
    private const int HoeRange = 10;
    private float currentYRotate;
    private Vector3 _dirtPos;

    protected override void Awake()
    {
        base.Awake();
        combo = new ComboAttackController(hoeAnimator, 2);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EventBus<ChangeActionMap>.Subscribe(OnChangeActionMap);
        EventBus<OnRotateActionEvent>.Subscribe(OnRotateAction);
        EventBus<OnDeleteActionEvent>.Subscribe(OnDeleteAction);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventBus<ChangeActionMap>.Unsubscribe(OnChangeActionMap);
        EventBus<OnRotateActionEvent>.Unsubscribe(OnRotateAction);
        EventBus<OnDeleteActionEvent>.Unsubscribe(OnDeleteAction);

        CurrentState = HoeState.Idle;
    }

    void OnChangeActionMap(ChangeActionMap evt)
    {
        if (evt.MapType != ActionMapType.Player)
        {
            CurrentState = HoeState.Idle;
        }
    }

    void OnRotateAction(OnRotateActionEvent evt)
    {
        if (CurrentState == HoeState.Farming)
        {
            currentYRotate += 90;
            EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { Position = _lastCellWorldPos, IsValid = lastCheckWasValid, YRotation = currentYRotate });
        }
    }

    void OnDeleteAction(OnDeleteActionEvent evt)
    {
        if (CurrentState != HoeState.Tilling && 
            CurrentState != HoeState.Attacking)
            CurrentState = CurrentState == HoeState.Deleting ? HoeState.Idle : HoeState.Deleting;
    }

    protected override void PrimaryAction()
    {
        if (!TryGetGrid()) return;

        if (CurrentState == HoeState.Farming)
        {
            if (!_isHit) return;
            if (grid.IsValidForTilling(_hit.point, out var cellWorldPos))
            {
                _dirtPos = cellWorldPos;
                StartTilling();
            }
        }
        else if (CurrentState == HoeState.Deleting)
        {
            if (!_isHit) return;
            if (grid.IsPlanted(_hit.point, out var cellWorldPos))
            {
                // _dirtPos = cellWorldPos1;
                DeletePlant(cellWorldPos);
            }
            else if (grid.IsTilled(_hit.point, out cellWorldPos))
            {
                // _dirtPos = cellWorldPos2;
                DeleteTile(cellWorldPos);
            }
        } 
        else if (CurrentState == HoeState.Idle)
        {
            CurrentState = HoeState.Attacking;
            combo.TryAttack();
        }
        else if (CurrentState == HoeState.Attacking)
        {
            combo.TryAttack();
        }
    }

    protected override void SecondaryAction()
    {
        if (CurrentState != HoeState.Tilling && 
            CurrentState != HoeState.Attacking)
            CurrentState = CurrentState == HoeState.Farming ? HoeState.Idle : HoeState.Farming;
    }

    private void StartTilling()
    {
        CurrentState = HoeState.Tilling;
        hoeAnimator.SetTrigger("Tilling");
    }

    private void DeleteTile(Vector3 pos)
    {
        // Only tell the world "a tile was cleared" if the grid's state actually
        // changed - keeps HoeFarmingBehaviour's spawned-object registry in sync
        // with FarmingGrid's logical TileState.
        if (!grid.TryUntill(pos, out var cellPos)) return;

        EventBus<OnTileClearEvent>.Raise(new OnTileClearEvent() { CellPos = cellPos });
        EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { Position = _lastCellWorldPos, IsValid = false, YRotation = currentYRotate });
    }

    private void DeletePlant(Vector3 pos)
    {
        if (!grid.TryClearPlant(pos, out var cellPos)) return;
        
        EventBus<OnClearPlantEvent>.Raise(new OnClearPlantEvent() {CellPos = cellPos});
    }

    public void OnTillingAnimationFinish()
    {
        if (grid.TryTill(_dirtPos, out var cellPos))
        {
            EventBus<OnTillingImpactEvent>.Raise(new OnTillingImpactEvent() { prefabs = dirtPrefabs, Position = _dirtPos, YRotation = currentYRotate, CellPos = cellPos });
        }

        CurrentState = HoeState.Farming;
    }

    public void OnAttackAnimationHit()
    {
        combo.OnAnimationHit();
        EventBus<OnPlayerRequestAttackEvent>.Raise(new OnPlayerRequestAttackEvent());
        if (!combo.IsAttacking()) CurrentState = HoeState.Idle;
    }

    void Update()
    {
        if (!TryGetGrid()) return;

        if (CurrentState == HoeState.Farming)
        {
            RunPreviewUpdate(HoeRange, dirtHologramPrefabs, PreviewState.Build, grid.IsValidForTilling, currentYRotate);
        }
        else if (CurrentState == HoeState.Deleting)
        {
            RunPreviewUpdate(HoeRange, deleteHologramPrefabs, PreviewState.Delete, grid.IsTilled, currentYRotate);
        }
    }
}