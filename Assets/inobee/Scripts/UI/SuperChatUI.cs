using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// スーパーチャット演出UI
    /// </summary>
    public class SuperChatUI : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private float _displayDuration = 3.0f;

        private Sequence _displaySequence;

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public void ShowSuperChat(int amount, string message)
        {
            _displaySequence?.Kill();

            if (_amountText != null)
                _amountText.text = $"¥{amount:N0}";

            if (_messageText != null)
                _messageText.text = message;

            gameObject.SetActive(true);

            // スライドイン演出
            RectTransform rect = GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(300f, rect.anchoredPosition.y);

            _displaySequence = DOTween.Sequence()
                .Append(rect.DOAnchorPosX(0f, 0.5f).SetEase(Ease.OutBack))
                .AppendInterval(_displayDuration)
                .Append(rect.DOAnchorPosX(300f, 0.3f).SetEase(Ease.InBack))
                .OnComplete(() => gameObject.SetActive(false));
        }

        public void OnEmoteMatched(int matchCount)
        {
            // 大きいマッチでスパチャ演出
            if (matchCount >= 5)
            {
                int amount = matchCount * 200;
                string[] messages = { "応援してます！", "最高！", "すごい！", "がんばって！" };
                string message = messages[Random.Range(0, messages.Length)];
                
                ShowSuperChat(amount, message);
            }
        }

        private void OnDestroy()
        {
            _displaySequence?.Kill();
        }
    }
}