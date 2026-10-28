using Unity.Cinemachine;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Inspector Fields

    [Header("Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera PlayerCam;
    [SerializeField] private CinemachinePanTilt panTilt;
    [SerializeField] private PlayerMovementData Data;
    [SerializeField] private PlayerInputReader InputReader;
    [SerializeField] private Transform Head;

    [Header("Settings")]
    [SerializeField] private float turnSpeed = 15f;
    [SerializeField] private LayerMask ceilingCheckMask = ~(1 << 8 | 1 << 2);

    #endregion


    #region Public Properties

    public bool IsCrouching { get; private set; }
    public bool IsSprinting { get; private set; }

    #endregion


    #region Private Fields

    private Vector3 movementInput;
    private Vector3 horizontalDirection;
    private Vector3 horizontalMovement;
    private float VerticalMovement;
    private Vector3 FinalMovement;
    private bool JumpTriggered;

    private Transform _camTransform;
    private Vector3 CamForwardDirection;
    private Vector3 CamRightDirection;
    private Vector3 _euler;

    private float _standingHeight;
    private Vector3 _standingCenter;
    private Vector3 _standingHeadLocalPos;

    #endregion


    #region Unity Messages

    private void Awake()
    {
        _camTransform = PlayerCam.transform;
        Head.rotation = _camTransform.rotation;

        if (characterController != null)
        {
            _standingHeight = characterController.height;
            _standingCenter = characterController.center;
        }

        if (Head != null)
        {
            _standingHeadLocalPos = Head.localPosition;
        }
    }

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

    private void Update()
    {
        UpdateCrouchState();
        HandleVerticalMovement();
        HandleHorizontalMovement();
        CalculateRotation();
        ApplyMovement();
    }

    private void LateUpdate()
    {
        UpdateCamRotateToPlayer();
    }

    #endregion


    #region Public Methods

    public void HandleJumpInput(OnJumpEvent ctx)
    {
        if (characterController.isGrounded && !IsCrouching)
        {
            JumpTriggered = true;
        }
    }

    public void OnRequestTeleport(OnRequestTeleportEvent evt)
    {
        TeleportTo(evt.AwakeTransform);
    }

    #endregion


    #region Movement & Crouch Logic

    private void UpdateCrouchState()
    {
        bool wantsToCrouch = InputReader != null && InputReader.IsCrouchPressed;

        if (wantsToCrouch)
        {
            IsCrouching = true;
        }
        else if (IsCrouching)
        {
            // If releasing crouch, check if there is an obstacle above
            IsCrouching = HasCeilingObstacle();
        }

        float targetHeight = IsCrouching ? Data.CrouchHeight : _standingHeight;
        float targetCenterY = _standingCenter.y - (_standingHeight - targetHeight) * 0.5f;
        float targetHeadY = IsCrouching ? Data.CrouchHeadY : _standingHeadLocalPos.y;

        float transitionStep = 1f - Mathf.Exp(-Data.CrouchTransitionSpeed * Time.deltaTime);

        float newHeight = Mathf.Lerp(characterController.height, targetHeight, transitionStep);
        float newCenterY = Mathf.Lerp(characterController.center.y, targetCenterY, transitionStep);
        float newHeadY = Mathf.Lerp(Head.localPosition.y, targetHeadY, transitionStep);

        characterController.height = newHeight;
        characterController.center = new Vector3(_standingCenter.x, newCenterY, _standingCenter.z);
        Head.localPosition = new Vector3(_standingHeadLocalPos.x, newHeadY, _standingHeadLocalPos.z);
    }

    private bool HasCeilingObstacle()
    {
        float currentHeight = characterController.height;
        float heightDifference = _standingHeight - currentHeight;

        if (heightDifference <= 0.02f)
        {
            return false;
        }

        float radius = characterController.radius * 0.9f;
        Vector3 sphereOrigin = transform.position + Vector3.up * (characterController.center.y + (currentHeight * 0.5f) - radius);

        return Physics.SphereCast(sphereOrigin, radius, Vector3.up, out _, heightDifference, ceilingCheckMask, QueryTriggerInteraction.Ignore);
    }

    private void HandleHorizontalMovement()
    {
        movementInput = InputReader != null ? InputReader.MovementInput : Vector3.zero;
        horizontalDirection = ((CamForwardDirection * movementInput.y) + (CamRightDirection * movementInput.x)).normalized;

        float targetSpeed;
        if (IsCrouching)
        {
            IsSprinting = false;
            targetSpeed = Data.CrouchSpeed;
        }
        else if (InputReader != null && InputReader.IsSprintPressed && movementInput.sqrMagnitude > 0.01f)
        {
            IsSprinting = true;
            targetSpeed = Data.SprintSpeed;
        }
        else
        {
            IsSprinting = false;
            targetSpeed = Data.Speed;
        }

        horizontalMovement = horizontalDirection * targetSpeed;
    }

    private void HandleVerticalMovement()
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
            VerticalMovement += Physics.gravity.y * Data.GravityMultiplyer * Time.deltaTime;
        }
    }

    private void ApplyMovement()
    {
        FinalMovement = horizontalMovement;
        FinalMovement.y = VerticalMovement;

        characterController.Move(FinalMovement * Time.deltaTime);
    }

    private void CalculateRotation()
    {
        CamForwardDirection = _camTransform.forward;
        CamForwardDirection.y = 0;
        CamForwardDirection.Normalize();

        CamRightDirection = _camTransform.right;
        CamRightDirection.y = 0;
        CamRightDirection.Normalize();
    }

    private void UpdateCamRotateToPlayer()
    {
        float t = 1f - Mathf.Exp(-turnSpeed * Time.deltaTime);
        Head.rotation = Quaternion.Slerp(Head.rotation, _camTransform.rotation, t);
    }

    private void TeleportTo(Transform targetTransform)
    {
        characterController.enabled = false;
        transform.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);
        VerticalMovement = 0f;
        characterController.enabled = true;

        Head.rotation = targetTransform.rotation;

        _euler = targetTransform.eulerAngles;
        panTilt.PanAxis.Value = _euler.y;
        panTilt.TiltAxis.Value = _euler.x;
    }

    #endregion
}
