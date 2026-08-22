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
    
    public void HandleTimeTick(float normalizedTime)
    {
        EvaluateLighting(normalizedTime);
    }
    
    private void EvaluateLighting(float normalizedTime)
    {
        if (_configuration == null)
            return;

        ApplySunRotation(normalizedTime);
    }
    
    private void ApplySunRotation(float normalizedTime)
    {
        if (_sunDirectionalLight == null)
            return;

        float sunPitch = (normalizedTime * 360f) + _sunRotationOffsetDegrees;
        _sunDirectionalLight.transform.rotation = Quaternion.Euler(sunPitch, _sunYawDegrees, 0f);
    }

}
