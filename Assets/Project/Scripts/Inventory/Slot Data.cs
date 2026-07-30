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
}
