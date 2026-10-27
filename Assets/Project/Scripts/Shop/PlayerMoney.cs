using TMPro;
using UnityEngine;

public class PlayerMoney : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textUI;
    
    
    
    private int money = 5;

    public int Money
    {
        get => money;
        set
        {
            money = value;
            textUI.SetText($"{money} $");
        }
    }

    private void OnEnable()
    {
        EventBus<OnDebugActionEvent>.Subscribe(OnDebugAction);
    }

    private void OnDisable()
    {
        EventBus<OnDebugActionEvent>.Unsubscribe(OnDebugAction);
    }

    private void OnDebugAction(OnDebugActionEvent evt)
    {
        AddMoney(100);
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
