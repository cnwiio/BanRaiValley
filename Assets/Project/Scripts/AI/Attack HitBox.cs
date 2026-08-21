using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AttackHitBox : MonoBehaviour
{
    [SerializeField] private BoxCollider boxCollider;

    private int _damage;
    private IDamageable _target;

    public void Initialize(int Damage)
    {
        _damage = Damage;
    }
    public void EnableHitBox(bool value)
    {
        boxCollider.enabled = value;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _target = other.GetComponent<IDamageable>();
            if (_target == null) return;
            _target.TakeDamage(_damage); 
        }
    }
}
