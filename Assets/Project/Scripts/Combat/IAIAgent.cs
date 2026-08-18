using UnityEngine;

/// <summary>
/// Contract for an AI-driven agent that can acquire and release a combat target.
/// Decouples higher-level systems (e.g., perception, decision) from the
/// concrete AI MonoBehaviour so cross-system coupling is avoided.
/// </summary>
public interface IAIAgent
{
    /// <summary>
    /// The Transform of the entity this agent is currently targeting.
    /// Returns <c>null</c> when no target is assigned.
    /// </summary>
    Transform TargetTransform { get; }

    /// <summary>
    /// The agent's current high-level behavioural state.
    /// State transitions must be emitted via <see cref="OnPlantStateChangedEvent"/> on the EventBus.
    /// </summary>
    PlantAIState CurrentState { get; }

    /// <summary>
    /// Assigns a new target to this agent. The implementation should
    /// update <see cref="TargetTransform"/> and trigger any internal
    /// state recalculation without polling.
    /// </summary>
    /// <param name="target">Transform of the target entity.</param>
    void SetTarget(Transform target);

    /// <summary>
    /// Removes the current target. The agent should return to an
    /// idle or patrol state via the state machine after clearing.
    /// </summary>
    void ClearTarget();
}
