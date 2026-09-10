using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Shared skeleton for any first-person tool that aims at the FarmingGrid and shows
/// a hologram preview (Hoe, WateringCan, and future tools like Fertilizer/Seeder).
///
/// What lives here vs. what stays in the subclass:
/// - HERE: camera/mouse setup, raycasting, lazy grid resolution, the
///   raycast->check->preview-event loop (RunPreviewUpdate / RunMultiPreviewUpdate), primary/secondary
///   input wiring, item data binding via IToolItemReceiver.
/// - SUBCLASS: its own state machine (Hoe has 4 states, WateringCan has fewer),
///   which grid check + prefab + PreviewState to use, and what Primary/Secondary
///   actually do.
/// </summary>
public abstract class FarmingToolBase : MonoBehaviour, IToolItemReceiver
{
    [SerializeField] protected FarmingGridReference farmingGridReference;
    [SerializeField] protected ItemToolData toolData = ItemToolData.DefaultHoe;

    protected const int MAX_TOOL_CELLS = 49;
    protected readonly Vector3Int[] _cachedPatternCells = new Vector3Int[MAX_TOOL_CELLS];
    protected readonly TilePreviewData[] _cachedPreviewData = new TilePreviewData[MAX_TOOL_CELLS];
    protected int _cachedPatternCellCount;

    protected Camera sceneCamera;
    private Mouse currentMouse;
    protected IFarmingGrid grid;

    protected Ray _ray;
    protected RaycastHit _hit;
    protected Vector3 _lastCellWorldPos;
    /// <summary>Result of the most recent RunPreviewUpdate check - handy for handlers
    /// (like a rotate action) that need "is the currently-aimed cell valid?" without
    /// re-running the grid check themselves.</summary>
    protected bool lastCheckWasValid;

    /// <summary>Matches the signature of IFarmingGrid.IsValidForTilling / IsTilled / IsWaterable / etc.</summary>
    public delegate bool GridCheck(Vector3 worldPos, out Vector3 cellWorldPos);

    public virtual void BindItemData(Item item)
    {
        if (item != null)
        {
            toolData = item.ToolData;
        }
    }

    protected virtual void Awake()
    {
        sceneCamera = Camera.main;
        currentMouse = Mouse.current;
    }

    protected virtual void OnEnable()
    {
        EventBus<OnPrimaryActionEvent>.Subscribe(HandlePrimaryAction);
        EventBus<OnSecondaryActionEvent>.Subscribe(HandleSecondaryAction);
    }

    protected virtual void OnDisable()
    {
        EventBus<OnPrimaryActionEvent>.Unsubscribe(HandlePrimaryAction);
        EventBus<OnSecondaryActionEvent>.Unsubscribe(HandleSecondaryAction);
    }

    private void HandlePrimaryAction(OnPrimaryActionEvent evt) => PrimaryAction();
    private void HandleSecondaryAction(OnSecondaryActionEvent evt) => SecondaryAction();

    protected abstract void PrimaryAction();
    protected abstract void SecondaryAction();

    protected Ray RayCastAtCursor()
    {
        Vector3 mousePos = currentMouse.position.ReadValue();
        mousePos.z = sceneCamera.nearClipPlane;
        return sceneCamera.ScreenPointToRay(mousePos);
    }

    /// <summary>
    /// Lazily resolves the grid. Call this at the top of Update() before touching
    /// `grid` - resolving lazily (instead of caching once in Awake) avoids breaking
    /// when FarmingGrid registers itself after this component's Awake has run.
    /// </summary>
    protected bool TryGetGrid()
    {
        grid ??= farmingGridReference != null ? farmingGridReference.Grid : null;
        return grid != null;
    }

    /// <summary>
    /// The raycast -> validity-check -> preview-event loop shared by single-tile preview callers.
    /// </summary>
    bool _isValid;
    protected void RunPreviewUpdate(int range, GameObject hologramPrefab, PreviewState previewState, GridCheck check, float yRotation)
    {
        _ray = RayCastAtCursor();
        if (Physics.Raycast(_ray, out _hit, range))
        {
            _isValid = check(_hit.point, out var cellWorldPos);
            lastCheckWasValid = _isValid;
            if (_lastCellWorldPos != cellWorldPos)
            {
                _lastCellWorldPos = cellWorldPos;
                EventBus<StartPreviewEvent>.Raise(new StartPreviewEvent() { prefabs = hologramPrefab, previewState = previewState });
                EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { Position = cellWorldPos, IsValid = _isValid, YRotation = yRotation });
            }
        }
        else
        {
            EndPreviewNow();
        }
    }

    /// <summary>
    /// Multi-tile preview calculation loop using pre-allocated zero-GC buffers and ToolAreaCalculator.
    /// </summary>
    protected void RunMultiPreviewUpdate(
        int range,
        GameObject hologramPrefab,
        PreviewState previewState,
        GridCheck check,
        float yRotation)
    {
        _ray = RayCastAtCursor();
        if (Physics.Raycast(_ray, out _hit, range))
        {
            Vector3Int centerCell = grid.WorldToCell(_hit.point);
            Vector3 lookDir = sceneCamera != null ? sceneCamera.transform.forward : Vector3.forward;
            Vector3Int cardinalDir = ToolAreaCalculator.GetCardinalFacingDirection(lookDir);

            _cachedPatternCellCount = ToolAreaCalculator.CalculatePatternCells(
                centerCell,
                cardinalDir,
                toolData.patternShape,
                toolData.patternDimension,
                _cachedPatternCells
            );

            bool hasAnyValid = false;
            for (int i = 0; i < _cachedPatternCellCount; i++)
            {
                Vector3 cellWorldPos = grid.GetCellCenterWorld(_cachedPatternCells[i]);
                bool isValid = check(cellWorldPos, out _);
                if (isValid) hasAnyValid = true;

                _cachedPreviewData[i] = new TilePreviewData
                {
                    Position = cellWorldPos,
                    IsValid = isValid
                };
            }

            lastCheckWasValid = hasAnyValid;
            _lastCellWorldPos = grid.GetCellCenterWorld(centerCell);

            EventBus<StartPreviewEvent>.Raise(new StartPreviewEvent { prefabs = hologramPrefab, previewState = previewState });
            EventBus<MultiPreviewingEvent>.Raise(new MultiPreviewingEvent
            {
                PreviewTiles = _cachedPreviewData,
                TileCount = _cachedPatternCellCount,
                YRotation = yRotation
            });
        }
        else
        {
            EndPreviewNow();
        }
    }

    protected void EndPreviewNow()
    {
        _lastCellWorldPos = Vector3.zero;
        EventBus<EndPreviewEvent>.Raise(new EndPreviewEvent() { });
    }
}
