using Lean.Pool;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.InputSystem;
using static FarmingGrid;

public enum HoeState
{
    Idle,
    Farming,
    Tilling,
    Deleting
}


public class Hoe : MonoBehaviour
{
    [SerializeField] private GameObject dirtHologramPrefabs;
    [SerializeField] private GameObject deleteHologramPrefabs;
    [SerializeField] private GameObject dirtPrefabs;
    [SerializeField] private Animator hoeAnimator;

    [Header("Grid link - same asset must be assigned on the FarmingGrid")]
    [SerializeField] private FarmingGridReference farmingGridReference;

    private Camera sceneCamera;
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
                        {
                            _lastCellWorldPos = Vector3.zero;
                            EventBus<EndPreviewEvent>.Raise(new EndPreviewEvent() { });
                        }
                        break;
                    case HoeState.Tilling:
                        _lastCellWorldPos = Vector3.zero;
                        EventBus<EndPreviewEvent>.Raise(new EndPreviewEvent() { });
                        break;
                    case HoeState.Deleting:
                        _lastCellWorldPos = Vector3.zero;
                        EventBus<EndPreviewEvent>.Raise(new EndPreviewEvent() { });
                        break;
                }
                // on enter state
                switch (value)
                {
                    case HoeState.Farming:
                        _lastCellWorldPos = Vector3.zero;
                        break;
                    case HoeState.Tilling:
                        EventBus<OnStartTillingEvent>.Raise(new OnStartTillingEvent() { });
                        break;
                    case HoeState.Deleting:
                        _lastCellWorldPos = Vector3.zero;
                        break;
                }
            }
            
            _currentState = value;

        }
    }

    private int HoeRange = 10;
    private float currentYRotate;
    private IFarmingGrid grid;

    // cached
    private Vector3 _mousePos;
    private Mouse _currentMouse;
    private Ray _ray;
    private RaycastHit _hit;
    private Vector3 _dirtPos;

    private void Awake()
    {
        sceneCamera = Camera.main;
        _currentMouse = Mouse.current;
        grid = farmingGridReference.Grid;
    }

    private void OnEnable()
    {
        EventBus<OnPrimaryActionEvent>.Subscribe(OnPrimaryAction);
        EventBus<OnSecondaryActionEvent>.Subscribe(OnSecondaryAction);
        EventBus<ChangeActionMap>.Subscribe(OnChangeActionMap);
        EventBus<OnRotateActionEvent>.Subscribe(OnRotateAction);
        EventBus<OnDeleteActionEvent>.Subscribe(OnDeleteAction);
    }

    private void OnDisable()
    {
        EventBus<OnPrimaryActionEvent>.Unsubscribe(OnPrimaryAction);
        EventBus<OnSecondaryActionEvent>.Unsubscribe(OnSecondaryAction);
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

    void OnPrimaryAction(OnPrimaryActionEvent evt) => PrimaryAction();
    void OnSecondaryAction(OnSecondaryActionEvent evt) => SecondaryAction();

    void OnRotateAction(OnRotateActionEvent evt)
    {
        if (CurrentState == HoeState.Farming)
        {
            currentYRotate += 90;
            EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { Position = _lastCellWorldPos, IsValid = _isValid, YRotation = currentYRotate });
        }
    }

    void OnDeleteAction(OnDeleteActionEvent evt)
    {
        if (CurrentState != HoeState.Tilling)
            CurrentState = CurrentState == HoeState.Deleting ? HoeState.Idle : HoeState.Deleting;
    }

    void PrimaryAction()
    {
        if (CurrentState == HoeState.Farming)
        {
            if (grid.IsValidForTilling(_hit.point, out var cellWorldPos))
            {
                _dirtPos = cellWorldPos;
                StartTilling();
            }
        }
        else if (CurrentState == HoeState.Deleting)
        {
            if (grid.IsTilled(_hit.point, out var cellWorldPos))
            {
                _dirtPos = cellWorldPos;
                DeleteTile(cellWorldPos);
            }
        }
    }

    void SecondaryAction()
    {
        if (CurrentState != HoeState.Tilling)
            CurrentState = CurrentState == HoeState.Farming ? HoeState.Idle : HoeState.Farming;
    }

    private Ray RayCastAtCursor()
    {
        _mousePos = _currentMouse.position.ReadValue();
        _mousePos.z = sceneCamera.nearClipPlane;
        return sceneCamera.ScreenPointToRay(_mousePos);
    }
    private void StartTilling()
    {
        CurrentState = HoeState.Tilling;
        hoeAnimator.SetTrigger("Tilling");
    }

    private void DeleteTile(Vector3 pos)
    {
        grid.TryUntill(_dirtPos, out var cellPos);
        EventBus<OnTileClearEvent>.Raise(new OnTileClearEvent() { CellPos = cellPos });
        EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { Position = _lastCellWorldPos, IsValid = _isValid, YRotation = currentYRotate });
    }


    public void OnTillingAnimationFinish()
    {
        grid.TryTill(_dirtPos, out var cellPos);
        EventBus<OnTillingImpactEvent>.Raise(new OnTillingImpactEvent() { prefabs = dirtPrefabs, Position = _dirtPos, YRotation = currentYRotate , CellPos = cellPos });
        CurrentState = HoeState.Farming;
    }

    // cached 
    bool _isValid;
    bool _isTilled;
    Vector3 _lastCellWorldPos;
    void Update()
    {
        // Preview events (StartPreviewEvent/PreviewingEvent/EndPreviewEvent) are kept
        // as broadcasts on purpose: several unrelated systems (hologram, VFX, audio)
        // may want to react to "player is aiming at a valid/invalid tile" without
        // Hoe or FarmingGrid needing to know about any of them.
        if (CurrentState == HoeState.Farming)
        {
            _ray = RayCastAtCursor();
            if (Physics.Raycast(_ray, out _hit, HoeRange))
            {
                _isValid = grid.IsValidForTilling(_hit.point, out var cellWorldPos);
                if (_lastCellWorldPos != cellWorldPos)
                {
                    _lastCellWorldPos = cellWorldPos;
                    EventBus<StartPreviewEvent>.Raise(new StartPreviewEvent() { prefabs = dirtHologramPrefabs, previewState = PreviewState.Build });
                    EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { Position = cellWorldPos, IsValid = _isValid, YRotation = currentYRotate });
                }
            }
            else
            {
                _lastCellWorldPos = Vector3.zero;
                EventBus<EndPreviewEvent>.Raise(new EndPreviewEvent() { });
            }
        }
        else if (CurrentState == HoeState.Deleting)
        {
            _ray = RayCastAtCursor();
            if (Physics.Raycast(_ray, out _hit, HoeRange))
            {
                _isTilled = grid.IsTilled(_hit.point, out var cellWorldPos);
                if (_lastCellWorldPos != cellWorldPos)
                {
                    _lastCellWorldPos = cellWorldPos;
                    EventBus<StartPreviewEvent>.Raise(new StartPreviewEvent() { prefabs = deleteHologramPrefabs, previewState = PreviewState.Delete });
                    EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { Position = cellWorldPos, IsValid = _isTilled, YRotation = currentYRotate }); 
                }
            }
            else
            {
                _lastCellWorldPos = Vector3.zero;
                EventBus<EndPreviewEvent>.Raise(new EndPreviewEvent() { });
            }
        }
    }
}




















//if (currentState == HoeState.Farming)
//{
//    RayCastAtCursor(_ray);
//    if (Physics.Raycast(_ray, out _hit, 100))
//    {
//        EventBus<OnHoePrimaryActionEvent>.Raise(new OnHoePrimaryActionEvent { Position = _hit.point });
//    }
//}





//private IEnumerator SendRayCoroutine()
//{
//    while (currentState == HoeState.Farming)
//    {
//        _ray = RayCastAtCursor();
//        if (Physics.Raycast(_ray, out _hit, 100))
//        {
//            //Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 2f);

//            EventBus<OnHoeRaycastEvent>.Raise(new OnHoeRaycastEvent { Position = _hit.point });
//        }
//        yield return null;
//    }
//}