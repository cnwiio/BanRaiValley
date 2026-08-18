using UnityEngine;

/// <summary>
/// Trigger-based melee hitbox component attached to the plant's attack collider GameObject.
/// Detects overlapping <see cref="IDamageable"/> entities during an active attack window
/// and applies damage via the interface contract. Guards against self-damage.
/// </summary>
public class PlantMeleeHitbox : MonoBehaviour
{
    #region Serialized Fields

    [Header("Hitbox Collider")]
    [Tooltip("The Collider that acts as the physical hitbox. Assign the trigger collider on this GameObject.")]
    [SerializeField] private Collider _hitboxCollider;

    #endregion

    #region Fields

    private float      _damageAmount;
    private GameObject _owner;

    #endregion

    #region Public Methods

    /// <summary>
    /// Binds this hitbox to its owning entity and configures the damage it will deal.
    /// Must be called by <see cref="PlantCombat.Initialize"/> before any attack can occur.
    /// </summary>
    /// <param name="owner">The plant's root GameObject, used to prevent self-damage collisions.</param>
    /// <param name="damage">Raw damage amount applied to every valid <see cref="IDamageable"/> hit.</param>
    public void Initialize(GameObject owner, float damage)
    {
        _owner        = owner;
        _damageAmount = damage;
    }

    /// <summary>
    /// Enables or disables the hitbox collider to open or close the active attack window.
    /// </summary>
    /// <param name="isEnabled"><c>true</c> to activate the hitbox; <c>false</c> to deactivate it.</param>
    public void EnableHitbox(bool isEnabled)
    {
        if (_hitboxCollider != null)
        {
            _hitboxCollider.enabled = isEnabled;
        }
    }

    #endregion

    #region Private Methods

    private void OnTriggerEnter(Collider other)
    {
        // Ignore self-collisions from the plant's own colliders.
        if (other.gameObject == _owner)
        {
            return;
        }

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable == null || !damageable.IsAlive)
        {
            return;
        }

        DamageData damageData = new DamageData(
            amount:         _damageAmount,
            type:           DamageType.Physical,
            source:         _owner,
            hitPoint:       other.ClosestPoint(transform.position),
            hitNormal:      (other.transform.position - transform.position).normalized
        );

        damageable.TakeDamage(damageData);
    }

    #endregion
}
