/// <summary>
/// Defines all discrete behaviour states for the Plant AI state machine.
/// Each value maps 1-to-1 with a PlantAIStateMachine transition.
/// </summary>
public enum PlantAIState
{
    /// <summary>Buried / static crop — monster is inactive and hidden underground.</summary>
    Dormant = 0,

    /// <summary>Uprooting animation — monster is transitioning from buried to active.</summary>
    Awakening = 1,

    /// <summary>Wandering / waiting — monster is active but has no target.</summary>
    Idle = 2,

    /// <summary>Pursuing target — monster is actively chasing a detected player.</summary>
    Chase = 3,

    /// <summary>Executing combat attack — monster is within attack range and striking.</summary>
    Attack = 4,

    /// <summary>Damage stagger / flinch — monster is recovering from a received hit.</summary>
    HitReact = 5,

    /// <summary>Defeated, awaiting despawn — monster has died and is pending cleanup.</summary>
    Dead = 6,
}
