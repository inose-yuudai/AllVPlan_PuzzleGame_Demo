using UnityEngine;
using TMPro;
using DG.Tweening;
using EmoteOrchestra.Core;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// スコア表示UI
    /// </summary>
    public class ScoreDisplayUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreValueText;
        [SerializeField] private float _countUpDuration = 0.5f;

        private int _currentDisplayScore;
        private Tweener _countUpTweener;

        public void OnEmoteMatched(int addScore)
        {
            MusicGameManager gameManager = ServiceLocator.Get<MusicGameManager>();
            if (gameManager != null)
            {
                gameManager.AddScore(addScore);
                UpdateScoreDisplay(gameManager.CurrentScore);
            }
        }

        private void UpdateScoreDisplay(int targetScore)
        {
            _countUpTweener?.Kill();

            _countUpTweener = DOTween.To(
                () => _currentDisplayScore,
                x => _currentDisplayScore = x,
                targetScore,
                _countUpDuration
            ).OnUpdate(() =>
            {
                if (_scoreValueText != null)
                {
                    _scoreValueText.text = _currentDisplayScore.ToString("N0");
                }
            });
        }

        private void OnDestroy()
        {
            _countUpTweener?.Kill();
        }
    }
}