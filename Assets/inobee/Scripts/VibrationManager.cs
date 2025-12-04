using UnityEngine;
using UnityEngine.InputSystem;

namespace EmoteOrchestra.Input
{
    /// <summary>
    /// コントローラー振動管理
    /// </summary>
    public class VibrationManager : MonoBehaviour
    {
        [SerializeField] private bool _enableVibration = true;
        [SerializeField] private float _vibrationIntensity = 1.0f;

        public void PlayLightVibration()
        {
            if (!_enableVibration) return;

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                gamepad.SetMotorSpeeds(0.1f * _vibrationIntensity, 0.1f * _vibrationIntensity);
                Invoke(nameof(StopVibration), 0.05f);
            }
        }

        public void PlayMediumVibration()
        {
            if (!_enableVibration) return;

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                gamepad.SetMotorSpeeds(0.3f * _vibrationIntensity, 0.3f * _vibrationIntensity);
                Invoke(nameof(StopVibration), 0.1f);
            }
        }

        public void PlayHeavyVibration()
        {
            if (!_enableVibration) return;

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                gamepad.SetMotorSpeeds(0.6f * _vibrationIntensity, 0.6f * _vibrationIntensity);
                Invoke(nameof(StopVibration), 0.2f);
            }
        }

        private void StopVibration()
        {
            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                gamepad.SetMotorSpeeds(0f, 0f);
            }
        }

        private void OnDestroy()
        {
            StopVibration();
        }
    }
}