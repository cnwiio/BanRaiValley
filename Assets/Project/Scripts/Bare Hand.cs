using System;
using Lean.Pool;
using UnityEngine;

public class BareHand : MonoBehaviour, IPoolable
{
    [SerializeField] private Animator animator;
    [Tooltip("Duration in seconds the primary action button must be held before continuous combo looping triggers.")]
    [SerializeField] private float holdThreshold = 0.25f;

    private ComboAttackController combo;

    private void Awake()
    {
        combo = new ComboAttackController(animator, 2, holdThreshold);
    }

    private void Update()
    {
        combo.HoldThreshold = holdThreshold;
        combo.Update();
    }

    private void OnEnable()
    {
        EventBus<OnPrimaryActionStartedEvent>.Subscribe(OnPrimaryActionStarted);
        EventBus<OnPrimaryActionCanceledEvent>.Subscribe(OnPrimaryActionCanceled);
        EventBus<ChangeActionMap>.Subscribe(OnChangeActionMap);
    }   

    private void OnDisable()
    {
        EventBus<OnPrimaryActionStartedEvent>.Unsubscribe(OnPrimaryActionStarted);
        EventBus<OnPrimaryActionCanceledEvent>.Unsubscribe(OnPrimaryActionCanceled);
        EventBus<ChangeActionMap>.Unsubscribe(OnChangeActionMap);

        combo.OnPressCanceled();
    }

    private void OnPrimaryActionStarted(OnPrimaryActionStartedEvent evt)
    {
        combo.OnPressStarted();
    }

    private void OnPrimaryActionCanceled(OnPrimaryActionCanceledEvent evt)
    {
        combo.OnPressCanceled();
    }

    private void OnChangeActionMap(ChangeActionMap evt)
    {
        if (evt.MapType != ActionMapType.Player)
        {
            combo.OnPressCanceled();
        }
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
