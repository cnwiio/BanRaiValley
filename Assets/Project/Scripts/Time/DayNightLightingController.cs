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
    
    [Header("Smoothing")]
    [SerializeField] private float _rotationSmoothSpeed = 15f;
    
    private Quaternion _targetSunRotation;
    private bool _hasTarget;

    public void HandleTimeTick(float normalizedTime)
    {
        EvaluateLighting(normalizedTime);
    }

    private void EvaluateLighting(float normalizedTime)
    {
        if (_configuration == null)
            return;

        UpdateSunTargetRotation(normalizedTime);
    }

    private void UpdateSunTargetRotation(float normalizedTime)
    {
        float sunPitch = (normalizedTime * 360f) + _sunRotationOffsetDegrees;
        _targetSunRotation = Quaternion.Euler(sunPitch, _sunYawDegrees, 0f);
        _hasTarget = true;
    }

    public void SetSunRotation(float normalizedTime)
    {
        float sunPitch = (normalizedTime * 360f) + _sunRotationOffsetDegrees;
        _sunDirectionalLight.transform.rotation = Quaternion.Euler(sunPitch, _sunYawDegrees, 0f);
        _hasTarget = false;
    }

    private void Update()
    {
        if (!_sunDirectionalLight || !_hasTarget)
            return;

        float t = 1f - Mathf.Exp(-_rotationSmoothSpeed * Time.deltaTime);
        _sunDirectionalLight.transform.rotation = Quaternion.Slerp(
            _sunDirectionalLight.transform.rotation,
            _targetSunRotation,
            t
        );
    }

}
