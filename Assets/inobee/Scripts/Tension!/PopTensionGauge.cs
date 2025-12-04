using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// ポップなテンションゲージ（状態変化・枠付き）
    /// </summary>
    public class PopTensionGauge : MonoBehaviour
    {
        [Header("ゲージ要素")]
        [SerializeField] private BentGaugeMesh _mainGauge;
        [SerializeField] private BentGaugeMesh _shadowGauge;
        [SerializeField] private Image _frame; // 枠
        [SerializeField] private TextMeshProUGUI _stateText;
        [SerializeField] private TextMeshProUGUI _valueText;

        [Header("テンション設定")]
        [SerializeField] private float _maxTension = 100f;
        [SerializeField] private float _initialTension = 50f;
		[SerializeField] private float _decreasePerSecond = 0.2f;
		[SerializeField, Tooltip("最大値から最小値まで放置で落ち切る目安秒数")] private float _targetSecondsToZeroFromMax = 40f;
		[SerializeField, Tooltip("High状態のとき減衰に掛かる倍率")] private float _highStateDecayMultiplier = 1.5f;
		[SerializeField, Tooltip("Hyper状態のとき減衰に掛かる倍率")] private float _hyperStateDecayMultiplier = 2.5f;
		[SerializeField, Tooltip("テキストコメントでテンションを増やすかどうか")] private bool _textCommentsAffectTension = false;
		[SerializeField, Tooltip("ミッション達成時のテンション倍率")] private float _missionTensionMultiplier = 2f;

        [Header("状態設定")]
        [SerializeField] private List<TensionStateConfig> _stateConfigs = new List<TensionStateConfig>
        {
            new TensionStateConfig
            {
                state = TensionState.Critical,
                displayText = "萎えぽよ...",
                color1 = new Color(0.5f, 0.5f, 0.5f),
                color2 = new Color(0.3f, 0.3f, 0.3f),
                threshold = 0f
            },
            new TensionStateConfig
            {
                state = TensionState.Low,
                displayText = "テンション低め",
                color1 = new Color(0.8f, 0.6f, 0.4f),
                color2 = new Color(0.6f, 0.4f, 0.2f),
                threshold = 0.2f
            },
            new TensionStateConfig
            {
                state = TensionState.Normal,
                displayText = "通常",
                color1 = new Color(1f, 0.8f, 0.2f),
                color2 = new Color(1f, 0.6f, 0f),
                threshold = 0.4f
            },
            new TensionStateConfig
            {
                state = TensionState.High,
                displayText = "いい感じ！",
                color1 = new Color(1f, 0.9f, 0.3f),
                color2 = new Color(1f, 0.7f, 0.1f),
                threshold = 0.7f
            },
            new TensionStateConfig
            {
                state = TensionState.Hyper,
                displayText = "ハイテンション！！",
                color1 = new Color(1f, 1f, 0.4f),
                color2 = new Color(1f, 0.8f, 0.2f),
                threshold = 0.9f
            }
        };

        [Header("コメント連携")]
        [SerializeField] private float _tensionPerComment = 2f;
        [SerializeField] private float _tensionPerGoodComment = 5f;

    [Header("アニメーション設定")]
    [SerializeField, Tooltip("状態テキストの拡大率（1 = 元のサイズ）")]
    private float _textScaleAmount = 1.3f;
    [SerializeField, Tooltip("状態テキストの拡大アニメの時間（秒）")]
    private float _textScaleUpDuration = 0.2f;
    [SerializeField, Tooltip("状態テキストの縮小アニメの時間（秒）")]
    private float _textScaleDownDuration = 0.2f;
    [SerializeField, Tooltip("状態テキストのフェードアウト時間（秒）")]
    private float _textFadeOutDuration = 0.1f;
    [SerializeField, Tooltip("状態テキストのフェードイン時間（秒）")]
    private float _textFadeInDuration = 0.1f;
    [SerializeField, Tooltip("良コメント時の跳ね（パンチ）強さ")]
    private float _textPunchStrength = 0.3f;
    [SerializeField, Tooltip("良コメント時の跳ね（パンチ）継続時間（秒）")]
    private float _textPunchDuration = 0.5f;
    [SerializeField, Tooltip("枠の光る（フェード）時間（秒）")]
    private float _frameFlashDuration = 0.1f;
    [SerializeField, Tooltip("枠の光るループ回数（Yoyo の往復回数）")]
    private int _frameFlashLoops = 2;

        private float _currentTension;
        private TensionState _currentState;
        private Tweener _fillTweener;
        private Sequence _textAnimSequence;
        private bool _hasInitialized;

        public float CurrentTension => _currentTension;
        public float TensionRatio => _currentTension / _maxTension;
        public TensionState CurrentState => _currentState;

        public event System.Action<TensionState> OnStateChanged;
        public event System.Action<float> OnTensionChanged;

        private void Start()
        {
            _currentTension = _initialTension;

            // 影の設定
            if (_shadowGauge != null)
            {
                _shadowGauge.IsShadowMode = true;
                _shadowGauge.color = new Color(0, 0, 0, 0.3f);
            }

            UpdateGauge();
        }

        private void Update()
        {
			// 自然減少（状態に応じて加速、目安40秒でゼロへ）
			float decay = ComputeDecayPerSecond();
			_currentTension -= decay * Time.deltaTime;
            _currentTension = Mathf.Clamp(_currentTension, 0f, _maxTension);

            UpdateGauge();
        }

		private float ComputeDecayPerSecond()
		{
			// 目安：最大値から _targetSecondsToZeroFromMax 秒でゼロへ（線形）
			float baseDecay = (_targetSecondsToZeroFromMax > 0f) ? (_maxTension / _targetSecondsToZeroFromMax) : _decreasePerSecond;
			// 既存の設定値も下限として尊重（インスペクタで微調整可）
			float decay = Mathf.Max(baseDecay, _decreasePerSecond);

			// 状態に応じた加速（ハイテンションは長く続かない）
			switch (_currentState)
			{
				case TensionState.High:
					decay *= Mathf.Max(1f, _highStateDecayMultiplier);
					break;
				case TensionState.Hyper:
					decay *= Mathf.Max(_highStateDecayMultiplier, _hyperStateDecayMultiplier);
					break;
			}

			return decay;
		}

        /// <summary>
        /// コメントを受信したときに呼ぶ
        /// </summary>
        public void OnCommentReceived(string comment)
        {
			if (!_textCommentsAffectTension)
			{
				// テキストコメントではテンションを増やさない
				return;
			}

            // コメントの内容に応じてテンション増加
            float tensionGain = _tensionPerComment;

            // ポジティブなコメントの場合、より増加
            if (IsPositiveComment(comment))
            {
                tensionGain = _tensionPerGoodComment;
                PlayGoodCommentEffect();
            }

            AddTension(tensionGain);
        }

		/// <summary>
		/// ミッション達成時のテンション増加（外部呼び出し用）
		/// </summary>
		public void OnMissionCompleted(float tensionReward)
		{
			float amount = Mathf.Max(0f, tensionReward) * Mathf.Max(0f, _missionTensionMultiplier);
			AddTension(amount);
		}

        /// <summary>
        /// テンションを追加
        /// </summary>
        public void AddTension(float amount)
        {
            _currentTension = Mathf.Clamp(_currentTension + amount, 0f, _maxTension);
            UpdateGauge();
            OnTensionChanged?.Invoke(_currentTension);
        }

        /// <summary>
        /// ゲージの表示を更新
        /// </summary>
        private void UpdateGauge()
        {
            float ratio = TensionRatio;

            // メインゲージの fillAmount を更新
            if (_mainGauge != null)
            {
                _fillTweener?.Kill();
                _fillTweener = DOTween.To(
                    () => _mainGauge.FillAmount,
                    x => _mainGauge.FillAmount = x,
                    ratio,
                    0.3f
                ).SetEase(Ease.OutQuad);
            }

            // 状態を判定
            TensionState newState = GetStateForRatio(ratio);

            if (newState != _currentState || !_hasInitialized)
            {
                _currentState = newState;
                OnStateChange(newState);
                _hasInitialized = true;
            }

            // 数値テキスト
            if (_valueText != null)
            {
                _valueText.text = Mathf.RoundToInt(_currentTension).ToString();
            }
        }

        /// <summary>
        /// テンション比率から状態を取得
        /// </summary>
        private TensionState GetStateForRatio(float ratio)
        {
            if (ratio >= 0.8f) return TensionState.Hyper;
            if (ratio >= 0.6f) return TensionState.High;
            if (ratio >= 0.4f) return TensionState.Normal;
            if (ratio >= 0.2f) return TensionState.Low;
            return TensionState.Critical;
        }

        /// <summary>
        /// 状態が変わったときの処理
        /// </summary>
        private void OnStateChange(TensionState newState)
        {
            Debug.Log($"[Tension] 状態変更: {newState}");

            TensionStateConfig config = _stateConfigs.Find(c => c.state == newState);
            if (config == null) return;

            // ゲージの色を変更
            if (_mainGauge != null)
            {
                _mainGauge.SetColors(config.color1, config.color2);
            }

            // テキストを変更
            if (_stateText != null)
            {
                _stateText.text = config.displayText;
                PlayTextChangeAnimation();
            }

            // 枠の色を変更
            if (_frame != null)
            {
                _frame.DOColor(config.color1, 0.3f);
            }

            OnStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// テキスト変更時のアニメーション
        /// </summary>
        private void PlayTextChangeAnimation()
        {
            if (_stateText == null) return;

            _textAnimSequence?.Kill();
            _textAnimSequence = DOTween.Sequence()
                .Append(_stateText.transform.DOScale(_textScaleAmount, _textScaleUpDuration).SetEase(Ease.OutBack))
                .Append(_stateText.transform.DOScale(1f, _textScaleDownDuration).SetEase(Ease.OutQuad))
                .Join(_stateText.DOFade(0f, _textFadeOutDuration))
                .Append(_stateText.DOFade(1f, _textFadeInDuration));
        }

        /// <summary>
        /// 良いコメントの時のエフェクト
        /// </summary>
        private void PlayGoodCommentEffect()
        {
            // テキストが跳ねる
            if (_stateText != null)
            {
                _stateText.transform.DOPunchScale(Vector3.one * _textPunchStrength, _textPunchDuration);
            }

            // 枠が光る
            if (_frame != null)
            {
                _frame.DOFade(1f, _frameFlashDuration).SetLoops(_frameFlashLoops, LoopType.Yoyo);
            }
        }

        /// <summary>
        /// ポジティブなコメントかチェック
        /// </summary>
        private bool IsPositiveComment(string comment)
        {
            if (string.IsNullOrEmpty(comment)) return false;

            string[] positiveWords = { "すごい", "いいね", "最高", "88888", "草", "w", "かわいい", "好き" };

            foreach (var word in positiveWords)
            {
                if (comment.Contains(word))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDestroy()
        {
            _fillTweener?.Kill();
            _textAnimSequence?.Kill();
        }
    }
}
