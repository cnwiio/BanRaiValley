using System;
using UnityEngine;
using BanRaiValley.Time;

/// <summary>
/// Central time-simulation service for the Day/Night Cycle and Calendar system.
///
/// Responsibilities:
///   - Accumulates real-world delta time and converts it to in-game minutes.
///   - Raises EventBus events for every minute tick, hour change, day end, season change, etc.
///   - Handles voluntary sleep (SleepToNextMorning) and forced passout (ForcePassout / 02:00 AM).
///   - Exposes PauseTime() and a TimeScaleMultiplier for fast-forward / slow-motion time.
///
/// Dependencies (assign in Inspector):
///   - TimeConfiguration ScriptableObject
///
/// Attach this MonoBehaviour to a persistent scene manager GameObject.
/// Subscribe to EventBus events (OnTimeTickEvent, OnNewDayStartedEvent, etc.) — never poll CurrentDateTime.
/// </summary>
public class TimeManager : MonoBehaviour
{
    // ----------------------------------------------------------------
    // Inspector Fields
    // ----------------------------------------------------------------

    [Header("Configuration")]
    [SerializeField] private TimeConfiguration _configuration;

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

    // ================================================================
    // Unity Lifecycle
    // ================================================================

    private void Awake()
    {
        _currentDateTime = GameDateTime.InitialDate;

        if (!_autoStartOnAwake)
            _isPaused = true;
    }

    private void OnEnable()
    {
        EventBus<InventoryToggleEvent>.Subscribe(OnInventoryToggled);
    }

    private void OnDisable()
    {
        EventBus<InventoryToggleEvent>.Unsubscribe(OnInventoryToggled);
    }

    private void Update()
    {
        if (_isPaused || _configuration == null)
            return;

        _minuteAccumulator += Time.deltaTime * _timeScaleMultiplier;

        float secondsPerTick = _configuration.RealSecondsPerGameMinute
                               * _configuration.MinuteTickInterval;

        while (_minuteAccumulator >= secondsPerTick)
        {
            _minuteAccumulator -= secondsPerTick;
            AdvanceMinute(_configuration.MinuteTickInterval);
        }
    }

    // ================================================================
    // Time Advancement
    // ================================================================

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
            EventBus<OnHourChangedEvent>.Raise(new OnHourChangedEvent
            {
                PreviousHour    = previousHour,
                NewHour         = _currentDateTime.Hour,
                CurrentDateTime = _currentDateTime
            });

            // Passout check — triggers once per day at the configured hour
            if (_currentDateTime.Hour == _configuration.PassoutHour && !_hasPassedOutToday)
            {
                _hasPassedOutToday = true;
                ForcePassout();
                return; // ForcePassout resets state; exit to avoid stale tick event
            }
        }

        EventBus<OnTimeTickEvent>.Raise(new OnTimeTickEvent
        {
            CurrentDateTime  = _currentDateTime,
            NormalizedDayTime = _currentDateTime.NormalizedDayTime
        });
    }

    // ================================================================
    // Day Transition
    // ================================================================

    /// <summary>
    /// Call this when the player chooses to sleep voluntarily.
    /// Ends the current day and begins a new one at the configured start hour.
    /// </summary>
    public void SleepToNextMorning()
    {
        EventBus<OnDayEndedEvent>.Raise(new OnDayEndedEvent
        {
            EndedDateTime = _currentDateTime,
            IsPassout     = false
        });

        AdvanceToNextDay(wasPassout: false);
    }

    /// <summary>
    /// Triggers a forced passout (player was still awake at the passout hour).
    /// Applies a stamina penalty and transitions to the next day.
    /// </summary>
    public void ForcePassout()
    {
        EventBus<OnPlayerPassedOutEvent>.Raise(new OnPlayerPassedOutEvent
        {
            PassoutTime           = _currentDateTime,
            StaminaPenaltyPercent = 0.5f
        });

        EventBus<OnDayEndedEvent>.Raise(new OnDayEndedEvent
        {
            EndedDateTime = _currentDateTime,
            IsPassout     = true
        });

        AdvanceToNextDay(wasPassout: true);
    }

    /// <summary>
    /// Shared calendar advancement logic used by both sleep and passout paths.
    /// </summary>
    private void AdvanceToNextDay(bool wasPassout)
    {
        // Reset clock to start of new day
        _currentDateTime.Hour   = _configuration.StartHour;
        _currentDateTime.Minute = 0;
        _minuteAccumulator      = 0f;
        _hasPassedOutToday      = false;

        // Advance day-of-season and day-of-week
        _currentDateTime.DayOfSeason++;
        _currentDateTime.DayOfWeek =
            (DayOfWeek)(((int)_currentDateTime.DayOfWeek + 1) % 7);

        // Season roll-over
        if (_currentDateTime.DayOfSeason > _configuration.DaysPerSeason)
        {
            _currentDateTime.DayOfSeason = 1;

            Season previousSeason    = _currentDateTime.Season;
            _currentDateTime.Season  = (Season)(((int)_currentDateTime.Season + 1) % 4);

            // Year increment when rolling from Winter -> Spring
            if (_currentDateTime.Season == Season.Spring)
                _currentDateTime.Year++;

            EventBus<OnSeasonChangedEvent>.Raise(new OnSeasonChangedEvent
            {
                PreviousSeason = previousSeason,
                NewSeason      = _currentDateTime.Season,
                Year           = _currentDateTime.Year
            });
        }

        EventBus<OnNewDayStartedEvent>.Raise(new OnNewDayStartedEvent
        {
            NewDateTime = _currentDateTime,
            WasPassout  = wasPassout
        });

        // Emit the first tick of the new day so listeners (lighting, HUD) update immediately
        EventBus<OnTimeTickEvent>.Raise(new OnTimeTickEvent
        {
            CurrentDateTime   = _currentDateTime,
            NormalizedDayTime = _currentDateTime.NormalizedDayTime
        });
    }

    // ================================================================
    // Pause Control
    // ================================================================

    /// <summary>
    /// Pauses or resumes the time simulation. Raises OnTimePausedStateChangedEvent
    /// only when the state actually changes.
    /// </summary>
    public void PauseTime(bool isPaused)
    {
        if (_isPaused == isPaused)
            return;

        _isPaused = isPaused;

        EventBus<OnTimePausedStateChangedEvent>.Raise(new OnTimePausedStateChangedEvent
        {
            IsPaused = isPaused
        });
    }

    /// <summary>
    /// Adjusts how fast the simulation runs relative to real time.
    /// 1.0 = normal speed, 2.0 = double speed, etc.
    /// </summary>
    public void SetTimeScaleMultiplier(float multiplier)
    {
        _timeScaleMultiplier = Mathf.Max(0f, multiplier);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// Editor / Development-build only. Immediately advances the in-game clock
    /// by the given number of minutes, firing all appropriate EventBus events.
    /// Used by TimeDebugController for keyboard-shortcut time skipping.
    /// </summary>
    public void DebugAdvanceMinutes(int minutes)
    {
        AdvanceMinute(minutes);
    }
#endif

    // ================================================================
    // Event Handlers
    // ================================================================

    private void OnInventoryToggled(InventoryToggleEvent evt)
    {
        // Auto-pause time while the inventory is open
        PauseTime(!_isPaused);
    }
}
