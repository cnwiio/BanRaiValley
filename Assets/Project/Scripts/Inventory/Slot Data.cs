using System;

public struct SlotData
{
    public Item item;
    public int count;

    public bool IsEmpty => item == null || count <= 0;
    public void Clear()
    {
        item = null;
        count = 0;
    }
    
    public SlotData Split(int amountToTake)
    {
        if (IsEmpty || amountToTake <= 0) return default;

        int actualTaken = UnityEngine.Mathf.Clamp(amountToTake, 1, count);
        SlotData splitData = new SlotData
        {
            item = item,
            count = actualTaken
        };

        count -= actualTaken;
        if (count <= 0)
        {
            Clear();
        }

        return splitData;
    }

    public int AddToStack(int amount)
    {
        int spaceLeft = item.MaxStack - count;
        if (spaceLeft > 0)
        {
            int amountToAdd = (int)MathF.Min(spaceLeft, amount);
            count += amountToAdd;
            amount -= amountToAdd;
        }
        return amount;
    }

    //public void RemoveFromStack(int amount)
    //{
    //    count -= amount;
    //    if (count < 0)
    //    {
    //        count = 0;
    //    }
    //}
}
