using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementData", menuName = "Scriptable Objects/PlayerMovementData")]
public class PlayerMovementData : ScriptableObject
{
    public float Speed;
    [Tooltip("Jump height in meters")]
    public float JumpHeight;
    [Tooltip("How much force that make player stick to ground")]
    public float GroundSnapForce;
    public float GravityMultiplyer;
}
