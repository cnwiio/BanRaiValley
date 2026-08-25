using NUnit.Framework.Internal.Commands;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera PlayerCam;
    // [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private CinemachinePanTilt panTilt;
    [SerializeField] private PlayerMovementData Data;
    [SerializeField] private PlayerInputReader InputReader;
    [SerializeField] private Transform Head;

    private Vector3 movementInput;
    private Vector3 horizontalDirection;
    private Vector3 horizontalMovement;
    private float VerticalMovement;
    private Vector3 FinalMovement;
    private bool JumpTriggered = false;

    private Transform _camTransform;
    private Vector3 CamForwardDirection;
    private Vector3 CamRightDirection;

    #region Handle Input

    private void OnEnable()
    {
        EventBus<OnJumpEvent>.Subscribe(HandleJumpInput);
        EventBus<OnRequestTeleportEvent>.Subscribe(OnRequestTeleport);
    }

    private void OnDisable()
    {
        EventBus<OnJumpEvent>.Unsubscribe(HandleJumpInput);
        EventBus<OnRequestTeleportEvent>.Unsubscribe(OnRequestTeleport);
    }

    public void HandleJumpInput(OnJumpEvent ctx)
    {
        if (characterController.isGrounded)
        {
            JumpTriggered = true;
        }
    }

    public void OnRequestTeleport(OnRequestTeleportEvent evt)
    {
        TeleportTo(evt.AwakeTransform);
    }
    #endregion

    private void Awake()
    {
        // cached
        _camTransform = PlayerCam.transform;
        Head.rotation = _camTransform.rotation;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        CalculateRotation();
        UpdateCamRotateToPlayer();
    }

    // Update is called once per frame
    void FixedUpdate()
    { 
        HandleVerticalMovement();
        HandleHorizontalMovement();
        ApplyMovement();
    }

    #region Handle Movement
    void HandleHorizontalMovement()
    {
        movementInput = InputReader.MovementInput;
        horizontalDirection = ((CamForwardDirection * movementInput.y) + (CamRightDirection * movementInput.x)).normalized;
        horizontalMovement = horizontalDirection * Data.Speed;
    }

    void HandleVerticalMovement()
    {
        if (characterController.isGrounded)
        {
            if (characterController.velocity.y < 0)
            {
                VerticalMovement = Data.GroundSnapForce; 
            }

            if (JumpTriggered)
            {
                VerticalMovement = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * Data.JumpHeight);
                JumpTriggered = false;
            }
        }
        else
        {
            VerticalMovement += Physics.gravity.y * Data.GravityMultiplyer;
        }
    }

    void ApplyMovement()
    {
        FinalMovement = horizontalMovement;
        FinalMovement.y = VerticalMovement;

        characterController.Move(FinalMovement * Time.fixedDeltaTime);
    }
    #endregion

    void CalculateRotation()
    {
        CamForwardDirection = _camTransform.forward;
        CamForwardDirection.y = 0;
        CamForwardDirection.Normalize();

        CamRightDirection = _camTransform.right;
        CamRightDirection.y = 0;
        CamRightDirection.Normalize();
    }

    void UpdateCamRotateToPlayer()
    {
        Head.rotation = _camTransform.rotation;
    }

    // Cached
    private Vector3 _euler;
    void TeleportTo(Transform targetTransform)
    {
        characterController.enabled = false;
        transform.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);
        VerticalMovement = 0f;
        characterController.enabled = true;

        Head.rotation = targetTransform.rotation;

        // รีเซ็ตค่ามุมที่ Cinemachine จำไว้
        _euler = targetTransform.eulerAngles;
        panTilt.PanAxis.Value = _euler.y;
        panTilt.TiltAxis.Value = _euler.x;
    }
}
