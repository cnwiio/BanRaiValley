using System;
using UnityEngine;

/// <summary>
/// Reusable melee combo controller: cycles through a set of animator triggers,
/// buffers one pending attack input while the current swing is playing, and
/// supports hold-to-attack continuous combo looping with a configurable hold threshold.
///
/// This is a plain C# class (not a MonoBehaviour) on purpose: it has no
/// dependency on GameObject lifecycle, so both BareHand and Hoe can each own
/// one via composition instead of duplicating fields and branching.
/// </summary>
[System.Serializable]
public class ComboAttackController
{
    private readonly Animator animator;
    private readonly int[] attackHashes;

    private int currentIndex;
    private bool isAttacking;
    private bool pendingAttack;
    private bool isHoldingAttack;
    private float pressStartTime;
    private float holdThreshold = 0.25f;
    private float lastAttackedTime;
    private readonly float comboResetTime = 2f;

    public float HoldThreshold
    {
        get => holdThreshold;
        set => holdThreshold = value;
    }

    public bool IsHolding => isHoldingAttack;

    public ComboAttackController(Animator animator, int comboLength, float holdThreshold = 0.25f)
    {
        this.animator = animator;
        this.holdThreshold = holdThreshold;

        if (comboLength == 0)
            throw new ArgumentException("ComboAttackController needs at least one trigger name.");

        attackHashes = new int[comboLength];
        for (int i = 0; i < comboLength; i++)
            attackHashes[i] = Animator.StringToHash("Attack" + (i + 1));
    }

    /// <summary>Call this on button down (e.g. from OnPrimaryActionStarted).</summary>
    public void OnPressStarted()
    {
        isHoldingAttack = true;
        pressStartTime = Time.time;

        if (Time.time - lastAttackedTime > comboResetTime && !isAttacking)
        {
            currentIndex = 0;
        }

        if (isAttacking)
        {
            pendingAttack = true;
            return;
        }

        Attack();
    }

    /// <summary>Call this on button up (e.g. from OnPrimaryActionCanceled).</summary>
    public void OnPressCanceled()
    {
        isHoldingAttack = false;
    }

    /// <summary>Call this from Update to trigger continuous looping once hold threshold is reached.</summary>
    public void Update()
    {
        if (isHoldingAttack && !isAttacking)
        {
            if (Time.time - pressStartTime >= holdThreshold)
            {
                Attack();
            }
        }
    }

    /// <summary>Call this for single-attack invocation or compatibility.</summary>
    public void TryAttack()
    {
        OnPressStarted();
    }

    /// <summary>Legacy toggle method for hold state.</summary>
    public void SetHoldAttack(bool isHolding)
    {
        if (isHolding)
            OnPressStarted();
        else
            OnPressCanceled();
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

        bool isHoldActive = isHoldingAttack && (Time.time - pressStartTime >= holdThreshold);

        if (isHoldActive)
        {
            pendingAttack = false;
            Attack();
        }
        else if (pendingAttack)
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
        isHoldingAttack = false;

        foreach (var hash in attackHashes)
            animator.ResetTrigger(hash);

        animator.Rebind();
        animator.Update(0f);
    }
}
