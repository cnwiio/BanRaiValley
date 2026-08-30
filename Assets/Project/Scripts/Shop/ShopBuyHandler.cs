using UnityEngine;

public class ShopBuyHandler : MonoBehaviour
{
    [SerializeField] private BaseInventory inventory;
    [SerializeField] private PlayerMoney playerMoney;
    
    
    private bool TryAddItem(Item item)
    {
        return inventory.TryAddItem(item, 1);
    }

    public void TryBuyItem(Item item)
    {
        if (playerMoney.CanSubtract(item.Price))
        {
            if (TryAddItem(item))
            {
                playerMoney.SubtractMoney(item.Price);
            }
        }
    }
}
