using UnityEngine;
using UnityEngine.UI;
using TwinsDefense.Systems;

namespace TwinsDefense.UI
{
    /// <summary>
    /// Binds a UI Slider to ProjectileOpacitySettings. Drop this on a Slider in
    /// both the Main Menu Settings panel and the Arena Run pause menu — either
    /// one persists the value immediately via PlayerPrefs, and every live
    /// Projectile/StarProjectile picks it up through ProjectileOpacitySettings.OnChanged.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class ProjectileOpacitySliderUI : MonoBehaviour
    {
        private Slider slider;

        private void Awake()
        {
            slider = GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
        }

        private void OnEnable()
        {
            slider.SetValueWithoutNotify(ProjectileOpacitySettings.Value);
            slider.onValueChanged.AddListener(HandleValueChanged);
        }

        private void OnDisable()
        {
            slider.onValueChanged.RemoveListener(HandleValueChanged);
        }

        private static void HandleValueChanged(float value)
        {
            ProjectileOpacitySettings.SetValue(value);
        }
    }
}
