using System;
using System.Collections;
using UnityEngine;

public class AIAttack : MonoBehaviour
{
    [SerializeField] private AttackHitBox hitBox;

    private bool canAttack = true;
    private bool isAttacking;

    public bool CanAttack => canAttack;

    private Coroutine _attackCoroutine;
    private Coroutine _cooldownCoroutine;

    public event Action OnAttackEndEvent;

    private float attackCooldown;
    private float hitboxLifeSpan;

    public void Initialize(float cooldown, float hitboxLife, int damage)
    {
        attackCooldown = cooldown;
        hitboxLifeSpan = hitboxLife;
        hitBox.Initialize(damage);
    }
    
    public void StartAttack()
    {
        if (!canAttack || isAttacking) return;
            
        canAttack = false;
        isAttacking = true;
    }

    public void ExcuteAttack()
    {
        _attackCoroutine = StartCoroutine(AttackCoroutine());
    }

    public void StopAttack()
    {
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }

        if (_cooldownCoroutine != null)
        {
            StopCoroutine(_cooldownCoroutine);
            _cooldownCoroutine = null;
        }
        
        hitBox.EnableHitBox(false);
        isAttacking = false;
        canAttack = true;
    }

    private IEnumerator AttackCoroutine()
    {
        hitBox.EnableHitBox(true);
        yield return new WaitForSeconds(hitboxLifeSpan);
        hitBox.EnableHitBox(false);
        _attackCoroutine = null;
        OnAttackEndEvent?.Invoke();
        
        if (_cooldownCoroutine != null)
        {
            StopCoroutine(_cooldownCoroutine);
        }

        _cooldownCoroutine = StartCoroutine(AttackCooldownRoutine());
    }
    
    private IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        isAttacking = false;
        _cooldownCoroutine = null;
    }
}
