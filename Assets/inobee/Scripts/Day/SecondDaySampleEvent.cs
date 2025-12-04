using UnityEngine;
using TMPro;

namespace EmoteOrchestra.Core
{
    /// <summary>
    /// DayEventDefinition の UnityEvent から呼び出して使う 2日目専用サンプル。
    /// </summary>
    public class SecondDaySampleEvent : MonoBehaviour
    {
        [Header("表示先")]
        [SerializeField] private CanvasGroup _panelGroup;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _bodyText;

        [Header("文言")]
        [SerializeField] private string _title = "2日目イベント！";
        [SerializeField][TextArea] private string _body = "ようこそ2日目！\n特別なイベントが始まります。";

        [Header("追加演出 (任意)")]
        [SerializeField] private AudioSource _jingleSource;
        [SerializeField] private AudioClip _jingleClip;

        [Header("イベントID設定")]
        [SerializeField] private GameDayEventChannel _eventChannel;
        [SerializeField] private string _targetEventId = "Day2";

        private void OnEnable()
        {
            if (_eventChannel != null)
            {
                _eventChannel.OnEventTriggered += HandleEventTriggered;
            }
        }

        private void OnDisable()
        {
            if (_eventChannel != null)
            {
                _eventChannel.OnEventTriggered -= HandleEventTriggered;
            }
        }

        private void HandleEventTriggered(string eventId)
        {
            if (eventId == _targetEventId)
            {
                ShowSecondDayMessage();
            }
        }

        /// <summary>
        /// GameDayManager の DayEvent から呼び出す
        /// </summary>
        public void ShowSecondDayMessage()
        {
            ShowPanel(true);
            ApplyTexts();
            PlayJingle();
        }

        public void Hide()
        {
            ShowPanel(false);
        }

        private void ShowPanel(bool visible)
        {
            if (_panelGroup == null)
                return;

            _panelGroup.gameObject.SetActive(true);
            _panelGroup.alpha = visible ? 1f : 0f;
            _panelGroup.blocksRaycasts = visible;
            _panelGroup.interactable = visible;
        }

        private void ApplyTexts()
        {
            if (_titleText != null)
            {
                _titleText.text = _title;
            }

            if (_bodyText != null)
            {
                _bodyText.text = _body;
            }
        }

        private void PlayJingle()
        {
            if (_jingleSource == null || _jingleClip == null)
                return;

            _jingleSource.clip = _jingleClip;
            _jingleSource.Play();
        }
    }
}

