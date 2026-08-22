using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Reusable melee combo controller: cycles through a set of animator triggers,
/// buffers one pending attack input while the current swing is playing, and
/// notifies the owner via OnComboHit when a hit lands (mirrors the old
/// "OnAnimationHit" animation-event callback in BareHand).
///
/// This is a plain C# class (not a MonoBehaviour) on purpose: it has no
/// dependency on GameObject lifecycle, so both BareHand and Hoe can each own
/// one via composition instead of duplicating the num/pendingAttack/isAttack
/// fields and the num==0/num==1 branching.
/// </summary>
[System.Serializable]
public class ComboAttackController
{
    private readonly Animator animator;
    private readonly int[] attackHashes;

    private int currentIndex;
    private bool isAttacking;
    private bool pendingAttack;
    private float lastAttackedTime;
    private readonly float comboResetTime = 2;

    public ComboAttackController(Animator animator, int comboLength)
    {
        this.animator = animator;

        if (comboLength == 0)
            throw new ArgumentException("ComboAttackController needs at least one trigger name.");

        attackHashes = new int[comboLength];
        for (int i = 0; i < comboLength; i++)
            attackHashes[i] = Animator.StringToHash("Attack" + (i + 1));
    }

    /// <summary>Call this from the input/event handler (e.g. OnPrimaryAction).</summary>
    public void TryAttack()
    {
        if (Time.time - lastAttackedTime > comboResetTime && !isAttacking)
        {
            currentIndex = 0;
        }
        if (!pendingAttack && isAttacking)
        {
            pendingAttack = true;
            return;
        }

        Attack();
    }

    private void Attack()
    {
        isAttacking = true;
        lastAttackedTime = Time.time;
        animator.SetTrigger(attackHashes[currentIndex]);
    }

    /// <summary>Call this from the Animation Event (e.g. OnAnimationHit / OnAttackAnimationHit).</summary>
    public void OnAnimationHit()
    {
        int previousIndex = currentIndex;
        currentIndex = (currentIndex + 1) % attackHashes.Length;
        animator.ResetTrigger(attackHashes[previousIndex]);

        isAttacking = false;
        // OnComboHit?.Invoke();

        if (pendingAttack)
        {
            pendingAttack = false;
            Attack();
        }
        
    }
    
    public bool IsAttacking()
    {
        return isAttacking;
    }
    
    /// <summary>Call on spawn/despawn/state-exit to fully reset the combo.</summary>
    public void Reset()
    {
        currentIndex = 0;
        isAttacking = false;
        pendingAttack = false;

        foreach (var hash in attackHashes)
            animator.ResetTrigger(hash);

        animator.Rebind();
        animator.Update(0f);
    }
}