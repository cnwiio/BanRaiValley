using System;
using TMPro;
using UnityEngine;

public class TimeUI : MonoBehaviour
{
    [Header("Time Display")]
    [Tooltip("Shows the current in-game time (e.g. \"06:00 AM\" or \"06:00\").")]
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private TextMeshProUGUI _DayText;
    [SerializeField] private TextMeshProUGUI _DayofWeekText;
    
    [Header("Configuration")]
    [Tooltip("TimeConfiguration ScriptableObject used to resolve season sprites.")]
    [SerializeField] private TimeConfiguration _configuration;

    [Tooltip("When true, displays time in 24-hour format. When false, uses 12-hour AM/PM.")]
    [SerializeField] private bool _use24HourFormat = false;

    private void Awake()
    {
        var date = GameDateTime.InitialDate;
        RefreshCalender(date);
    }

    private void OnEnable()
    {
        EventBus<OnNewDayStartedEvent>.Subscribe(OnNewDayStarted);
    }

    private void OnDisable()
    {
        EventBus<OnNewDayStartedEvent>.Unsubscribe(OnNewDayStarted);
    }
    public void OnNewDayStarted(OnNewDayStartedEvent evt)
    {
        RefreshCalender(evt.NewDateTime);
    }
    
    public void HandleTimeTick(GameDateTime dateTime)
    {
        if (_timeText != null)
            _timeText.text = dateTime.ToTimeString(_use24HourFormat);
    }

    public void RefreshCalender(GameDateTime dateTime)
    {
        if (_DayText != null)
            _DayText.text = $"{dateTime.DayOfSeason}";
        
        if (_DayofWeekText)
            _DayofWeekText.text = dateTime.DayOfWeek.ToString().Substring(0, 3) + ".";
    }
    
}
