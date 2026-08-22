using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BanRaiValley.Time;

namespace BanRaiValley.Time.UI
{
    /// <summary>
    /// HUD component that displays in-game time, weekday, date, season, year, and pause state.
    ///
    /// Responsibilities:
    ///   - Renders the current time string (12h or 24h) on every OnTimeTickEvent.
    ///   - Refreshes calendar labels (day-of-week, date, season/year) on OnNewDayStartedEvent.
    ///   - Updates the season icon on both OnNewDayStartedEvent and OnSeasonChangedEvent.
    ///   - Toggles the pause indicator GameObject on OnTimePausedStateChangedEvent.
    ///
    /// Design constraints:
    ///   - Zero Update() polling — all state changes are driven by EventBus events.
    ///   - Subscribe in OnEnable, unsubscribe in OnDisable.
    /// </summary>
    public class TimeCalendarHUD : MonoBehaviour
    {
        // ----------------------------------------------------------------
        // Inspector Fields — UI Elements
        // ----------------------------------------------------------------

        [Header("Time Display")]
        [Tooltip("Shows the current in-game time (e.g. \"06:00 AM\" or \"06:00\").")]
        [SerializeField] private TextMeshProUGUI _timeText;

        [Header("Calendar Display")]
        [Tooltip("Shows the abbreviated weekday name (e.g. \"Mon.\").")]
        [SerializeField] private TextMeshProUGUI _dayOfWeekText;

        [Tooltip("Shows the day number within the current season (e.g. \"15\").")]
        [SerializeField] private TextMeshProUGUI _dateText;

        [Tooltip("Shows the season and year (e.g. \"Spring, Year 1\").")]
        [SerializeField] private TextMeshProUGUI _seasonYearText;

        [Tooltip("Image component used to render the current season icon.")]
        [SerializeField] private Image _seasonIconImage;

        [Header("Pause State")]
        [Tooltip("GameObject toggled on/off to indicate whether time is paused.")]
        [SerializeField] private GameObject _pauseIndicator;

        // ----------------------------------------------------------------
        // Inspector Fields — Configuration
        // ----------------------------------------------------------------

        [Header("Configuration")]
        [Tooltip("TimeConfiguration ScriptableObject used to resolve season sprites.")]
        [SerializeField] private TimeConfiguration _configuration;

        [Tooltip("When true, displays time in 24-hour format. When false, uses 12-hour AM/PM.")]
        [SerializeField] private bool _use24HourFormat = false;

        private void Awake()
        {
            var date = new GameDateTime()
            {
                DayOfSeason = 1,
                DayOfWeek = DayOfWeek.Monday,
                Hour = _configuration.StartHour,
                Minute = 0,
                Season = Season.Summer,
                Year = 1
            };
            RefreshCalendarLabels(date);
            RefreshSeasonIcon(date.Season);
        }

        // ================================================================
        // Unity Lifecycle
        // ================================================================

        private void OnEnable()
        {
            EventBus<OnTimeTickEvent>.Subscribe(HandleTimeTick);
            EventBus<OnNewDayStartedEvent>.Subscribe(HandleNewDayStarted);
            EventBus<OnSeasonChangedEvent>.Subscribe(HandleSeasonChanged);
            EventBus<OnTimePausedStateChangedEvent>.Subscribe(HandlePauseStateChanged);
        }

        private void OnDisable()
        {
            EventBus<OnTimeTickEvent>.Unsubscribe(HandleTimeTick);
            EventBus<OnNewDayStartedEvent>.Unsubscribe(HandleNewDayStarted);
            EventBus<OnSeasonChangedEvent>.Unsubscribe(HandleSeasonChanged);
            EventBus<OnTimePausedStateChangedEvent>.Unsubscribe(HandlePauseStateChanged);
        }

        // ================================================================
        // Event Handlers
        // ================================================================
        
        /// <summary>
        /// Refreshes the time display text on every game-minute tick.
        /// </summary>
        private void HandleTimeTick(OnTimeTickEvent evt)
        {
            if (_timeText != null)
                _timeText.text = evt.CurrentDateTime.ToTimeString(_use24HourFormat);
        }

        /// <summary>
        /// Refreshes the day-of-week, date, season/year labels, and season icon
        /// when a new in-game day begins.
        /// </summary>
        private void HandleNewDayStarted(OnNewDayStartedEvent evt)
        {
            RefreshCalendarLabels(evt.NewDateTime);
            RefreshSeasonIcon(evt.NewDateTime.Season);
        }

        /// <summary>
        /// Refreshes the season icon (and season/year label) when the season rolls over.
        /// </summary>
        private void HandleSeasonChanged(OnSeasonChangedEvent evt)
        {
            if (_seasonYearText != null)
                _seasonYearText.text = $"{evt.NewSeason}, Year {evt.Year}";

            RefreshSeasonIcon(evt.NewSeason);
        }

        /// <summary>
        /// Toggles the pause indicator when the time-pause state changes.
        /// </summary>
        private void HandlePauseStateChanged(OnTimePausedStateChangedEvent evt)
        {
            if (_pauseIndicator != null)
                _pauseIndicator.SetActive(evt.IsPaused);
        }

        // ================================================================
        // Private Helpers
        // ================================================================

        /// <summary>
        /// Updates the weekday abbreviation, day number, and season/year text fields.
        /// </summary>
        private void RefreshCalendarLabels(GameDateTime dateTime)
        {
            if (_dayOfWeekText != null)
                _dayOfWeekText.text = dateTime.DayOfWeek.ToString().Substring(0, 3) + ".";

            if (_dateText != null)
                _dateText.text = $"{dateTime.DayOfSeason}";

            if (_seasonYearText != null)
                _seasonYearText.text = $"{dateTime.Season}, Year {dateTime.Year}";
        }

        /// <summary>
        /// Updates the season icon sprite from the TimeConfiguration asset.
        /// </summary>
        private void RefreshSeasonIcon(Season season)
        {
            if (_seasonIconImage != null && _configuration != null)
                _seasonIconImage.sprite = _configuration.GetSeasonIcon(season);
        }
    }
}
