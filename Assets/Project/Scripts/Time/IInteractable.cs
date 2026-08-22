using UnityEngine;

/// <summary>
/// Marks a scene object as interactable by the player.
///
/// Consumers should call <see cref="CanInteract"/> before <see cref="Interact"/>.
/// All implementations must reside on a dedicated component — do not merge with physics or combat logic.
/// </summary>
public interface IInteractable
{
    /// <summary>Human-readable prompt shown in the interaction UI (e.g. "Sleep (Rest until 6:00 AM)").</summary>
    string InteractionLabel { get; }

    /// <summary>
    /// Returns <c>true</c> when the interactor is currently allowed to trigger this object.
    /// Use this to gate interactions behind conditions (time of day, quest state, etc.).
    /// </summary>
    bool CanInteract(GameObject interactor);

    /// <summary>
    /// Executes the interaction logic. Only called after <see cref="CanInteract"/> returns <c>true</c>.
    /// </summary>
    void Interact(GameObject interactor);
}
