using UnityEngine;
using BanRaiValley.Time;

/// <summary>
/// ScriptableObject holding all designer-tunable parameters for the Day/Night Cycle system.
/// Create via: Assets > Create > BanRaiValley > Time > Time Configuration
/// </summary>
[CreateAssetMenu(fileName = "TimeConfiguration", menuName = "BanRaiValley/Time/Time Configuration")]
public class TimeConfiguration : ScriptableObject
{
    // ----------------------------------------------------------------
    // Simulation Timing
    // ----------------------------------------------------------------

    [Header("Simulation Timing")]
    [Tooltip("How many real-world seconds pass per in-game minute.")]
    [SerializeField] private float _realSecondsPerGameMinute = 0.75f;

    [Tooltip("How many in-game minutes advance per timer tick.")]
    [SerializeField] private int _minuteTickInterval = 1;

    // ----------------------------------------------------------------
    // Day Boundaries
    // ----------------------------------------------------------------

    [Header("Day Boundaries")]
    [Tooltip("The hour (0-23) at which a new day begins after sleep.")]
    [SerializeField] private int _startHour = 6;

    [Tooltip("The hour (0-23) at which the player is forcibly passed out.")]
    [SerializeField] private int _passoutHour = 2;

    [Tooltip("Number of in-game days per season.")]
    [SerializeField] private int _daysPerSeason = 30;

    // ----------------------------------------------------------------
    // Lighting & Sky
    // ----------------------------------------------------------------

    [Header("Sun Lighting")]
    [Tooltip("Colour of the directional sun light sampled by normalised day time (0=midnight, 1=midnight).")]
    [SerializeField] private Gradient _sunColorGradient;

    [Tooltip("Intensity of the directional sun light sampled by normalised day time.")]
    [SerializeField] private AnimationCurve _sunIntensityCurve;

    [Header("Ambient Sky Colours")]
    [SerializeField] private Gradient _ambientSkyColorGradient;
    [SerializeField] private Gradient _ambientEquatorColorGradient;
    [SerializeField] private Gradient _ambientGroundColorGradient;

    [Header("Sun Rotation")]
    [Tooltip("Euler offset applied on top of the calculated sun orbit rotation.")]
    [SerializeField] private Vector3 _sunRotationEulerOffset = new Vector3(50f, -30f, 0f);

    // ----------------------------------------------------------------
    // Season Icons
    // ----------------------------------------------------------------

    [Header("Season Icons")]
    [SerializeField] private Sprite _springIcon;
    [SerializeField] private Sprite _summerIcon;
    [SerializeField] private Sprite _fallIcon;
    [SerializeField] private Sprite _winterIcon;

    // ================================================================
    // Public Getters
    // ================================================================

    public float RealSecondsPerGameMinute => _realSecondsPerGameMinute;
    public int   MinuteTickInterval       => _minuteTickInterval;
    public int   StartHour                => _startHour;
    public int   PassoutHour              => _passoutHour;
    public int   DaysPerSeason            => _daysPerSeason;

    public Gradient       SunColorGradient            => _sunColorGradient;
    public AnimationCurve SunIntensityCurve           => _sunIntensityCurve;
    public Gradient       AmbientSkyColorGradient     => _ambientSkyColorGradient;
    public Gradient       AmbientEquatorColorGradient => _ambientEquatorColorGradient;
    public Gradient       AmbientGroundColorGradient  => _ambientGroundColorGradient;
    public Vector3        SunRotationEulerOffset       => _sunRotationEulerOffset;

    // ================================================================
    // Helper Methods
    // ================================================================

    /// <summary>
    /// Returns the UI sprite associated with the given season.
    /// </summary>
    public Sprite GetSeasonIcon(Season season)
    {
        return season switch
        {
            Season.Spring => _springIcon,
            Season.Summer => _summerIcon,
            Season.Fall   => _fallIcon,
            Season.Winter => _winterIcon,
            _             => null
        };
    }
}
