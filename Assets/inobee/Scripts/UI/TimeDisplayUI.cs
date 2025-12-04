using UnityEngine;
using TMPro;
using EmoteOrchestra.Core;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// 曲の再生時間表示
    /// </summary>
    public class TimeDisplayUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timeText;

        private void Update()
        {

            MusicGameManager gameManager = ServiceLocator.Get<MusicGameManager>();
            if (gameManager == null || gameManager.CurrentSong == null)
            {
                if (_timeText != null)
                    _timeText.text = "0:00 / 0:00";
                return;
            }

            if (_timeText != null)
            {
                string currentTime = gameManager.GetCurrentTimeString();
                string totalTime = gameManager.CurrentSong.GetDurationString();
                _timeText.text = $"{currentTime} / {totalTime}";
            }
        }
    }
}