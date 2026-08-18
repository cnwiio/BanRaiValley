using UnityEngine;

/// <summary>
/// Pure data container ScriptableObject holding all balance metrics for a Plant AI monster.
/// Contains no runtime state, no scene references, and no event subscriptions.
/// </summary>
[CreateAssetMenu(fileName = "PlantAIConfig", menuName = "BanRaiValley/AI/Plant AI Config")]
public class PlantAIConfigSO : ScriptableObject
{
    #region Serialized Fields

    [Header("Health & Defense")]
    [Tooltip("Maximum hit points of this plant monster variant.")]
    [field: SerializeField]
    public float MaxHp { get; private set; } = 50f;

    [Tooltip("Duration in seconds the plant staggers after receiving a hit.")]
    [field: SerializeField]
    public float HitStaggerDurationSec { get; private set; } = 0.3f;


    [Header("Locomotion & Range")]
    [Tooltip("Movement speed in Unity units per second (Ups).")]
    [field: SerializeField]
    public float MoveSpeedUps { get; private set; } = 3.5f;

    [Tooltip("Turn speed in degrees per second applied via NavMeshAgent angular speed.")]
    [field: SerializeField]
    public float RotationSpeedDeg { get; private set; } = 360f;

    [Tooltip("Radius in metres within which the plant detects and aggros a player.")]
    [field: SerializeField]
    public float AggroRadiusM { get; private set; } = 8f;

    [Tooltip("Maximum distance in metres at which the plant can land an attack.")]
    [field: SerializeField]
    public float AttackRangeM { get; private set; } = 1.8f;

    [Tooltip("NavMeshAgent stopping distance in metres — plant halts when this close to the target.")]
    [field: SerializeField]
    public float StoppingDistanceM { get; private set; } = 1.2f;


    [Header("Combat & Timing")]
    [Tooltip("Raw damage dealt per successful attack before the target's defense is applied.")]
    [field: SerializeField]
    public float BaseAttackDamage { get; private set; } = 10f;

    [Tooltip("Minimum seconds between consecutive attacks.")]
    [field: SerializeField]
    public float AttackCooldownSec { get; private set; } = 1.5f;

    [Tooltip("Seconds from attack trigger until the hit-box is active (windup phase).")]
    [field: SerializeField]
    public float AttackWindupSec { get; private set; } = 0.4f;

    [Tooltip("Duration in seconds of the Awakening (uprooting) animation before transitioning to Idle.")]
    [field: SerializeField]
    public float AwakeningDurationSec { get; private set; } = 1.2f;


    [Header("Animation & Feedback")]
    [Tooltip("Animator trigger parameter name for the awakening / uprooting animation.")]
    [field: SerializeField]
    public string AwakeningTriggerName { get; private set; } = "Awaken";

    [Tooltip("Animator trigger parameter name for the attack animation.")]
    [field: SerializeField]
    public string AttackTriggerName { get; private set; } = "Attack";

    [Tooltip("Animator trigger parameter name for the hit-react / flinch animation.")]
    [field: SerializeField]
    public string HitTriggerName { get; private set; } = "Hit";

    [Tooltip("Animator trigger parameter name for the death animation.")]
    [field: SerializeField]
    public string DieTriggerName { get; private set; } = "Die";

    #endregion
}
