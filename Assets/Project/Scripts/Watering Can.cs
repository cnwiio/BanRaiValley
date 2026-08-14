using UnityEngine;
using UnityEngine.InputSystem;

public enum WaterCanState
{
    Idle,
    Farm,
    Watering
}
public class WateringCan : MonoBehaviour
{
    [SerializeField] private Animator wateringCanAnimator;
    [SerializeField] private GameObject HologramPrefabs;
    [Header("Grid link - same asset must be assigned on the FarmingGrid")]
    [SerializeField] private FarmingGridReference farmingGridReference;
    private WaterCanState _currentState = WaterCanState.Idle;
    private WaterCanState CurrentState
    {
        get => _currentState;
        set
        {
            if (_currentState != value)
            {
                // on exit state
                switch (_currentState)
                {
                    case WaterCanState.Idle:
                        break;
                    case WaterCanState.Farm:
                        _lastCellWorldPos = Vector3.zero;
                        EventBus<EndPreviewEvent>.Raise(new EndPreviewEvent() { });
                        break;
                }
                // on enter state
                switch (value)
                {
                    case WaterCanState.Idle:
                        break;
                    case WaterCanState.Farm:
                        _lastCellWorldPos = Vector3.zero;
                        break;
                }
                _currentState = value;
            }
        }
    }


    private Camera sceneCamera;
    private Vector3 mousePos;
    private Mouse currentMouse;
    private int WateringCanRange = 10;
    private IFarmingGrid grid;

    // cached
    private Ray _ray;
    private RaycastHit _hit;
    private Vector3 _lastCellWorldPos;
    private void Awake()
    {
        sceneCamera = Camera.main;
        currentMouse = Mouse.current;
        grid = farmingGridReference.Grid;
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

        CurrentState = WaterCanState.Idle;
    }

    bool _isWaterable = false;
    private void OnPrimaryAction(OnPrimaryActionEvent evt)
    {
        PrimaryAction();
    }

    private void OnSecondaryAction(OnSecondaryActionEvent evt)
    {
        SecondaryAction();
    }

    private Ray RayCastAtCursor()
    {
        mousePos = currentMouse.position.ReadValue();
        mousePos.z = sceneCamera.nearClipPlane;
        return sceneCamera.ScreenPointToRay(mousePos);
    }

    private void PrimaryAction()
    {
        
    }

    private void SecondaryAction()
    {
        CurrentState = CurrentState == WaterCanState.Farm ? WaterCanState.Idle : WaterCanState.Farm;
    }

    private void Update()
    {
        //Debug.Log(CurrentState);
        if (CurrentState == WaterCanState.Farm)
        {
            _ray = RayCastAtCursor();
            if (Physics.Raycast(_ray, out _hit, WateringCanRange))
            {
                _isWaterable = grid.IsWaterable(_hit.point, out var cellWorldPos);
                //Debug.Log(_isWaterable);
                if (_lastCellWorldPos != cellWorldPos)
                {
                    _lastCellWorldPos = cellWorldPos;
                    EventBus<StartPreviewEvent>.Raise(new StartPreviewEvent() { prefabs = HologramPrefabs, previewState = PreviewState.Watering });
                    EventBus<PreviewingEvent>.Raise(new PreviewingEvent() { Position = cellWorldPos, IsValid = _isWaterable, YRotation = 0 });
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
