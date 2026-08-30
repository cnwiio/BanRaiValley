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
        Debug.Log($"Player take {amount} damage");
        Debug.Log("current HP = " + Hp);
    }
}
