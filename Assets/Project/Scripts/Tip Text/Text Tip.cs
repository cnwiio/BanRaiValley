using System;
using Project.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class TextTip : MonoBehaviour, ITextTip
{
    [SerializeField] private TextTipReference textTipReference;
    [SerializeField] private TextMeshProUGUI _textTip;

    private GameObject _textTipGameObject;
    private void Awake()
    {
        _textTipGameObject = _textTip.gameObject;
        textTipReference.Register(this);
        gameObject.SetActive(false);
    }

    public void SetText(string text)
    {
        _textTip.SetText(text);
    }

    public void SetActive(bool isActive)
    {
        _textTipGameObject.SetActive(isActive);
    }

    public void OnDestroy()
    {
        textTipReference.Unregister(this);
    }
}
