using UnityEngine;
using BanRaiValley.Farming;

/// <summary>
/// Runtime component representing a single growing crop on a farm tile.
/// Manages stage progression, visual model swapping, withering, regrowth,
/// and exposes <see cref="IInteractable"/> for first-person raycast harvest prompts.
/// </summary>
/// <remarks>
/// Lifecycle: Spawned by the planting system → <see cref="Initialize"/> called immediately →
/// <see cref="AdvanceGrowthDay"/> called each morning by the CropGrowthManager →
/// destroyed or returned to pool after harvest or clearing.
/// </remarks>
public class CropInstance : MonoBehaviour, ICropInstance, IInteractable
{
    #region Serialized Fields

    [Header("Transforms & Containers")]
    [Tooltip("Parent transform under which all stage visual prefabs are instantiated. " +
             "Must be a dedicated child so visuals can be destroyed independently.")]
    [SerializeField] private Transform _visualContainer;

    [Tooltip("Optional component that handles the crop-to-monster awakening sequence. " +
             "Leave null for crops that are harvested normally without awakening.")]
    [SerializeField] private AwakenedCropHarvestTrigger _awakenedHarvestTrigger;

    #endregion


    #region Fields

    private Vector3Int _cellPos;
    private CropDataSO _cropData;
    private CropState _currentState = CropState.Growing;
    private int _currentStageIndex = 0;
    private int _daysInCurrentStage = 0;
    private GameObject _activeVisualInstance;

    #endregion


    #region Properties

    /// <summary>Tilemap cell position this crop instance occupies.</summary>
    public Vector3Int CellPos => _cellPos;

    /// <summary>The ScriptableObject data asset driving this crop's configuration.</summary>
    public CropDataSO CropData => _cropData;

    /// <summary>Current lifecycle state of this crop.</summary>
    public CropState CurrentState => _currentState;

    /// <summary>Zero-based index of the growth stage this crop is currently in.</summary>
    public int CurrentStageIndex => _currentStageIndex;

    /// <summary>Number of watered days accumulated in the current growth stage.</summary>
    public int DaysInCurrentStage => _daysInCurrentStage;

    /// <summary>Returns true when the crop is fully grown and awaiting player interaction.</summary>
    public bool IsMature => _currentState == CropState.Mature;

    /// <summary>Returns true when the crop has withered and can be cleared by the player.</summary>
    public bool IsWithered => _currentState == CropState.Withered;

    /// <summary>
    /// Human-readable interaction prompt shown in the UI based on current crop state.
    /// Returns <see cref="string.Empty"/> when the crop is not interactable.
    /// </summary>
    public string InteractionLabel
    {
        get
        {
            if (_currentState == CropState.Mature)
                return "[E] Awaken & Harvest " + _cropData.CropName;

            if (_currentState == CropState.Withered)
                return "[E] Clear Withered Crop";

            return string.Empty;
        }
    }

    #endregion


    #region Public Methods

    /// <summary>
    /// Initialises this crop instance at the given tilemap cell with the specified crop data.
    /// Must be called immediately after spawning, before any growth ticks.
    /// </summary>
    /// <param name="cellPos">The tilemap cell this crop occupies.</param>
    /// <param name="cropData">The crop configuration ScriptableObject driving this instance.</param>
    public void Initialize(Vector3Int cellPos, CropDataSO cropData)
    {
        _cellPos = cellPos;
        _cropData = cropData;
        _currentState = CropState.Growing;
        _currentStageIndex = 0;
        _daysInCurrentStage = 0;

        if (_awakenedHarvestTrigger != null)
            _awakenedHarvestTrigger.Initialize(cellPos);

        UpdateVisualForCurrentStage();
    }

    /// <summary>
    /// Advances growth by one in-game day. Only progresses when the crop was watered
    /// and is still in the <see cref="CropState.Growing"/> state.
    /// Raises <see cref="OnCropStageChangedEvent"/> when a stage boundary is crossed.
    /// </summary>
    /// <param name="wasWatered">True if this crop's tile was watered during the day being evaluated.</param>
    public void AdvanceGrowthDay(bool wasWatered)
    {
        if (_currentState != CropState.Growing || !wasWatered)
            return;

        _daysInCurrentStage++;

        if (_daysInCurrentStage < _cropData.Stages[_currentStageIndex].DaysRequired)
            return;

        // Stage threshold reached — attempt to advance.
        if (_currentStageIndex >= _cropData.FinalStageIndex)
            return;

        int previousIndex = _currentStageIndex;
        _currentStageIndex++;
        _daysInCurrentStage = 0;

        if (_currentStageIndex == _cropData.FinalStageIndex)
            _currentState = CropState.Mature;

        UpdateVisualForCurrentStage();

        EventBus<OnCropStageChangedEvent>.Raise(new OnCropStageChangedEvent
        {
            CropInstance       = gameObject,
            CellPos            = _cellPos,
            PreviousStageIndex = previousIndex,
            NewStageIndex      = _currentStageIndex,
            IsMature           = IsMature,
        });
    }

    /// <summary>
    /// Transitions the crop into the <see cref="CropState.Withered"/> state, swapping
    /// visuals to the withered prefab and raising <see cref="OnCropWitheredEvent"/>.
    /// </summary>
    public void SetWithered()
    {
        _currentState = CropState.Withered;
        SwapVisualTo(_cropData.WitheredPrefab);

        EventBus<OnCropWitheredEvent>.Raise(new OnCropWitheredEvent
        {
            CropInstance = gameObject,
            CellPos      = _cellPos,
            CropData     = _cropData,
        });
    }

    /// <summary>
    /// Resets a regrowable crop back to its configured regrowth stage after harvest.
    /// Raises <see cref="OnCropStageChangedEvent"/> to notify visual and tracking systems.
    /// Has no effect if <see cref="CropDataSO.IsRegrowable"/> is false.
    /// </summary>
    public void ResetToRegrowth()
    {
        if (!_cropData.IsRegrowable)
            return;

        int previousIndex = _currentStageIndex;
        _currentState = CropState.Growing;
        _currentStageIndex = _cropData.RegrowStageIndex;
        _daysInCurrentStage = 0;

        UpdateVisualForCurrentStage();

        EventBus<OnCropStageChangedEvent>.Raise(new OnCropStageChangedEvent
        {
            CropInstance       = gameObject,
            CellPos            = _cellPos,
            PreviousStageIndex = previousIndex,
            NewStageIndex      = _currentStageIndex,
            IsMature           = false,
        });
    }

    /// <summary>
    /// Returns true when the crop is mature or withered — the only states that allow player interaction.
    /// </summary>
    /// <param name="interactor">The player or tool GameObject requesting interaction.</param>
    /// <returns>True if the crop can be interacted with right now.</returns>
    public bool CanInteract(GameObject interactor)
    {
        return _currentState == CropState.Mature || _currentState == CropState.Withered;
    }

    /// <summary>
    /// Executes the interaction: raises harvest request for mature crops,
    /// or clears withered crops from the tile.
    /// </summary>
    /// <param name="interactor">The player or tool GameObject triggering the interaction.</param>
    public void Interact(GameObject interactor)
    {
        if (_currentState == CropState.Mature)
        {
            EventBus<OnCropHarvestRequestedEvent>.Raise(new OnCropHarvestRequestedEvent
            {
                CropInstance = gameObject,
                CellPos      = _cellPos,
                Interactor   = interactor,
            });

            if (_awakenedHarvestTrigger != null)
            {
                // Push the crop-specific monster prefab from CropDataSO before awakening.
                _awakenedHarvestTrigger.SetMonsterPrefab(_cropData.AwakenedMonsterPrefab);
                _awakenedHarvestTrigger.TriggerAwakening(isRegrowable: _cropData.IsRegrowable);

                // For regrowable crops the trigger skips OnClearPlant; we handle the visual reset here.
                if (_cropData.IsRegrowable)
                    ResetToRegrowth();
            }
            else
            {
                // No awakening component — clear the tile directly.
                EventBus<OnClearPlant>.Raise(new OnClearPlant { CellPos = _cellPos });
            }

            return;
        }

        if (_currentState == CropState.Withered)
        {
            EventBus<OnClearPlant>.Raise(new OnClearPlant { CellPos = _cellPos });
        }
    }

    #endregion


    #region Private Methods

    /// <summary>
    /// Destroys the currently active visual instance and instantiates the prefab
    /// for <see cref="_currentStageIndex"/> under <see cref="_visualContainer"/>.
    /// Safe to call when the stage prefab reference is null (no visual is shown).
    /// </summary>
    private void UpdateVisualForCurrentStage()
    {
        if (_cropData == null || _cropData.Stages == null || _cropData.Stages.Count == 0)
            return;

        CropStageData stageData = _cropData.Stages[_currentStageIndex];
        SwapVisualTo(stageData.StageVisualPrefab);
    }

    /// <summary>
    /// Destroys the previous visual instance and spawns <paramref name="prefab"/>
    /// under <see cref="_visualContainer"/> with zeroed local transform.
    /// </summary>
    /// <param name="prefab">The prefab to instantiate as the new visual. May be null to show nothing.</param>
    private void SwapVisualTo(GameObject prefab)
    {
        if (_activeVisualInstance != null)
        {
            Destroy(_activeVisualInstance);
            _activeVisualInstance = null;
        }

        if (prefab == null)
            return;

        Transform parent = _visualContainer != null ? _visualContainer : transform;
        _activeVisualInstance = Instantiate(prefab, parent);
        _activeVisualInstance.transform.localPosition = Vector3.zero;
        _activeVisualInstance.transform.localRotation = Quaternion.identity;
    }

    #endregion
}
