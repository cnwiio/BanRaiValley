using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementData", menuName = "Scriptable Objects/PlayerMovementData")]
public class PlayerMovementData : ScriptableObject
{
    [Header("Speeds")]
    [Tooltip("Normal walking speed in m/s")]
    public float Speed = 6f;

    [Tooltip("Sprinting speed in m/s")]
    public float SprintSpeed = 10f;

    [Tooltip("Crouching speed in m/s")]
    public float CrouchSpeed = 3f;

    [Header("Jumping & Gravity")]
    [Tooltip("Jump height in meters")]
    public float JumpHeight = 5f;

    [Tooltip("How much force that make player stick to ground")]
    public float GroundSnapForce = -0.5f;

    public float GravityMultiplyer = 4f;

    [Header("Crouch Settings")]
    [Tooltip("Character controller collider height when crouching")]
    public float CrouchHeight = 1.0f;

    [Tooltip("Camera head local Y offset when crouching")]
    public float CrouchHeadY = 0.25f;

    [Tooltip("Speed of transition between standing and crouching")]
    public float CrouchTransitionSpeed = 12f;
}
