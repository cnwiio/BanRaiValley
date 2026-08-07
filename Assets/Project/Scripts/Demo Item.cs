using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum HoeState
{
    Idle,
    Farming
}


public class DemoItem : MonoBehaviour
{
    private Camera sceneCamera;
    private HoeState currentState = HoeState.Idle;
    public HoeState CurrentState
    {
        get => currentState;
        set
        {
            currentState = value;
            //switch (value)
            //{
            //    case HoeState.Idle:
            //        PreviewPrefab.SetActive(false);
            //        break;
            //    case HoeState.Farming:
            //        PreviewPrefab.SetActive(true);
            //        break;
            //}
        }
    }

    private Coroutine sendRayCoroutine;
    private int BuildDistance = 10;

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
        EventBus<OnAction1Event>.Subscribe(OnAction1);
        EventBus<OnAction2Event>.Subscribe(OnAction2);
    }

    private void OnDisable()
    {
        EventBus<OnAction1Event>.Unsubscribe(OnAction1);
        EventBus<OnAction2Event>.Unsubscribe(OnAction2);
    }

    void OnAction1(OnAction1Event evt)
    {
        Action1();
    }
    void OnAction2(OnAction2Event evt)
    {
        Action2();
    }

    void Action1()
    {
        Debug.Log("Action 1");

        //if (currentState == HoeState.Farming)
        //{
        //    RayCastAtCursor(_ray);
        //    if (Physics.Raycast(_ray, out _hit, 100))
        //    {
        //        EventBus<OnHoeDoAction1Event>.Raise(new OnHoeDoAction1Event { Position = _hit.point });
        //    }
        //}
    }

    void Action2()
    {
        Debug.Log("Action 2");
        if (currentState != HoeState.Farming)
        {
            currentState = HoeState.Farming;
            //sendRayCoroutine = StartCoroutine(SendRayCoroutine());
        }
        else
        {
            currentState = HoeState.Idle;
            //StopCoroutine(sendRayCoroutine);
        }
    }

    private IEnumerator SendRayCoroutine()
    {
        while (currentState == HoeState.Farming)
        {
            _ray = RayCastAtCursor();
            if (Physics.Raycast(_ray, out _hit, 100))
            {
                //Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 2f);

                EventBus<OnHoeFarmingMode>.Raise(new OnHoeFarmingMode { Position = _hit.point });
            }
            yield return null;
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
        if (currentState == HoeState.Farming)
        {
            _ray = RayCastAtCursor();
            if (Physics.Raycast(_ray, out _hit, 10))
            {
                EventBus<OnHoeFarmingMode>.Raise(new OnHoeFarmingMode { Position = _hit.point });
            }
        }
    }
}
