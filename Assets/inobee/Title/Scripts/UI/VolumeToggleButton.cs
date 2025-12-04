using UnityEngine;
using UnityEngine.UI;

namespace Title.UI
{
    public class VolumeToggleButton : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button toggleButton;
        [SerializeField] private Image iconImage;

        [Header("Icons (Optional)")]
        [SerializeField] private Sprite iconVolumeOn;
        [SerializeField] private Sprite iconVolumeOff;

        private const string PlayerPrefsKeyMuted = "title_volume_muted";

        private bool isMuted;

        private void Awake()
        {
            // Load persisted state (default: not muted)
            isMuted = PlayerPrefs.GetInt(PlayerPrefsKeyMuted, 0) == 1;
            ApplyVolumeState(isMuted);

            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(OnToggleClicked);
            }
        }

        private void OnDestroy()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(OnToggleClicked);
            }
        }

        private void OnToggleClicked()
        {
            isMuted = !isMuted;
            ApplyVolumeState(isMuted);
            PlayerPrefs.SetInt(PlayerPrefsKeyMuted, isMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void ApplyVolumeState(bool muted)
        {
            // Simple global mute. If you use AudioMixer, replace with exposed parameter logic.
            AudioListener.pause = muted;
            AudioListener.volume = muted ? 0f : 1f;

            if (iconImage != null)
            {
                if (muted && iconVolumeOff != null)
                {
                    iconImage.sprite = iconVolumeOff;
                }
                else if (!muted && iconVolumeOn != null)
                {
                    iconImage.sprite = iconVolumeOn;
                }
            }
        }
    }
}


