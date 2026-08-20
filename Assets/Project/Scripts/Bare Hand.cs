using System;
using UnityEngine;

public class BareHand : MonoBehaviour
{
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
        EventBus<OnPlayerRequestAttackEvent>.Raise(new OnPlayerRequestAttackEvent());
    }
}
