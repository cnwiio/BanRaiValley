using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum HoeState
{
    Idle,
    Farming,
    Tilling
}


public class DemoItem : MonoBehaviour
{
    [SerializeField] private GameObject dirtHologramPrefabs;

    private Camera sceneCamera;
    private HoeState currentState = HoeState.Idle;
    public HoeState CurrentState
    {
        get => currentState;
        set
        {
            // on exit state
            if (currentState != value)
            {
                switch (currentState)
                {
                    case HoeState.Idle:
                        break;
                    case HoeState.Farming:
                        EventBus<EndPreviewEvent>.Raise(new EndPreviewEvent() { });
                        break;
                }
            }
            currentState = value;
        }
    }

    private int HoeRange = 10;

    // cached
    private Vector3 _mousePos;
    private Mouse _currentMouse;
    private Ray _ray;
    private RaycastHit _hit;
    private void Awake()
    {
        sceneCamera = Camera.main;
        _currentMouse = Mouse.current;
    }

    private void OnEnable()
    {
        EventBus<OnPrimaryActionEvent>.Subscribe(OnPrimaryAction);
        EventBus<OnSecondaryActionEvent>.Subscribe(OnSecondaryAction);
    }

    private void OnDisable()
    {
        EventBus<OnPrimaryActionEvent>.Unsubscribe(OnPrimaryAction);
        EventBus<OnSecondaryActionEvent>.Unsubscribe(OnSecondaryAction);
    }

    void OnPrimaryAction(OnPrimaryActionEvent evt)
    {
        PrimaryAction();
    }
    void OnSecondaryAction(OnSecondaryActionEvent evt)
    {
        SecondaryAction();
    }

    void PrimaryAction()
    {
        Debug.Log("Action 1");

    }

    void SecondaryAction()
    {
        Debug.Log("Action 2");
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

    void Update()
    {
        if (CurrentState == HoeState.Farming)
        {
            _ray = RayCastAtCursor();
            if (Physics.Raycast(_ray, out _hit, HoeRange))
            {
                EventBus<StartPreviewEvent>.Raise(new StartPreviewEvent() { prefabs = dirtHologramPrefabs });
                EventBus<OnHoeRaycastEvent>.Raise(new OnHoeRaycastEvent { Position = _hit.point , IsHit = true});
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