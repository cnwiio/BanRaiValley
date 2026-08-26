using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float range;

    private const float INTERACT_INTERVAL = 0.1f;
    private float interactTimer;
    
    private Camera _camera;
    private Mouse _mouse;
    private Vector3 _mousePos;

    private Ray _ray;
    private RaycastHit _hit;
    private IInteractable _target;

    private void Awake()
    {
        _camera = Camera.main;
        _mouse = Mouse.current;
    }

    private void OnEnable()
    {
        EventBus<OnInteractActionEvent>.Subscribe(OnInteractAction);
    }

    private void OnDisable()
    {
        EventBus<OnInteractActionEvent>.Unsubscribe(OnInteractAction);
    }

    private void OnInteractAction(OnInteractActionEvent evt)
    {
        _target?.Interact();
    }
    
    protected Ray RayCastAtCursor()
    {
        _mousePos = _mouse.position.ReadValue();
        _mousePos.z = _camera.nearClipPlane;
        return _camera.ScreenPointToRay(_mousePos);
    }
    
    private void Update()
    {
        // interactTimer += Time.deltaTime;
        // if (interactTimer <= INTERACT_INTERVAL) return;
        // interactTimer = 0;
        
        _ray = RayCastAtCursor();
        if (Physics.Raycast(_ray, out _hit, range, layerMask) &&
            _hit.collider.TryGetComponent<IInteractable>(out var interactable))
        {
            if (!ReferenceEquals(interactable, _target))
            {
                _target = interactable;
                _target.IsLookAt(true);
            }
        } 
        else if (!ReferenceEquals(_target, null))
        {
            _target.IsLookAt(false);
            _target = null;
        }
    }
    
}
