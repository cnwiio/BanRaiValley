using UnityEngine;

[CreateAssetMenu(fileName = "AIData", menuName = "Scriptable Objects/AIData")]
public class AIData : ScriptableObject
{
    [Header("Movement")]
    public float Speed = 2.5f;
    public float Acceleration = 8;
    public float TurnSpeed = 250;
    // public float MoveThreshold;
    [Tooltip("Must be smaller than attack distance if want melee attack")]
    public float StopDistance = 3;
    public float DetectionRange = 15;
    
    [Header("Combat")]
    public int HP = 10;
    public int Damage = 1;
    public float AttackRange = 5;
    public float AttackCooldown = 2;
    public float StunTime = 2.5f;
    
    [Header("Drop")]
    public Item dropItem;
    public int dropAmountMin = 1;
    public int dropAmountMax = 1;
    [Range(0f, 1f)] public float dropChance = 1f;

    // public WaitForSeconds DespawnTime = new WaitForSeconds(3f);
    // public WaitForSeconds chaseWaitInterval = new WaitForSeconds(3f);
}
