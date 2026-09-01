using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CasinoState
{
    Idle,
    Playing
}

public class Casino : MonoBehaviour
{
    [Header("UI Ref")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI gamblingMoneyText;
    [SerializeField] private TextMeshProUGUI gamblingButtonText;
    [SerializeField] private Button gamblingButton;
    [SerializeField] private Button withdrawButton;
    
    [Header("Other Ref")]
    [SerializeField] private PlayerMoney playerMoney;
    [SerializeField] private TextMeshProUGUI playerMoneyUI;

    private CasinoState _currrentState = CasinoState.Idle;
    private CasinoState currrentState
    {
        get => _currrentState;
        set
        {
            _currrentState = value;
            SetUI();
        }
    }
    
    private int inputMoney;
    private int currentMoney;
    private const float WIN_CHANCE = 0.5F;
    
    // cached
    private string _initialText;
    private GameObject _gamblingTextGameObject;
    private GameObject _inputFieldGameObject;

    private void Awake()
    {
        _initialText = inputField.text;
        _gamblingTextGameObject = gamblingMoneyText.gameObject;
        _inputFieldGameObject = inputField.gameObject;
    }
    
    private void OnEnable()
    {
        inputField.text = _initialText;
        gamblingMoneyText.gameObject.SetActive(false);
        withdrawButton.interactable = false;
        playerMoneyUI.SetText($"Money : {playerMoney.Money.ToString()}$");
        EventBus<InventoryToggleEvent>.Subscribe(OnInventoryToggle);
    }

    private void OnDisable()
    {
        EventBus<InventoryToggleEvent>.Unsubscribe(OnInventoryToggle);
    }
    
    private void OnInventoryToggle(InventoryToggleEvent evt)
    {
        if (currrentState == CasinoState.Playing)
        {
            Withdrawn();
            currrentState = CasinoState.Idle;
        }
    }

    private bool IsValidNumber()
    {
        if (int.TryParse(inputField.text, out inputMoney))
        {
            if (playerMoney.CanSubtract(inputMoney))
            {
                currentMoney = inputMoney;
                return true;
            }
            else
            {
                inputField.text = "Invalid Amount";
            }
        }
        
        return false;
    }

    private bool CanGambling()
    {
        return currentMoney > 0;
    }

    private void SetUI()
    {
        switch (currrentState)
        {
            case CasinoState.Idle:
                inputField.text = _initialText;
                _inputFieldGameObject.SetActive(true);
                
                withdrawButton.interactable = false;
                
                _gamblingTextGameObject.SetActive(false);
                
                gamblingButton.interactable = true;
                gamblingButtonText.SetText("Bet");
                break;
            case CasinoState.Playing:
                _inputFieldGameObject.SetActive(false);
                
                gamblingMoneyText.SetText(currentMoney.ToString());
                _gamblingTextGameObject.SetActive(true);
                
                withdrawButton.interactable = true;
                
                gamblingButton.interactable = true;
                gamblingButtonText.SetText("Gambling");
                break;
        }
        
        // if (value) // gambling mode
        // {
        //     _inputFieldGameObject.SetActive(false);
        //     gamblingMoneyText.SetText(currentMoney.ToString());
        //     _gamblingTextGameObject.SetActive(true);
        //     withdrawButton.interactable = true;
        //     gamblingButtonText.SetText("Gambling");
        // }
        // else // Idle mode
        // {
        //     inputField.text = _initialText;
        //     _inputFieldGameObject.SetActive(true);
        //     _gamblingTextGameObject.SetActive(false);
        //     withdrawButton.interactable = false;
        //     gamblingButton.interactable = true;
        //     gamblingButtonText.SetText("Bet");
        // }
    }

    public void Gambling()
    {
        switch (currrentState)
        {
            case CasinoState.Idle:
                if (!IsValidNumber()) return;
                playerMoney.SubtractMoney(currentMoney);    
                playerMoneyUI.SetText($"Money : {playerMoney.Money.ToString()}$");
                currrentState = CasinoState.Playing;
                break;
            case CasinoState.Playing:
                if (CanGambling())
                    DoubleOrNothing();
                break;
        }
    }

    public void Withdrawn()
    {
        if (currrentState != CasinoState.Playing) return;
        playerMoney.AddMoney(currentMoney);
        playerMoneyUI.SetText($"Money : {playerMoney.Money.ToString()}$");
        currrentState = CasinoState.Idle;
    }

    private void DoubleOrNothing()
    {
        if (UnityEngine.Random.value > WIN_CHANCE)
        {
            currentMoney *= 2;
        }
        else
        {
            currentMoney = 0;
            gamblingButton.interactable = false;
        }

        gamblingMoneyText.SetText(currentMoney.ToString());
    }
}
