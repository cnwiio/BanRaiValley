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

    public GameDateTime CurrentDateTime   => _currentDateTime;
    public bool         IsPaused          => _isPaused;
    public float        TimeScaleMultiplier => _timeScaleMultiplier;

    private void OnEnable()
    {
        EventBus<OnSleepEvent>.Subscribe(OnSleep);
    }

    private void OnDisable()
    {
        EventBus<OnSleepEvent>.Unsubscribe(OnSleep);
    }

    private void OnSleep(OnSleepEvent evt)
    {
        AdvanceToNextDay(false);
    }
    
    
    private void Awake()
    {
        _currentDateTime = GameDateTime.InitialDate;
        

        if (!_autoStartOnAwake)
            _isPaused = true;
    }

    private void Start()
    {
        dayNightLightingController.HandleTimeTick(_currentDateTime.NormalizedDayTime);
        timeUI.HandleTimeTick(_currentDateTime);
    }

    private void FixedUpdate()
    {
        if (_isPaused || configuration == null)
            return;

        _minuteAccumulator += Time.deltaTime * _timeScaleMultiplier;

        float secondsPerTick = configuration.realSecondsPerGameMinute
                               * configuration.minuteTickInterval;

        while (_minuteAccumulator >= secondsPerTick)
        {
            _minuteAccumulator -= secondsPerTick;
            AdvanceMinute(configuration.minuteTickInterval);
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
            // EventBus<OnHourChangedEvent>.Raise(new OnHourChangedEvent
            // {
            //     PreviousHour    = previousHour,
            //     NewHour         = _currentDateTime.Hour,
            //     CurrentDateTime = _currentDateTime
            // });

            // Passout check — triggers once per day at the configured hour
            if (_currentDateTime.Hour == configuration.passoutHour && !_hasPassedOutToday)
            {
                _hasPassedOutToday = true;
                ForcePassout();
                return; // ForcePassout resets state; exit to avoid stale tick event
            }
        }
        
        dayNightLightingController.HandleTimeTick(_currentDateTime.NormalizedDayTime);
        timeUI.HandleTimeTick(_currentDateTime);
    }
    
    /// <summary>
    /// Call this when the player chooses to sleep voluntarily.
    /// Ends the current day and begins a new one at the configured start hour.
    /// </summary>
    public void SleepToNextMorning()
    {
        // EventBus<OnDayEndedEvent>.Raise(new OnDayEndedEvent
        // {
        //     EndedDateTime = _currentDateTime,
        //     IsPassOut     = false
        // });

        AdvanceToNextDay(wasPassout: false);
    }
    
    /// <summary>
    /// Triggers a forced passout (player was still awake at the passout hour).
    /// Applies a stamina penalty and transitions to the next day.
    /// </summary>
    public void ForcePassout()
    {
        // {
        //     PassoutTime           = _currentDateTime,
        //     StaminaPenaltyPercent = 0.5f
        // });
        //
        // EventBus<OnDayEndedEvent>.Raise(new OnDayEndedEvent
        // {
        //     EndedDateTime = _currentDateTime,
        //     IsPassout     = true
        // });
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

        // Advance day-of-season and day-of-week
        _currentDateTime.DayOfSeason++;
        _currentDateTime.DayOfWeek =
            (DayOfWeek)(((int)_currentDateTime.DayOfWeek + 1) % 7);

        // Season roll-over
        if (_currentDateTime.DayOfSeason > configuration.daysPerSeason)
        {
            _currentDateTime.DayOfSeason = 1;

            Season previousSeason    = _currentDateTime.Season;
            _currentDateTime.Season  = (Season)(((int)_currentDateTime.Season + 1) % 4);

            // Year increment when rolling from Winter -> Spring
            if (_currentDateTime.Season == Season.Spring)
                _currentDateTime.Year++;

            // EventBus<OnSeasonChangedEvent>.Raise(new OnSeasonChangedEvent
            // {
            //     PreviousSeason = previousSeason,
            //     NewSeason      = _currentDateTime.Season,
            //     Year           = _currentDateTime.Year
            // });
        }

        timeUI.RefreshCalender(_currentDateTime);
        dayNightLightingController.HandleTimeTick(_currentDateTime.NormalizedDayTime);
        timeUI.HandleTimeTick(_currentDateTime);
        // EventBus<OnNewDayStartedEvent>.Raise(new OnNewDayStartedEvent
        // {
        //     NewDateTime = _currentDateTime,
        //     WasPassOut  = wasPassout
        // });
        
        // Emit the first tick of the new day so listeners (lighting, HUD) update immediately
        // EventBus<OnTimeTickEvent>.Raise(new OnTimeTickEvent
        // {
        //     CurrentDateTime   = _currentDateTime,
        //     NormalizedDayTime = _currentDateTime.NormalizedDayTime
        // });
    }
}
