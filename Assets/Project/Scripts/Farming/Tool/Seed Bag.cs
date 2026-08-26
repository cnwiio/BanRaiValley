using BanRaiValley.Farming;
using UnityEngine;

public enum SeedBagState
{
    Idle,
    Farm,
    Planting
}

public class SeedBag : FarmingToolBase
{
    #region Serialized Fields

    [Header("Visuals & Prefabs")]
    [Tooltip("Animator on the seed bag model. Must have a 'Sowing' trigger parameter.")]
    [SerializeField] private Animator seedBagAnimator;

    [Tooltip("Hologram preview prefab shown before planting.")]
    [SerializeField] private GameObject hologramPrefab;

    [Tooltip("Fallback plant prefab used when no CropDataSO is available (legacy / debug use).")]
    [SerializeField] private GameObject plantPrefab;

    [Header("Farming / Seed Data")]
    [Tooltip("Fallback CropDataSO used for editor testing when no seed item is equipped in the hotbar.")]
    [SerializeField] private CropDataSO defaultCropData;

    #endregion


    #region Fields

    private const int SEED_BAG_RANGE = 10;

    private SeedBagState _currentState = SeedBagState.Farm;
    private Vector3 _plantingPos;

    /// <summary>The CropDataSO resolved from the currently equipped hotbar seed item.</summary>
    private CropDataSO _currentCropData;

    #endregion


    #region Properties

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

    #endregion


    #region Unity Messages

    protected override void OnEnable()
    {
        base.OnEnable();
        EventBus<ChangeActionMap>.Subscribe(OnChangeActionMap);
        EventBus<OnHotbarChangeEvent>.Subscribe(OnHotbarChange);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventBus<ChangeActionMap>.Unsubscribe(OnChangeActionMap);
        EventBus<OnHotbarChangeEvent>.Unsubscribe(OnHotbarChange);

        EndPreviewNow();
        CurrentState = SeedBagState.Farm;
    }

    private void Update()
    {
        if (CurrentState != SeedBagState.Farm) return;
        if (!TryGetGrid()) return;

        RunPreviewUpdate(SEED_BAG_RANGE, hologramPrefab, PreviewState.Planting, grid.IsPlantable, 0f);
    }

    #endregion


    #region Public Methods

    /// <summary>
    /// Called by the seed bag's sowing animation event when the planting moment is reached.
    /// Resolves final crop data, raises <see cref="OnPlantingEvent"/>, and returns to idle.
    /// </summary>
    public void OnPlantingAnimationFinished()
    {
        if (grid.TryPlanting(_plantingPos, out var cellPos))
        {
            CropDataSO resolvedCropData = _currentCropData != null ? _currentCropData : defaultCropData;

            EventBus<OnPlantingEvent>.Raise(new OnPlantingEvent
            {
                CropData = resolvedCropData,
                Prefab   = plantPrefab,
                Position = _plantingPos,
                CellPos  = cellPos,
            });

            EventBus<PreviewingEvent>.Raise(new PreviewingEvent
            {
                Position  = _plantingPos,
                IsValid   = false,
                YRotation = 0,
            });
        }

        CurrentState = SeedBagState.Farm;
    }

    #endregion


    #region Private Methods

    private void StartPlanting()
    {
        CurrentState = SeedBagState.Planting;
        seedBagAnimator.SetTrigger("Sowing");
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
        // Reserved for future use (e.g. select seed type from bag).
    }

    #endregion


    #region Event Handlers

    private void OnChangeActionMap(ChangeActionMap evt)
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

    /// <summary>
    /// Tracks the currently equipped hotbar item and resolves its <see cref="CropDataSO"/>
    /// when a <see cref="ItemType.Seed"/> item is selected.
    /// Falls back to <see cref="defaultCropData"/> for non-seed or empty slots.
    /// </summary>
    private void OnHotbarChange(OnHotbarChangeEvent evt)
    {
        if (evt.slotData.item != null && evt.slotData.item.type == ItemType.Seed)
        {
            _currentCropData = evt.slotData.item.CropData;
        }
        else
        {
            _currentCropData = defaultCropData;
        }
    }

    #endregion
}
