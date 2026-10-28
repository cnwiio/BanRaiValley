using Lean.Pool;
using UnityEngine;

public enum SeedBagState
{
    Idle,
    Farm,
    Planting
}

public class SeedBag : FarmingToolBase, IPoolable
{
    [SerializeField] private Animator seedBagAnimator;
    [SerializeField] private GameObject hologramPrefab;
    [SerializeField] private PlantData plantData;

    private static readonly int IsPlantingHash = Animator.StringToHash("IsPlanting");
    private static readonly int SowingHash = Animator.StringToHash("Sowing");

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
    private const float PLANT_SWEEP_INTERVAL = 0.18f;
    private float _plantingTimer;
    private bool _isPrimaryHeld;
    private Vector3Int _lastPlantedCellPos = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

    private Vector3 _plantingPos;
    private GameObject plantPrefab;

    #region Event Handles
    protected override void OnEnable()
    {
        base.OnEnable();
        EventBus<ChangeActionMap>.Subscribe(OnChangeActionMap);
        EventBus<OnPrimaryActionStartedEvent>.Subscribe(OnPrimaryActionStarted);
        EventBus<OnPrimaryActionCanceledEvent>.Subscribe(OnPrimaryActionCanceled);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventBus<ChangeActionMap>.Unsubscribe(OnChangeActionMap);
        EventBus<OnPrimaryActionStartedEvent>.Unsubscribe(OnPrimaryActionStarted);
        EventBus<OnPrimaryActionCanceledEvent>.Unsubscribe(OnPrimaryActionCanceled);

        EndPreviewNow();
        StopPlantingLoop();
        CurrentState = SeedBagState.Farm;
    }

    private void OnChangeActionMap(ChangeActionMap evt)
    {
        if (evt.MapType != ActionMapType.Player)
        {
            StopPlantingLoop();
            CurrentState = SeedBagState.Idle;
        }
        else
        {
            CurrentState = SeedBagState.Farm;
        }
    }

    private void OnPrimaryActionStarted(OnPrimaryActionStartedEvent evt)
    {
        if (CurrentState == SeedBagState.Idle) return;
        _isPrimaryHeld = true;

        if (TryGetGrid() && _isHit && grid.IsPlantable(_hit.point, out _))
        {
            StartPlantingLoop();
            TryPlantAtCurrentTarget();
        }
    }

    private void OnPrimaryActionCanceled(OnPrimaryActionCanceledEvent evt)
    {
        _isPrimaryHeld = false;
        if (seedBagAnimator != null)
        {
            seedBagAnimator.SetBool(IsPlantingHash, false);
        }
    }
    #endregion

    private void StartPlantingLoop()
    {
        CurrentState = SeedBagState.Planting;
        if (seedBagAnimator != null)
        {
            seedBagAnimator.SetBool(IsPlantingHash, true);
            seedBagAnimator.SetTrigger(SowingHash);
        }
    }

    private void StopPlantingLoop()
    {
        _isPrimaryHeld = false;
        _plantingTimer = 0f;
        _lastPlantedCellPos = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

        if (seedBagAnimator != null)
        {
            seedBagAnimator.SetBool(IsPlantingHash, false);
        }

        if (CurrentState == SeedBagState.Planting)
        {
            CurrentState = SeedBagState.Farm;
        }
    }

    public void OnPlantingAnimationFinished()
    {
        if (!_isPrimaryHeld)
        {
            StopPlantingLoop();
        }
    }

    private bool TryPlantAtCurrentTarget()
    {
        if (!TryGetGrid()) return false;
        if (!_isHit) return false;

        if (grid.IsPlantable(_hit.point, out var cellWorldPos))
        {
            if (grid.TryPlanting(cellWorldPos, out var cellPos))
            {
                _plantingPos = cellWorldPos;
                _lastPlantedCellPos = cellPos;
                _plantingTimer = PLANT_SWEEP_INTERVAL;

                EventBus<OnPlantingEvent>.Raise(new OnPlantingEvent()
                {
                    Prefab = plantPrefab,
                    Position = _plantingPos,
                    CellPos = cellPos,
                    PlantData = plantData
                });

                _cachedPreviewData[0] = new PreviewTileData()
                {
                    Position = _plantingPos,
                    IsValid = false
                };
                EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { PreviewTileData = _cachedPreviewData, YRotation = 0 });

                return true;
            }
        }

        return false;
    }

    protected override void PrimaryAction()
    {
        if (CurrentState == SeedBagState.Idle) return;
        if (!TryGetGrid()) return;

        if (grid.IsPlantable(_hit.point, out _))
        {
            StartPlantingLoop();
            TryPlantAtCurrentTarget();
        }
    }

    protected override void SecondaryAction()
    {
    }

    private void Update()
    {
        if (CurrentState == SeedBagState.Idle) return;
        if (!TryGetGrid()) return;

        RunPreviewUpdate(SEED_BAG_RANGE, hologramPrefab, PreviewState.Planting, grid.IsPlantable, 0f);

        if (_isPrimaryHeld)
        {
            _plantingTimer -= Time.deltaTime;
            if (_plantingTimer <= 0f)
            {
                if (TryPlantAtCurrentTarget())
                {
                    if (CurrentState != SeedBagState.Planting)
                    {
                        StartPlantingLoop();
                    }
                }
            }
        }
    }

    public void OnSpawn()
    {
        plantPrefab = plantData.plantPrefabs;
    }

    public void OnDespawn()
    {
        StopPlantingLoop();
    }
}
