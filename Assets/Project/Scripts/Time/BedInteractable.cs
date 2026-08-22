using UnityEngine;

namespace BanRaiValley.Time
{
    /// <summary>
    /// Interactable component placed on a bed GameObject.
    /// When the player interacts with it (via <see cref="IInteractable.Interact"/>),
    /// it delegates to <see cref="TimeManager.SleepToNextMorning"/> to end the current day.
    ///
    /// Setup:
    ///   1. Attach this component to the bed GameObject.
    ///   2. Assign the scene's <see cref="TimeManager"/> reference.
    ///   3. Ensure the player's interaction system calls <see cref="CanInteract"/> then <see cref="Interact"/>.
    /// </summary>
    public class BedInteractable : MonoBehaviour, IInteractable
    {
        // ----------------------------------------------------------------
        // Inspector Fields
        // ----------------------------------------------------------------

        [Tooltip("Text shown in the player's interaction prompt UI.")]
        [SerializeField] private string      _interactionLabel = "Sleep (Rest until 6:00 AM)";

        [SerializeField] private TimeManager _timeManager;

        // ================================================================
        // IInteractable Implementation
        // ================================================================

        /// <inheritdoc/>
        public string InteractionLabel => _interactionLabel;

        /// <inheritdoc/>
        /// <remarks>Beds are always interactable. Override this method to add conditions (e.g. only at night).</remarks>
        public bool CanInteract(GameObject interactor) => true;

        /// <inheritdoc/>
        /// <remarks>Calls <see cref="TimeManager.SleepToNextMorning"/> to advance the calendar to the next morning.</remarks>
        public void Interact(GameObject interactor)
        {
            if (_timeManager != null)
            {
                _timeManager.SleepToNextMorning();
            }
            else
            {
                Debug.LogWarning("[BedInteractable] TimeManager reference is missing.", this);
            }
        }
    }
}
