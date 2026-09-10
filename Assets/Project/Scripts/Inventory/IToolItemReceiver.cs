/// <summary>
/// Interface for tools or components that receive an Item asset and bind its data.
/// </summary>
public interface IToolItemReceiver
{
    void BindItemData(Item item);
}
