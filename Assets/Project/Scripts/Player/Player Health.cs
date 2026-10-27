using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private Image UI;
    
    private int MAXHP = 10;
    private int _hp = 10;

    public int Hp
    {
        get => _hp;
        set
        {
            _hp = Math.Clamp(value, 0, MAXHP);
            UI.fillAmount = (float)_hp / MAXHP;
            if (_hp == 0) EventBus<OnPlayerOutOfHPEvent>.Raise(new OnPlayerOutOfHPEvent());
        }
    }

    private void OnEnable()
    {
        EventBus<OnNewDayStartedEvent>.Subscribe(OnNewDay);
    }

    private void OnDisable()
    {
        EventBus<OnNewDayStartedEvent>.Unsubscribe(OnNewDay);
    }

    private void OnNewDay(OnNewDayStartedEvent evt)
    {
        if (evt.WasPassOut)
        {
            Hp = MAXHP / 2;
        }
        else
        {
            Hp = MAXHP;
        }
    }

    public void Start()
    {
        MAXHP = _hp;
    }

    public void SetMaxHP(int hp)
    {
        MAXHP = hp;
        Hp = hp;
    }
    
    public void TakeDamage(int amount)
    {
        Hp -= amount;
    }
}
