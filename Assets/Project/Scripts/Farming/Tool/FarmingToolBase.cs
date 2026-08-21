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

    private Camera sceneCamera;
    private Mouse currentMouse;
    protected IFarmingGrid grid;

    protected Ray _ray;
    protected RaycastHit _hit;
    protected bool _isHit;
    protected Vector3 _lastCellWorldPos;
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
    protected void RunPreviewUpdate(int range, GameObject hologramPrefab, PreviewState previewState, GridCheck check, float yRotation)
    {
        _ray = RayCastAtCursor();
        if (Physics.Raycast(_ray, out _hit, range))
        {
            _isHit = true;
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
            _isHit = false;
            EndPreviewNow();
        }
    }

    protected void EndPreviewNow()
    {
        _lastCellWorldPos = Vector3.zero;
        EventBus<EndPreviewEvent>.Raise(new EndPreviewEvent() { });
    }
}
