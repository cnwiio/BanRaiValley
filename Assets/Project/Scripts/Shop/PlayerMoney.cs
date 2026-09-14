using System;
using TMPro;
using UnityEngine;

public class PlayerMoney : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textUI;
    
    
    
    private int money = 999;

    public int Money
    {
        get => money;
        set
        {
            money = value;
            textUI.SetText($"{money} $");
        }
    }

    public void Start()
    {
        textUI.SetText($"{money} $");
    }

    public void SubtractMoney(int amount)
    {
        Money -= amount;
    }

    public void AddMoney(int amount)
    {
        Money += amount;
    }

    public bool CanSubtract(int amount)
    {
        return amount <= Money;
    }
}
