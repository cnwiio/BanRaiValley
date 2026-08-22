using UnityEngine;
using BanRaiValley.Time;

namespace BanRaiValley.Time
{
    /// <summary>
    /// Developer-only keyboard shortcut controller for the Time System.
    /// Provides hotkeys to skip hours, jump to next morning, toggle pause, and fast-forward.
    ///
    /// This component is active only in UNITY_EDITOR and DEVELOPMENT_BUILD.
    /// Attach alongside TimeManager on a scene manager GameObject.
    ///
    /// Default hotkeys (all reassignable in Inspector):
    ///   F5 — Skip 1 hour forward
    ///   F6 — Sleep to next morning (full day transition)
    ///   F7 — Toggle time pause
    ///   F8 — Toggle fast-forward (1x ↔ configured multiplier)
    /// </summary>
    public class TimeDebugController : MonoBehaviour
    {
        // ----------------------------------------------------------------
        // Inspector Fields
        // ----------------------------------------------------------------

        [Header("References")]
        [Tooltip("The TimeManager to control. Assign in Inspector.")]
        [SerializeField] private TimeManager _timeManager;

        [Header("Toggle")]
        [Tooltip("Master switch — set false to disable all hotkeys without removing the component.")]
        [SerializeField] private bool _enableDebugHotkeys = true;

        [Header("Hotkeys")]
        [Tooltip("Advance in-game time by 60 minutes.")]
        [SerializeField] private KeyCode _skipOneHourKey = KeyCode.F5;

        [Tooltip("Immediately transition to the next morning (triggers full day-end / day-start cycle).")]
        [SerializeField] private KeyCode _skipToMorningKey = KeyCode.F6;

        [Tooltip("Toggle time simulation pause on / off.")]
        [SerializeField] private KeyCode _togglePauseKey = KeyCode.F7;

        [Tooltip("Toggle between 1× and the fast-forward multiplier.")]
        [SerializeField] private KeyCode _toggleFastForwardKey = KeyCode.F8;

        [Header("Fast-Forward")]
        [Tooltip("Time scale multiplier used when fast-forward is active (e.g. 10 = 10× speed).")]
        [SerializeField] private float _fastForwardMultiplier = 10f;

        // ----------------------------------------------------------------
        // Private State
        // ----------------------------------------------------------------

        private bool _isFastForwarding;

        // ================================================================
        // Unity Lifecycle
        // ================================================================

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!_enableDebugHotkeys || _timeManager == null)
                return;

            if (Input.GetKeyDown(_skipOneHourKey))
                SkipOneHour();

            if (Input.GetKeyDown(_skipToMorningKey))
                SkipToMorning();

            if (Input.GetKeyDown(_togglePauseKey))
                TogglePause();

            if (Input.GetKeyDown(_toggleFastForwardKey))
                ToggleFastForward();
#endif
        }

        // ================================================================
        // Debug Actions
        // ================================================================

        /// <summary>
        /// Advances in-game time by exactly 60 minutes, triggering all intermediate
        /// EventBus events (OnTimeTickEvent, OnHourChangedEvent, etc.).
        /// </summary>
        private void SkipOneHour()
        {
            Debug.Log("[TimeDebugController] F5: Skipping 1 hour forward.");
            _timeManager.DebugAdvanceMinutes(60);
        }

        /// <summary>
        /// Transitions to the next morning immediately (full day-end / day-start cycle).
        /// </summary>
        private void SkipToMorning()
        {
            Debug.Log("[TimeDebugController] F6: Sleeping to next morning.");
            _timeManager.SleepToNextMorning();
        }

        /// <summary>
        /// Toggles the time simulation between paused and running.
        /// </summary>
        private void TogglePause()
        {
            bool nextState = !_timeManager.IsPaused;
            _timeManager.PauseTime(nextState);
            Debug.Log($"[TimeDebugController] F7: Time {(nextState ? "PAUSED" : "RESUMED")}.");
        }

        /// <summary>
        /// Toggles time scale between 1× (normal) and the configured fast-forward multiplier.
        /// </summary>
        private void ToggleFastForward()
        {
            _isFastForwarding = !_isFastForwarding;
            float multiplier = _isFastForwarding ? _fastForwardMultiplier : 1f;
            _timeManager.SetTimeScaleMultiplier(multiplier);
            Debug.Log($"[TimeDebugController] F8: Fast-forward {(_isFastForwarding ? "ON" : "OFF")} " +
                      $"({multiplier}×).");
        }
    }
}
