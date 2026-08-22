using System;
using UnityEngine;

public class AIHealth : MonoBehaviour, IDamageable
{
    public bool IsInvicible;
    
    private int MaxHP;
    private int _hp;
    
    public int Hp
    {
        get => _hp;
        set
        {
            _hp = Math.Clamp(value, 0, MaxHP);
        }
    }

    public event Action OnDieEvent;
    public event Action OnTakeDamageEvent;

    public void Initialize(int HP)
    {
        _hp = HP;
        MaxHP = HP;
    }
    
    public void TakeDamage(int amount)
    {
        if (IsInvicible) return;
        Hp -= amount;
        // Debug.Log(gameObject.name  + " Take Damage : " + amount);
        // Debug.Log(gameObject.name  + " current HP : " + Hp);
        OnTakeDamageEvent?.Invoke();
        if (Hp <= 0)
        {
            OnDieEvent?.Invoke();
        }
    }
}