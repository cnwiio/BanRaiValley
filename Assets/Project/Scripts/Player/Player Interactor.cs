using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float range;
    [SerializeField] private InputManager inputManager;

    private const float INTERACT_INTERVAL = 0.15f;
    private float _interactTimer;
    private bool _isInteractHeld;

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
        EventBus<OnInteractActionStartedEvent>.Subscribe(OnInteractActionStarted);
        EventBus<OnInteractActionCanceledEvent>.Subscribe(OnInteractActionCanceled);
    }

    private void OnDisable()
    {
        EventBus<OnInteractActionEvent>.Unsubscribe(OnInteractAction);
        EventBus<OnInteractActionStartedEvent>.Unsubscribe(OnInteractActionStarted);
        EventBus<OnInteractActionCanceledEvent>.Unsubscribe(OnInteractActionCanceled);

        _isInteractHeld = false;
        _interactTimer = 0f;
    }

    private void OnInteractActionStarted(OnInteractActionStartedEvent evt)
    {
        _isInteractHeld = true;
    }

    private void OnInteractAction(OnInteractActionEvent evt)
    {
        _target?.Interact();
        _interactTimer = 0f;
    }

    private void OnInteractActionCanceled(OnInteractActionCanceledEvent evt)
    {
        _isInteractHeld = false;
        _interactTimer = 0f;
    }

    private Ray RayCastAtCursor()
    {
        _mousePos = _mouse.position.ReadValue();
        _mousePos.z = _camera.nearClipPlane;
        return _camera.ScreenPointToRay(_mousePos);
    }

    private void Update()
    {
        if (inputManager.CurrentActionMaptype != ActionMapType.Player) return;

        _ray = RayCastAtCursor();
        if (Physics.Raycast(_ray, out _hit, range, layerMask) &&
            _hit.collider.TryGetComponent<IInteractable>(out var interactable))
        {
            if (ReferenceEquals(interactable, _target))
            {
                if (_isInteractHeld && _target != null)
                {
                    _interactTimer += Time.deltaTime;
                    if (_interactTimer >= INTERACT_INTERVAL)
                    {
                        _interactTimer = 0f;
                        _target.Interact();
                    }
                }
                return;
            }

            if (!ReferenceEquals(_target, null))
            {
                _target.IsLookAt(false);
                _target = null;
            }

            _target = interactable;
            _target.IsLookAt(true);

            if (_isInteractHeld && _target != null)
            {
                _interactTimer = 0f;
                _target.Interact();
            }
        }
        else if (!ReferenceEquals(_target, null))
        {
            _target.IsLookAt(false);
            _target = null;
            _interactTimer = 0f;
        }
    }
}
