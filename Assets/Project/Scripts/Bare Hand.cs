using System;
using Lean.Pool;
using UnityEngine;
using Random = System.Random;

public class BareHand : MonoBehaviour, IPoolable
{
    [SerializeField] private Animator animator;

    private int attackHash;
    private int attackHash2;
    private int isAttackingHash;
    private void Awake()
    {
        attackHash = Animator.StringToHash("Attack");
        attackHash2 = Animator.StringToHash("Attack2");
        isAttackingHash = Animator.StringToHash("IsAttacking");
    }

    private void OnEnable()
    {
        EventBus<OnPrimaryActionEvent>.Subscribe(OnPrimaryAction);
        // ResetAnimation();
    }   

    private void OnDisable()
    {
        EventBus<OnPrimaryActionEvent>.Unsubscribe(OnPrimaryAction);
    }

    private int num = 0;
    private bool isAttacking;
    private void OnPrimaryAction(OnPrimaryActionEvent evt)
    {
        if (isAttacking) return;
        isAttacking = true;
        animator.SetBool(isAttackingHash, true);
        
        if (num == 0)
        {
            Debug.Log("Trigger 1");
            animator.SetTrigger("Attack");
        }else if (num == 1)
        {
            Debug.Log("Trigger 2");
            animator.SetTrigger("Attack2");
        }
    }

    public void OnAnimationHit()
    {
        num = (num + 1) % 2;
        if (num == 0)
        {
            animator.ResetTrigger("Attack2");
        }else if (num == 1)
        {
            animator.ResetTrigger("Attack");
        }
        animator.SetBool(isAttackingHash, false);
        isAttacking = false;
        EventBus<OnPlayerRequestAttackEvent>.Raise(new OnPlayerRequestAttackEvent());
    }

    public void ResetAnimation()
    {
        num = 0;
        isAttacking = false;
        animator.SetBool(isAttackingHash, false);
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Attack2");
        animator.Rebind();
        animator.Update(0f);
    }
    
    public void OnSpawn()
    {
        ResetAnimation();
    }

    public void OnDespawn()
    {
        ResetAnimation();
    }
}
