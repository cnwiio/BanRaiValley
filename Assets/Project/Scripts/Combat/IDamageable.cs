/// <summary>
/// Contract for any entity that can receive combat damage.
/// Implement this on HealthComponent or directly on a MonoBehaviour.
/// Consumers must use <c>GetComponent&lt;IDamageable&gt;()</c> — never cast to concrete types.
/// </summary>
public interface IDamageable
{
    /// <summary>Current hit-point total. Read-only to external systems.</summary>
    float CurrentHp { get; }

    /// <summary>Maximum hit-point capacity.</summary>
    float MaxHp { get; }

    /// <summary>
    /// Returns <c>true</c> while the entity has more than zero HP and
    /// has not begun a death sequence.
    /// </summary>
    bool IsAlive { get; }

    /// <summary>
    /// Applies the supplied damage to this entity.
    /// Implementations are responsible for clamping HP, emitting events,
    /// and triggering death when HP reaches zero.
    /// </summary>
    /// <param name="damageData">Full context of the incoming damage hit.</param>
    void TakeDamage(DamageData damageData);
}
