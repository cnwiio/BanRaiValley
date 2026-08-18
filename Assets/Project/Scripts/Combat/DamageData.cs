using UnityEngine;

/// <summary>
/// An immutable value object that encapsulates all data relevant to a single
/// damage event. Passed via <see cref="IDamageable.TakeDamage"/> and the
/// Plant AI EventBus events so every system has a consistent view of the hit.
/// </summary>
public readonly struct DamageData
{
    /// <summary>The raw damage amount to apply before resistances.</summary>
    public float Amount { get; }

    /// <summary>The elemental or physical category of the damage.</summary>
    public DamageType Type { get; }

    /// <summary>The GameObject that is the originator of this damage.</summary>
    public GameObject Source { get; }

    /// <summary>World-space position where the hit landed.</summary>
    public Vector3 HitPoint { get; }

    /// <summary>World-space surface normal at the hit point (used for VFX/decals).</summary>
    public Vector3 HitNormal { get; }

    /// <summary>Impulse force applied to the target upon receiving this damage.</summary>
    public float KnockbackForce { get; }

    /// <summary>
    /// Initialises a new <see cref="DamageData"/> instance.
    /// </summary>
    /// <param name="amount">Raw damage amount.</param>
    /// <param name="type">Damage category.</param>
    /// <param name="source">Originating GameObject.</param>
    /// <param name="hitPoint">World-space hit position.</param>
    /// <param name="hitNormal">Surface normal at the hit position.</param>
    /// <param name="knockbackForce">Impulse force applied to the target. Defaults to 0.</param>
    public DamageData(
        float amount,
        DamageType type,
        GameObject source,
        Vector3 hitPoint,
        Vector3 hitNormal,
        float knockbackForce = 0f)
    {
        Amount         = amount;
        Type           = type;
        Source         = source;
        HitPoint       = hitPoint;
        HitNormal      = hitNormal;
        KnockbackForce = knockbackForce;
    }
}
