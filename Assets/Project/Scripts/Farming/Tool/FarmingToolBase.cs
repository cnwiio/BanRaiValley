using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Shared skeleton for any first-person tool that aims at the FarmingGrid and shows
/// a hologram preview (Hoe, WateringCan, and future tools like Fertilizer/Seeder).
///
/// What lives here vs. what stays in the subclass:
/// - HERE: camera/mouse setup, raycasting, lazy grid resolution, the
///   raycast->check->preview-event loop (RunPreviewUpdate), primary/secondary
///   input wiring.
/// - SUBCLASS: its own state machine (Hoe has 4 states, WateringCan has fewer),
///   which grid check + prefab + PreviewState to use, and what Primary/Secondary
///   actually do.
///
/// Adding a new tool no longer means copy-pasting the raycast/preview block -
/// just inherit this and call RunPreviewUpdate with your own arguments.
/// </summary>
public abstract class FarmingToolBase : MonoBehaviour
{
    [SerializeField] protected FarmingGridReference farmingGridReference;
    [SerializeField] protected ToolData toolData;
       
    protected int _cachedPatternCellCount;
    protected const int MAX_TOOL_CELLS = 49;
    protected readonly Vector3Int[] _cachedPatternCells = new Vector3Int[MAX_TOOL_CELLS];
    protected readonly PreviewTileData[] _cachedPreviewData  = new PreviewTileData[MAX_TOOL_CELLS];

    protected Camera sceneCamera;
    private Mouse currentMouse;
    protected IFarmingGrid grid;
    private LayerMask raycastTargetLayer;

    protected Ray _ray;
    protected RaycastHit _hit;
    protected bool _isHit;
    protected Vector3 _lastCellWorldPos;
    private Vector3Int _lastCardinalDir;
    /// <summary>Result of the most recent RunPreviewUpdate check - handy for handlers
    /// (like a rotate action) that need "is the currently-aimed cell valid?" without
    /// re-running the grid check themselves.</summary>
    protected bool lastCheckWasValid;

    /// <summary>Matches the signature of IFarmingGrid.IsValidForTilling / IsTilled / IsWaterable / etc.</summary>
    public delegate bool GridCheck(Vector3 worldPos, out Vector3 cellWorldPos);

    protected virtual void Awake()
    {
        sceneCamera = Camera.main;
        currentMouse = Mouse.current;
        raycastTargetLayer = LayerMask.GetMask("Ground");
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
    /// The raycast -> validity-check -> preview-event loop shared by every tool.
    /// `check` is whichever IFarmingGrid method decides validity for this tool
    /// (grid.IsValidForTilling, grid.IsTilled, grid.IsWaterable, ...).
    /// </summary>
    bool _isValid;
    protected void RunPreviewUpdate(
        int range, 
        GameObject hologramPrefab, 
        PreviewState previewState, 
        GridCheck check, 
        float yRotation)
    {
        _ray = RayCastAtCursor();
        if (Physics.Raycast(_ray, out _hit, range, raycastTargetLayer))
        {
            _isHit = true;
            _isValid = check(_hit.point, out var cellWorldPos);
            lastCheckWasValid = _isValid;
            if (_lastCellWorldPos != cellWorldPos)
            {
                _lastCellWorldPos = cellWorldPos;
                _cachedPatternCellCount = 1;
                _cachedPreviewData[0] = new PreviewTileData()
                {
                    Position = cellWorldPos,
                    IsValid = _isValid
                };
                EventBus<StartPreviewEvent>.Raise(new StartPreviewEvent() { prefabs = hologramPrefab, previewState = previewState, TileCount = _cachedPatternCellCount});
                EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { PreviewTileData = _cachedPreviewData, YRotation = yRotation });
            }
        }
        else
        {
            _isHit = false;
            EndPreviewNow();
        }
    }
    
    protected void RunMultiPreviewUpdate(
        int range, 
        GameObject hologramPrefab, 
        PreviewState previewState,
        GridCheck check, 
        float yRotation)
    {
        _ray = RayCastAtCursor();
        if (Physics.Raycast(_ray, out _hit, range, raycastTargetLayer))
        {
            _isHit = true;
            Vector3Int centerCell = grid.WorldToCell(_hit.point);
            var raycastCellPos = grid.GetCellCenterWorld(centerCell);
            Vector3 lookDir = sceneCamera.transform.forward;
            Vector3Int cardinalDir = ToolAreaCalculator.GetCardinalFacingDirection(lookDir);
            if (_lastCellWorldPos != raycastCellPos || cardinalDir != _lastCardinalDir)
            {
                _lastCellWorldPos = raycastCellPos;
                _lastCardinalDir = cardinalDir;
                
                
                _cachedPatternCellCount = ToolAreaCalculator.CalculatePatternCells(
                    centerCell,
                    cardinalDir,
                    toolData.toolPattern,
                    toolData.patternDimension,
                    _cachedPatternCells
                );
                
                for (int i = 0; i < _cachedPatternCellCount; i++)
                {
                    Vector3 cachedCellWorldPos = grid.GetCellCenterWorld(_cachedPatternCells[i]);
                    bool isValid = check(cachedCellWorldPos, out _);
                    // if (isValid) lastCheckWasValid = true;

                    _cachedPreviewData[i] = new PreviewTileData()
                    {
                        Position = cachedCellWorldPos,
                        IsValid = isValid
                    };
                }
                EventBus<StartPreviewEvent>.Raise(new StartPreviewEvent() 
                { 
                    prefabs = hologramPrefab,
                    previewState = previewState,
                    TileCount = _cachedPatternCellCount
                });
                EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { PreviewTileData = _cachedPreviewData, YRotation = yRotation });
            }
        }
        else
        {
            _isHit = false;
            EndPreviewNow();
        }
    }

    protected void EndPreviewNow()
    {
        _lastCellWorldPos = Vector3.zero;
        _lastCardinalDir = Vector3Int.zero;
        EventBus<EndPreviewEvent>.Raise(new EndPreviewEvent() { });
    }
}
