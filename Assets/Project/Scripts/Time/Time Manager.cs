using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private TimeConfiguration configuration;
    [SerializeField] private DayNightLightingController dayNightLightingController;
    [SerializeField] private TimeUI timeUI;
    
    [Tooltip("When true, the simulation starts ticking immediately in Awake.")]
    [SerializeField] private bool _autoStartOnAwake = true;

    // ----------------------------------------------------------------
    // Private State
    // ----------------------------------------------------------------

    private GameDateTime _currentDateTime;
    private bool         _isPaused;
    private float        _minuteAccumulator;
    private float        _timeScaleMultiplier = 1f;
    private bool         _hasPassedOutToday;

    // ----------------------------------------------------------------
    // Public Properties
    // ----------------------------------------------------------------

    public GameDateTime CurrentDateTime     => _currentDateTime;
    public bool         IsPaused            => _isPaused;
    public float        TimeScaleMultiplier => _timeScaleMultiplier;

    /// <summary>
    /// Returns the continuous fraction of the 24-hour day elapsed (0.0 to 1.0),
    /// incorporating sub-tick real-time progress for smooth per-frame visual rendering.
    /// </summary>
    public float ContinuousNormalizedDayTime
    {
        get
        {
            float secondsPerMinute = (configuration != null && configuration.realSecondsPerGameMinute > 0f)
                ? configuration.realSecondsPerGameMinute
                : 1f;

            float fractionalMinutes = _minuteAccumulator / secondsPerMinute;
            float totalMinutes = (_currentDateTime.Hour * 60f) + _currentDateTime.Minute + fractionalMinutes;
            return (totalMinutes / 1440f) % 1f;
        }
    }

    private void OnEnable()
    {
        EventBus<OnSleepEvent>.Subscribe(OnSleep);
        EventBus<OnPlayerOutOfHPEvent>.Subscribe(OnPlayerOutOfHP);
    }

    private void OnDisable()
    {
        EventBus<OnSleepEvent>.Unsubscribe(OnSleep);
        EventBus<OnPlayerOutOfHPEvent>.Unsubscribe(OnPlayerOutOfHP);
    }

    private void OnSleep(OnSleepEvent evt)
    {
        SleepToNextMorning();
    }

    private void OnPlayerOutOfHP(OnPlayerOutOfHPEvent evt)
    {
        ForcePassout();
    }

    private void Awake()
    {
        _currentDateTime = GameDateTime.InitialDate;

        if (!_autoStartOnAwake)
            _isPaused = true;
    }

    private void Start()
    {
        if (dayNightLightingController != null)
            dayNightLightingController.SetSunRotation(_currentDateTime.NormalizedDayTime);

        if (timeUI != null)
            timeUI.HandleTimeTick(_currentDateTime);
    }

    private void Update()
    {
        if (_isPaused || configuration == null)
            return;

        _minuteAccumulator += Time.deltaTime * _timeScaleMultiplier;

        float secondsPerTick = configuration.realSecondsPerGameMinute
                               * configuration.minuteTickInterval;

        while (_minuteAccumulator >= secondsPerTick && secondsPerTick > 0)
        {
            _minuteAccumulator -= secondsPerTick;
            AdvanceMinute(configuration.minuteTickInterval);
        }

        if (dayNightLightingController != null)
        {
            dayNightLightingController.UpdateContinuousLighting(ContinuousNormalizedDayTime);
        }
    }

    /// <summary>
    /// Advances the in-game clock by the specified number of minutes,
    /// handling hour roll-overs, passout detection, and event emission.
    /// </summary>
    private void AdvanceMinute(int minutes)
    {
        int previousHour = _currentDateTime.Hour;

        _currentDateTime.Minute += minutes;

        // Roll over minutes -> hours
        while (_currentDateTime.Minute >= 60)
        {
            _currentDateTime.Minute -= 60;
            _currentDateTime.Hour++;
        }

        // Hour roll-over (past midnight)
        if (_currentDateTime.Hour >= 24)
            _currentDateTime.Hour %= 24;

        bool hourDidChange = _currentDateTime.Hour != previousHour;

        if (hourDidChange)
        {
            // Passout check — triggers once per day at the configured hour
            if (_currentDateTime.Hour == configuration.passoutHour && !_hasPassedOutToday)
            {
                _hasPassedOutToday = true;
                ForcePassout();
                return; // ForcePassout resets state; exit to avoid stale tick event
            }
        }

        if (dayNightLightingController != null)
            dayNightLightingController.HandleTimeTick(_currentDateTime.NormalizedDayTime);

        if (timeUI != null)
            timeUI.HandleTimeTick(_currentDateTime);
    }

    /// <summary>
    /// Call this when the player chooses to sleep voluntarily.
    /// Ends the current day and begins a new one at the configured start hour.
    /// </summary>
    public void SleepToNextMorning()
    {
        EventBus<OnDayEndedEvent>.Raise(new OnDayEndedEvent
        {
            EndedDateTime = _currentDateTime,
            IsPassOut     = false
        });

        _isPaused = true;
        AdvanceToNextDay(wasPassout: false);
    }

    /// <summary>
    /// Triggers a forced passout (player was still awake at the passout hour).
    /// Applies a stamina penalty and transitions to the next day.
    /// </summary>
    public void ForcePassout()
    {
        EventBus<OnDayEndedEvent>.Raise(new OnDayEndedEvent
        {
            EndedDateTime = _currentDateTime,
            IsPassOut     = true
        });
        EventBus<OnPlayerPassedOutEvent>.Raise(new OnPlayerPassedOutEvent());
        AdvanceToNextDay(wasPassout: true);
    }

    /// <summary>
    /// Shared calendar advancement logic used by both sleep and passout paths.
    /// </summary>
    private void AdvanceToNextDay(bool wasPassout)
    {
        // Reset clock to start of new day
        _currentDateTime.Hour   = configuration.startHour;
        _currentDateTime.Minute = 0;
        _minuteAccumulator      = 0f;
        _hasPassedOutToday      = false;
        _isPaused               = false;

        // Advance day-of-season and day-of-week
        _currentDateTime.DayOfSeason++;
        _currentDateTime.DayOfWeek =
            (DayOfWeek)(((int)_currentDateTime.DayOfWeek + 1) % 7);

        // Season roll-over
        if (_currentDateTime.DayOfSeason > configuration.daysPerSeason)
        {
            _currentDateTime.DayOfSeason = 1;

            Season previousSeason   = _currentDateTime.Season;
            _currentDateTime.Season = (Season)(((int)_currentDateTime.Season + 1) % 4);

            // Year increment when rolling from Winter -> Spring
            if (_currentDateTime.Season == Season.Spring)
                _currentDateTime.Year++;
        }

        if (timeUI != null)
        {
            timeUI.RefreshCalender(_currentDateTime);
            timeUI.HandleTimeTick(_currentDateTime);
        }

        if (dayNightLightingController != null)
            dayNightLightingController.SetSunRotation(_currentDateTime.NormalizedDayTime);

        EventBus<OnNewDayStartedEvent>.Raise(new OnNewDayStartedEvent
        {
            NewDateTime = _currentDateTime,
            WasPassOut  = wasPassout
        });
    }
}
