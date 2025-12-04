using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace EmoteOrchestra.Mission
{
    /// <summary>
    /// ミッションUI（テキスト + チェック + フェードアウト対応）
    /// </summary>
    public class MissionUIItem : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Image _checkMarkImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _iconImage; // アイコン使わないなら外してOK

        private MissionProgress _mission;
        private Sequence _animationSequence;

        // ★今この枠が達成済みかどうかをMissionUIManagerから見れるようにする
        public bool IsCompleted { get; private set; }

        public void Initialize(MissionProgress mission)
        {
            _mission = mission;
            IsCompleted = false;

            // タイトル（説明を表示）
            if (_titleText != null)
            {
                _titleText.text = mission.Data.missionDescription;
            }

            // 背景色
            if (_backgroundImage != null)
            {
                _backgroundImage.color = mission.Data.missionColor;
            }

            // アイコン（MissionData側にSpriteを足してる前提）
            if (_iconImage != null)
            {
                if (mission.Data.missionIcon != null)
                {
                    _iconImage.sprite = mission.Data.missionIcon;
                    _iconImage.gameObject.SetActive(true);
                }
                else
                {
                    _iconImage.gameObject.SetActive(false);
                }
            }

            // チェックマークは最初隠す
            if (_checkMarkImage != null)
            {
                _checkMarkImage.enabled = false;
                _checkMarkImage.transform.localScale = Vector3.one;
            }

            // 再利用されることがあるので見た目リセット
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
            transform.localScale = Vector3.one;

            PlayShowAnimation();
        }

        public void UpdateProgress()
        {
            // 今は表示する進捗なし
        }

        public void PlayShowAnimation()
        {
            _animationSequence?.Kill();

            transform.localScale = Vector3.zero;
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;

            _animationSequence = DOTween.Sequence()
                .Append(transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack))
                .Join(_canvasGroup.DOFade(1f, 0.25f));
        }

        /// <summary>
        /// ★1個だけ達成したとき用：チェックを付けるだけで消さない
        /// </summary>
        public void MarkCompleted()
        {
            IsCompleted = true;

            if (_checkMarkImage != null)
            {
                _checkMarkImage.enabled = true;
                _checkMarkImage.transform.localScale = Vector3.zero;

                _checkMarkImage.transform
                    .DOScale(1.2f, 0.2f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        _checkMarkImage.transform.DOScale(1f, 0.1f);
                    });
            }
        }

        /// <summary>
        /// ★2個そろってMissionUIManagerが「消していいよ」って言ってきたとき用
        /// </summary>
        public void PlayCompleteAnimation(System.Action onComplete)
        {
            _animationSequence?.Kill();

            // ここではすでにMarkCompletedされてる想定なので、そのままフェードアウトでOK
            _animationSequence = DOTween.Sequence()
                .Append(transform.DOScale(0.8f, 0.25f).SetEase(Ease.InBack))
                .Join(_canvasGroup.DOFade(0f, 0.25f))
                .OnComplete(() => onComplete?.Invoke());
        }

        private void OnDestroy()
        {
            _animationSequence?.Kill();
        }
    }
}
