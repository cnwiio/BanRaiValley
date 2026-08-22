using TMPro;
using UnityEngine;

public class TimeUI : MonoBehaviour
{
    [Header("Time Display")]
    [Tooltip("Shows the current in-game time (e.g. \"06:00 AM\" or \"06:00\").")]
    [SerializeField] private TextMeshProUGUI _timeText;
    
    [Header("Configuration")]
    [Tooltip("TimeConfiguration ScriptableObject used to resolve season sprites.")]
    [SerializeField] private TimeConfiguration _configuration;

    [Tooltip("When true, displays time in 24-hour format. When false, uses 12-hour AM/PM.")]
    [SerializeField] private bool _use24HourFormat = false;
    
    public void HandleTimeTick(GameDateTime dateTime)
    {
        if (_timeText != null)
            _timeText.text = dateTime.ToTimeString(_use24HourFormat);
    }
}
