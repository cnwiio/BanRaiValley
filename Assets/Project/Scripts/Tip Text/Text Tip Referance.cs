using Project.Scripts;
using UnityEngine;

[CreateAssetMenu(fileName = "TextTipReferance", menuName = "Scriptable Objects/TextTipReferance")]
public class TextTipReference : ScriptableObject
{
    private ITextTip _textTip;
    public ITextTip TextTip => _textTip;
    
    public void Register(ITextTip grid) => _textTip = grid;

    public void Unregister(ITextTip grid)
    {
        // guard against a second/old grid instance clearing a newer one's registration
        if (ReferenceEquals(_textTip, grid))
            _textTip = null;
    }

    // Safety net: if "Enter Play Mode Options" has domain reload disabled,
    // a stale reference from the previous play session could otherwise leak in.
    private void OnDisable() => _textTip = null;
}
