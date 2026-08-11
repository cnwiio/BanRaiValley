using Lean.Pool;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
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
                    case HoeState.Deleting:
                            EventBus<EndPreviewEvent>.Raise(new EndPreviewEvent() { });
                        break;
                }
            }
            _currentState = value;
            //Debug.Log(_currentState);
        }
    }

    private int HoeRange = 10;
    private float currentYRotate;
    //private LayerMask raycastLayerMask;
    //private System.Collections.Generic.List<GameObject> spawnedPrefabsList;
    private Dictionary<Vector3, GameObject> _spawnedPrefabs = new Dictionary<Vector3, GameObject>();

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
        EventBus<OnDeleteActionEvent>.Subscribe(OnDeleteAction);
        EventBus<OnTiledGridEvent>.Subscribe(OnTileGrid);
    }

    private void OnDisable()
    {
        EventBus<OnPrimaryActionEvent>.Unsubscribe(OnPrimaryAction);
        EventBus<OnSecondaryActionEvent>.Unsubscribe(OnSecondaryAction);
        EventBus<ChangeActionMap>.Unsubscribe(OnChangeActionMap);
        EventBus<OnValidGridEvent>.Unsubscribe(OnValidGrid);
        EventBus<OnDeleteActionEvent>.Unsubscribe(OnDeleteAction);
        EventBus<OnTiledGridEvent>.Unsubscribe(OnTileGrid);

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

    void OnTileGrid(OnTiledGridEvent evt)
    {
        _dirtPos = evt.Position;
        DeleteTile(_dirtPos);
    }

    void OnRotateAction(OnRotateActionEvent evt)
    {
        if (CurrentState == HoeState.Farming)
        {
            currentYRotate += 90;
            EventBus<OnRotateFarmEvent>.Raise(new OnRotateFarmEvent() { YRotation = currentYRotate }); 
        }
    }

    void OnDeleteAction(OnDeleteActionEvent evt)
    {
        if (CurrentState != HoeState.Tilling)
            CurrentState = CurrentState == HoeState.Deleting ? HoeState.Idle : HoeState.Deleting;
    }


    private void StartTilling()
    {
        CurrentState = HoeState.Tilling;
        hoeAnimator.SetTrigger("Tilling");
    }

    private void DeleteTile(Vector3 pos)
    {
        LeanPool.Despawn(GetSpawnedPrefab(pos));
    }
    void PrimaryAction()
    {
        //Debug.Log("Action 1");
        if (CurrentState == HoeState.Farming)
        {
            EventBus<OnHoeTillingEvent>.Raise(new OnHoeTillingEvent() { });
        } else if (CurrentState == HoeState.Deleting)
        {
            EventBus<OnHoeDeletingEvent>.Raise(new OnHoeDeletingEvent() { });
        }
    }

    void SecondaryAction()
    {
        //Debug.Log("Action 2");
        if (CurrentState != HoeState.Tilling)
            CurrentState = CurrentState == HoeState.Farming ? HoeState.Idle : HoeState.Farming;
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
        RegisterSpawnedPrefabs(
            _dirtPos,
            LeanPool.Spawn(dirtPrefabs, _dirtPos, Quaternion.Euler(0, currentYRotate, 0))
            );
        CurrentState = HoeState.Farming;
    }

    private void RegisterSpawnedPrefabs(Vector3 pos, GameObject prefabs)
    {
        _spawnedPrefabs[pos] = prefabs;
    }

    private GameObject GetSpawnedPrefab(Vector3 pos)
    {
        return _spawnedPrefabs[pos];
    }

    void Update()
    {
        if (CurrentState == HoeState.Farming)
        {
            _ray = RayCastAtCursor();
            if (Physics.Raycast(_ray, out _hit, HoeRange))
            {
                EventBus<StartPreviewEvent>.Raise(new StartPreviewEvent() { prefabs = dirtHologramPrefabs , previewState = PreviewState.Build});
                EventBus<OnHoeRaycastEvent>.Raise(new OnHoeRaycastEvent { Position = _hit.point , IsHit = true, PreviewState = PreviewState.Build });
            } 
            else
            {
                EventBus<OnHoeRaycastEvent>.Raise(new OnHoeRaycastEvent { IsHit = false });
                EventBus<EndPreviewEvent>.Raise(new EndPreviewEvent() { });
            }
        }
        else if (CurrentState == HoeState.Deleting)
        {
            _ray = RayCastAtCursor();
            if (Physics.Raycast(_ray, out _hit, HoeRange))
            {
                EventBus<StartPreviewEvent>.Raise(new StartPreviewEvent() { prefabs = deleteHologramPrefabs , previewState = PreviewState.Delete});
                EventBus<OnHoeRaycastEvent>.Raise(new OnHoeRaycastEvent { Position = _hit.point , IsHit = true, PreviewState = PreviewState.Delete});
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