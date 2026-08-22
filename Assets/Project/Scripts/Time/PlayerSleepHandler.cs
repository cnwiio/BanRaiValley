using UnityEngine;

namespace BanRaiValley.Time
{
    /// <summary>
    /// Listens to day-transition events and handles all player-side consequences of sleeping or passing out:
    ///   - Repositions the player to the designated wake-up spawn point.
    ///   - Applies a 50% max-stamina penalty when the player passed out (vs. 100% recovery for voluntary sleep).
    ///
    /// This component is intentionally decoupled from <see cref="TimeManager"/>; it only reacts to EventBus events.
    /// When a Stamina system is implemented, replace the TODO stubs below with concrete stamina component calls.
    ///
    /// Setup:
    ///   1. Attach to a persistent player-management GameObject.
    ///   2. Assign <c>_playerTransform</c> (the root player Transform).
    ///   3. Assign <c>_wakeUpSpawnPoint</c> (an empty Transform placed at the bed's wake-up position).
    /// </summary>
    public class PlayerSleepHandler : MonoBehaviour
    {
        // ----------------------------------------------------------------
        // Inspector Fields
        // ----------------------------------------------------------------

        [Header("Spawn Point")]
        [Tooltip("The Transform the player will be moved to at the start of each new day.")]
        [SerializeField] private Transform _wakeUpSpawnPoint;

        [Header("Player Reference")]
        [SerializeField] private Transform _playerTransform;

        // ----------------------------------------------------------------
        // Private State
        // ----------------------------------------------------------------

        /// <summary>
        /// Set to <c>true</c> when a passout event is received so the penalty
        /// is applied when the new day event arrives (they may arrive in the same frame).
        /// </summary>
        private bool _hasPendingPassoutPenalty;

        // ================================================================
        // Unity Lifecycle
        // ================================================================

        private void OnEnable()
        {
            EventBus<OnPlayerPassedOutEvent>.Subscribe(HandlePlayerPassedOut);
            EventBus<OnNewDayStartedEvent>.Subscribe(HandleNewDayStarted);
        }

        private void OnDisable()
        {
            EventBus<OnPlayerPassedOutEvent>.Unsubscribe(HandlePlayerPassedOut);
            EventBus<OnNewDayStartedEvent>.Unsubscribe(HandleNewDayStarted);
        }

        // ================================================================
        // Event Handlers
        // ================================================================

        private void HandlePlayerPassedOut(OnPlayerPassedOutEvent evt)
        {
            _hasPendingPassoutPenalty = true;
            Debug.Log("[PlayerSleepHandler] Player passed out at 2:00 AM. 50% stamina penalty applied.");
        }

        private void HandleNewDayStarted(OnNewDayStartedEvent evt)
        {
            RepositionPlayer();

            if (evt.WasPassout || _hasPendingPassoutPenalty)
            {
                ApplyPassoutStaminaRecovery();
                _hasPendingPassoutPenalty = false;
            }
            else
            {
                ApplyFullStaminaRecovery();
            }
        }

        // ================================================================
        // Player Repositioning
        // ================================================================

        private void RepositionPlayer()
        {
            if (_wakeUpSpawnPoint == null || _playerTransform == null)
            {
                Debug.LogWarning("[PlayerSleepHandler] Wake-up spawn point or player Transform is not assigned.", this);
                return;
            }

            _playerTransform.position = _wakeUpSpawnPoint.position;
            _playerTransform.rotation = _wakeUpSpawnPoint.rotation;
        }

        // ================================================================
        // Stamina Recovery
        // ================================================================

        /// <summary>
        /// Restores the player to full stamina and HP after a voluntary sleep.
        /// Connect this to your StaminaComponent / HealthComponent when implemented.
        /// </summary>
        private void ApplyFullStaminaRecovery()
        {
            // TODO: staminaComponent.RestoreToMax();
            // TODO: healthComponent.RestoreToMax();
            Debug.Log("[PlayerSleepHandler] Full stamina and HP restored after sleep.");
        }

        /// <summary>
        /// Restores the player to only 50% of max stamina after a passout.
        /// Connect this to your StaminaComponent when implemented.
        /// </summary>
        private void ApplyPassoutStaminaRecovery()
        {
            // TODO: staminaComponent.RestoreToPercent(0.5f);
            // TODO: healthComponent.RestoreToMax(); // HP still fully restored
            Debug.Log("[PlayerSleepHandler] 50% stamina restored after passout penalty.");
        }
    }
}
