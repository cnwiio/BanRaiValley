using System;
using TMPro;
using UnityEngine;

public class ShopBuyHandler : MonoBehaviour
{
    [SerializeField] private InventoyModel inventory;
    [SerializeField] private HotbarInventoryModel hotbar;
    [SerializeField] private PlayerMoney playerMoney;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI moneyUI;


    private void Start()
    {
        moneyUI.SetText($"Money : {playerMoney.Money}$");
    }

    private bool TryAddItem(Item item)
    {
        if (hotbar.TryAddItem(item, 1)) return true;
        return inventory.TryAddItem(item, 1);
    }

    public void TryBuyItem(Item item)
    {
        if (playerMoney.CanSubtract(item.Price))
        {
            if (TryAddItem(item))
            {
                playerMoney.SubtractMoney(item.Price);
                moneyUI.SetText($"Money : {playerMoney.Money}$");
            }
        }
    }
}
