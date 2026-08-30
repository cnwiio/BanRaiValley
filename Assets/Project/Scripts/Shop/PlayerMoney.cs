using UnityEngine;

public class PlayerMoney : MonoBehaviour
{
    private int money = 999;

    public int Money
    {
        get => money;
        set
        {
            money = value;
            Debug.Log(money);
        }
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
        return amount < Money;
    }
}
