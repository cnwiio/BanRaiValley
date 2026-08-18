/// <summary>
/// Defines the category of damage inflicted in combat interactions.
/// Used by <see cref="DamageData"/> to allow systems to react
/// differently based on damage category (e.g., resistances, VFX).
/// </summary>
public enum DamageType
{
    /// <summary>Generic physical damage with no sub-type.</summary>
    Physical = 0,

    /// <summary>Cutting or slicing damage (blades, claws).</summary>
    Slashing = 1,

    /// <summary>Crushing or impact damage (hammers, clubs).</summary>
    Blunt = 2,

    /// <summary>Puncture damage (arrows, spears).</summary>
    Piercing = 3,

    /// <summary>Fire-element damage.</summary>
    Fire = 4,

    /// <summary>Water-element damage.</summary>
    Water = 5,
}
