using UnityEngine;

public class DayNightLightingController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light             _sunDirectionalLight;
    [SerializeField] private TimeConfiguration _configuration;
    
    [Header("Rotation Settings")]
    [Tooltip("Pitch offset in degrees so that 0.0 normalised time (midnight) maps to the correct horizon position. " +
             "Default -90 places sunrise (~0.25) on the horizon.")]
    [SerializeField] private float _sunRotationOffsetDegrees = -90f;

    [Tooltip("Fixed yaw (Y-axis) of the sun orbit. Controls the compass direction of sunrise/sunset.")]
    [SerializeField] private float _sunYawDegrees = -30f;

    /// <summary>
    /// Smoothly updates the sun's pitch and rotation per frame using continuous normalized day time.
    /// </summary>
    /// <param name="continuousNormalizedTime">Continuous fraction of the day elapsed (0.0 to 1.0).</param>
    public void UpdateContinuousLighting(float continuousNormalizedTime)
    {
        if (_sunDirectionalLight == null)
            return;

        float sunPitch = (continuousNormalizedTime * 360f) + _sunRotationOffsetDegrees;
        _sunDirectionalLight.transform.rotation = Quaternion.Euler(sunPitch, _sunYawDegrees, 0f);
    }

    /// <summary>
    /// Immediately snaps the sun's rotation to the specified normalized day time (e.g. on new day or wake-up).
    /// </summary>
    public void SetSunRotation(float normalizedTime)
    {
        if (_sunDirectionalLight == null)
            return;

        float sunPitch = (normalizedTime * 360f) + _sunRotationOffsetDegrees;
        _sunDirectionalLight.transform.rotation = Quaternion.Euler(sunPitch, _sunYawDegrees, 0f);
    }

    /// <summary>
    /// Discrete tick callback kept for backward compatibility with external event callers.
    /// Continuous lighting updates are now driven per-frame via UpdateContinuousLighting.
    /// </summary>
    public void HandleTimeTick(float normalizedTime)
    {
        // No-op: per-frame continuous lighting handles smooth sun traversal.
    }
}
