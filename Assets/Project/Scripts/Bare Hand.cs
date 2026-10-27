using System;
using Lean.Pool;
using UnityEngine;
using Random = System.Random;

public class BareHand : MonoBehaviour, IPoolable
{
    [SerializeField] private Animator animator;

    private ComboAttackController combo;
    private void Awake()
    {
        combo = new ComboAttackController(animator, 2);
    }

    private void OnEnable()
    {
        EventBus<OnPrimaryActionEvent>.Subscribe(OnPrimaryAction);
    }   

    private void OnDisable()
    {
        EventBus<OnPrimaryActionEvent>.Unsubscribe(OnPrimaryAction);
    }

    private void OnPrimaryAction(OnPrimaryActionEvent evt)
    {
        combo.TryAttack();
    }

    public void OnAnimationHit()
    {
        combo.OnAnimationHit();
        EventBus<OnPlayerRequestAttackEvent>.Raise(new OnPlayerRequestAttackEvent());
    }

    
    public void OnSpawn()
    {
        combo.Reset();
    }

    public void OnDespawn()
    {
        combo.Reset();
    }
}
