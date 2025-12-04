using UnityEngine;
using TMPro;
using DG.Tweening;

namespace EmoteOrchestra.Vtuber
{
    /// <summary>
    /// Vtuberのリアクション管理
    /// </summary>
    public class ReactionSystem : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _reactionText;
        [SerializeField] private CanvasGroup _reactionCanvasGroup;

        [Header("リアクションメッセージ")]
        [SerializeField] private string[] _smallReactions = { "ありがとう♪", "嬉しい！", "いいね！" };
        [SerializeField] private string[] _mediumReactions = { "すごい！", "最高！", "やったー！" };
        [SerializeField] private string[] _largeReactions = { "最高！！", "すごすぎる！", "感動！！" };

        private Sequence _currentSequence;

        public void PlayReaction(int matchCount)
        {
            _currentSequence?.Kill();

            string message = GetReactionMessage(matchCount);
            ShowReactionText(message);
        }

        private string GetReactionMessage(int matchCount)
        {
            if (matchCount >= 5)
            {
                return _largeReactions[Random.Range(0, _largeReactions.Length)];
            }
            else if (matchCount >= 4)
            {
                return _mediumReactions[Random.Range(0, _mediumReactions.Length)];
            }
            else
            {
                return _smallReactions[Random.Range(0, _smallReactions.Length)];
            }
        }

        private void ShowReactionText(string message)
        {
            if (_reactionText == null || _reactionCanvasGroup == null)
                return;

            _reactionText.text = message;
            _reactionCanvasGroup.alpha = 0f;

            _currentSequence = DOTween.Sequence()
                .Append(_reactionCanvasGroup.DOFade(1f, 0.3f))
                .AppendInterval(1.5f)
                .Append(_reactionCanvasGroup.DOFade(0f, 0.5f));
        }

        private void OnDestroy()
        {
            _currentSequence?.Kill();
        }
    }
}