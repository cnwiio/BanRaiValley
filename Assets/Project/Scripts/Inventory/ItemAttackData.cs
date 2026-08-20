using UnityEngine;

/// <summary>
/// Holds all attack parameters for an equippable item.
/// Attach via <see cref="Item.AttackData"/> on any Item ScriptableObject.
/// </summary>
[System.Serializable]
public struct ItemAttackData
{
    /// <summary>Whether this item is allowed to initiate an attack.</summary>
    public bool canAttack;

    /// <summary>Raw damage dealt per successful hit.</summary>
    public float damageAmount;

    /// <summary>Category of damage applied to the target.</summary>
    public DamageType damageType;

    /// <summary>Minimum time in seconds between consecutive attacks.</summary>
    public float attackCooldownSec;

    /// <summary>Stamina deducted from the player on each attack.</summary>
    public float staminaCost;

    /// <summary>Force magnitude applied to the target's rigidbody on hit.</summary>
    public float knockbackForce;

    /// <summary>Half-extents of the overlap-box used for hit detection.</summary>
    public Vector3 attackBoxSize;

    /// <summary>Local offset from the player origin where the attack box is centred.</summary>
    public Vector3 attackBoxOffset;

    /// <summary>
    /// Default configuration for an unarmed (fist) attack.
    /// Use this as the fallback when no weapon is equipped.
    /// </summary>
    public static ItemAttackData DefaultUnarmed => new ItemAttackData
    {
        canAttack          = true,
        damageAmount       = 2f,
        damageType         = DamageType.Physical,
        attackCooldownSec  = 0.4f,
        staminaCost        = 1f,
        knockbackForce     = 2f,
        attackBoxSize      = new Vector3(1.2f, 1.2f, 2.0f),
        attackBoxOffset    = new Vector3(0f, 0f, 1.2f)
    };
}
