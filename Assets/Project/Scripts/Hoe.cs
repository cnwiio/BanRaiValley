using Lean.Pool;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum HoeState
{
    Idle,
    Farming,
    Tilling
}


public class Hoe : MonoBehaviour
{
    [SerializeField] private GameObject dirtHologramPrefabs;
    [SerializeField] private GameObject dirtPrefabs;
    [SerializeField] private Animator hoeAnimator;

    private Camera sceneCamera;
    private HoeState _currentState = HoeState.Idle;
    public HoeState CurrentState
    {
        get => _currentState;
        set
        {
            // on exit state
            if (_currentState != value)
            {
                switch (_currentState)
                {
                    case HoeState.Idle:
                        break;
                    case HoeState.Farming:
                        if (value != HoeState.Tilling)
                            EventBus<EndPreviewEvent>.Raise(new EndPreviewEvent() { });
                        break;
                }
            }
            _currentState = value;
        }
    }

    private int HoeRange = 10;
    private float currentYRotate;
    //private System.Collections.Generic.List<GameObject> spawnedPrefabsList;

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
    }

    private void OnEnable()
    {
        EventBus<OnPrimaryActionEvent>.Subscribe(OnPrimaryAction);
        EventBus<OnSecondaryActionEvent>.Subscribe(OnSecondaryAction);
        EventBus<ChangeActionMap>.Subscribe(OnChangeActionMap);
        EventBus<OnValidGridEvent>.Subscribe(OnValidGrid);
        EventBus<OnRotateActionEvent>.Subscribe(OnRotateAction);
    }

    private void OnDisable()
    {
        EventBus<OnPrimaryActionEvent>.Unsubscribe(OnPrimaryAction);
        EventBus<OnSecondaryActionEvent>.Unsubscribe(OnSecondaryAction);
        EventBus<ChangeActionMap>.Unsubscribe(OnChangeActionMap);
        EventBus<OnValidGridEvent>.Unsubscribe(OnValidGrid);
        EventBus<OnRotateActionEvent>.Unsubscribe(OnRotateAction);

        CurrentState = HoeState.Idle;
    }

    void OnChangeActionMap(ChangeActionMap evt)
    {
        if (evt.MapType != ActionMapType.Player)
        {
            CurrentState = HoeState.Idle;
        }
    }

    void OnPrimaryAction(OnPrimaryActionEvent evt)
    {
        PrimaryAction();
    }
    void OnSecondaryAction(OnSecondaryActionEvent evt)
    {
        SecondaryAction();
    }

    void OnValidGrid(OnValidGridEvent evt)
    {
        _dirtPos = evt.Position;
        StartTilling();
    }

    void OnRotateAction(OnRotateActionEvent evt)
    {
        if (CurrentState == HoeState.Farming)
        {
            currentYRotate += 90;
            EventBus<OnRotateFarmEvent>.Raise(new OnRotateFarmEvent() { YRotation = currentYRotate }); 
        }
    }

    void PrimaryAction()
    {
        //Debug.Log("Action 1");
        if (CurrentState == HoeState.Farming)
        {
            EventBus<OnHoeTillingEvent>.Raise(new OnHoeTillingEvent() { });
        }
    }

    private void StartTilling()
    {
        CurrentState = HoeState.Tilling;
        hoeAnimator.SetTrigger("Tilling");
    }

    void SecondaryAction()
    {
        //Debug.Log("Action 2");
        if (CurrentState != HoeState.Farming)
        {
            CurrentState = HoeState.Farming;
        }
        else
        {
            CurrentState = HoeState.Idle;
        }
    }


    private Ray RayCastAtCursor()
    {
        _mousePos = _currentMouse.position.ReadValue();
        _mousePos.z = sceneCamera.nearClipPlane;
        return sceneCamera.ScreenPointToRay(_mousePos);
    }

    public void OnTillingAnimationFinish()
    {
        //Debug.Log("Finish");
        LeanPool.Spawn(dirtPrefabs, _dirtPos, Quaternion.Euler(0, currentYRotate, 0));
        CurrentState = HoeState.Farming;
    }

    void Update()
    {
        if (CurrentState == HoeState.Farming)
        {
            _ray = RayCastAtCursor();
            if (Physics.Raycast(_ray, out _hit, HoeRange))
            {
                EventBus<StartPreviewEvent>.Raise(new StartPreviewEvent() { prefabs = dirtHologramPrefabs });
                EventBus<OnHoeRaycastEvent>.Raise(new OnHoeRaycastEvent { Position = _hit.point , IsHit = true, });
            } 
            else
            {
                EventBus<OnHoeRaycastEvent>.Raise(new OnHoeRaycastEvent { IsHit = false });
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