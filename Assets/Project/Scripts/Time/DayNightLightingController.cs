using UnityEngine;
using UnityEngine.Rendering;

namespace BanRaiValley.Time
{
    /// <summary>
    /// Drives all sky and sun lighting purely in response to <see cref="OnTimeTickEvent"/>.
    /// No polling in Update — all lighting changes happen exactly once per in-game minute tick.
    ///
    /// Responsibilities:
    ///   - Rotates the scene's Directional Sun Light along a 360-degree pitch arc per day.
    ///   - Evaluates sun colour and intensity from <see cref="TimeConfiguration"/> gradients/curves.
    ///   - Drives trilight ambient sky/equator/ground colours from TimeConfiguration gradients.
    ///   - Optionally disables soft shadows at night when sun intensity falls below a threshold.
    ///
    /// Setup:
    ///   1. Assign the scene Directional Light to <c>_sunDirectionalLight</c>.
    ///   2. Assign the <c>TimeConfiguration</c> ScriptableObject.
    ///   3. Ensure <see cref="TimeManager"/> is active in the scene and raising OnTimeTickEvent.
    /// </summary>
    public class DayNightLightingController : MonoBehaviour
    {
        // ----------------------------------------------------------------
        // Inspector Fields
        // ----------------------------------------------------------------

        [Header("References")]
        [SerializeField] private Light             _sunDirectionalLight;
        [SerializeField] private TimeConfiguration _configuration;

        [Header("Rotation Settings")]
        [Tooltip("Pitch offset in degrees so that 0.0 normalised time (midnight) maps to the correct horizon position. " +
                 "Default -90 places sunrise (~0.25) on the horizon.")]
        [SerializeField] private float _sunRotationOffsetDegrees = -90f;

        [Tooltip("Fixed yaw (Y-axis) of the sun orbit. Controls the compass direction of sunrise/sunset.")]
        [SerializeField] private float _sunYawDegrees = -30f;

        [Header("Shadow Control")]
        [Tooltip("When enabled, shadow casting is automatically disabled at night to improve performance.")]
        [SerializeField] private bool _disableShadowsAtNight = true;

        // ----------------------------------------------------------------
        // Constants
        // ----------------------------------------------------------------

        private const float SHADOW_INTENSITY_THRESHOLD = 0.05f;

        // ================================================================
        // Unity Lifecycle
        // ================================================================

        private void OnEnable()
        {
            EventBus<OnTimeTickEvent>.Subscribe(HandleTimeTick);
        }

        private void OnDisable()
        {
            EventBus<OnTimeTickEvent>.Unsubscribe(HandleTimeTick);
        }

        // ================================================================
        // Event Handler
        // ================================================================

        private void HandleTimeTick(OnTimeTickEvent evt)
        {
            EvaluateLighting(evt.NormalizedDayTime);
        }

        // ================================================================
        // Lighting Evaluation
        // ================================================================

        /// <summary>
        /// Applies sun rotation, colour, intensity, and ambient lighting based on the
        /// fractional time of day (0.0 = midnight, 0.5 = noon, 1.0 = next midnight).
        /// Can also be called directly for initialisation or editor preview.
        /// </summary>
        /// <param name="normalizedTime">Fractional day time in the range [0, 1].</param>
        public void EvaluateLighting(float normalizedTime)
        {
            if (_configuration == null)
                return;

            ApplySunRotation(normalizedTime);
            ApplySunLightProperties(normalizedTime);
            ApplyAmbientLighting(normalizedTime);
        }

        // ----------------------------------------------------------------
        // Sun Rotation
        // ----------------------------------------------------------------

        private void ApplySunRotation(float normalizedTime)
        {
            if (_sunDirectionalLight == null)
                return;

            float sunPitch = (normalizedTime * 360f) + _sunRotationOffsetDegrees;
            _sunDirectionalLight.transform.rotation = Quaternion.Euler(sunPitch, _sunYawDegrees, 0f);
        }

        // ----------------------------------------------------------------
        // Sun Light Properties
        // ----------------------------------------------------------------

        private void ApplySunLightProperties(float normalizedTime)
        {
            if (_sunDirectionalLight == null)
                return;

            if (_configuration.SunColorGradient != null)
                _sunDirectionalLight.color = _configuration.SunColorGradient.Evaluate(normalizedTime);

            if (_configuration.SunIntensityCurve != null)
            {
                float intensity = _configuration.SunIntensityCurve.Evaluate(normalizedTime);
                _sunDirectionalLight.intensity = intensity;

                if (_disableShadowsAtNight)
                {
                    _sunDirectionalLight.shadows = intensity > SHADOW_INTENSITY_THRESHOLD
                        ? LightShadows.Soft
                        : LightShadows.None;
                }
            }
        }

        // ----------------------------------------------------------------
        // Ambient Lighting
        // ----------------------------------------------------------------

        private void ApplyAmbientLighting(float normalizedTime)
        {
            if (_configuration.AmbientSkyColorGradient != null)
            {
                RenderSettings.ambientMode     = AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = _configuration.AmbientSkyColorGradient.Evaluate(normalizedTime);
            }

            if (_configuration.AmbientEquatorColorGradient != null)
                RenderSettings.ambientEquatorColor = _configuration.AmbientEquatorColorGradient.Evaluate(normalizedTime);

            if (_configuration.AmbientGroundColorGradient != null)
                RenderSettings.ambientGroundColor = _configuration.AmbientGroundColorGradient.Evaluate(normalizedTime);
        }
    }
}
