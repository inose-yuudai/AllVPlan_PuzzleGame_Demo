using UnityEngine;
using TMPro;
using DG.Tweening;

namespace EmoteOrchestra.UI
{
    public class ViewerCountManager : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private TextMeshProUGUI _viewerCountText;
        [SerializeField] private TextMeshProUGUI _viewerChangeText;

        [Header("視聴者数の設定")]
        [SerializeField] private int _initialViewerCount = 1234;
        [SerializeField] private int _maxViewerCount = 99999;
        [SerializeField] private int _minViewerCount = 0;

        [Header("視聴者減少の設定")]
        [SerializeField] private bool _enableViewerDecrease = true;
        [SerializeField] private float _decreaseInterval = 5f;
        [SerializeField] private int _decreaseAmount = 5;
        [SerializeField] private float _noActionTimeout = 10f;
        [SerializeField] private int _noActionPenalty = 20;

        [Header("アニメーション設定")]
        [SerializeField] private float _countAnimationDuration = 0.5f;
        [SerializeField] private bool _enableShakeOnDecrease = true;

        [Header("表示設定")]
        [SerializeField] private int _commaFontSize = 20;

		[Header("テンション連動")]
		[SerializeField] private bool _enableTensionScaledDecrease = true;
		[SerializeField] private PopTensionGauge _popTensionGauge;
		[SerializeField] private TensionGauge _tensionGauge;
		[SerializeField, Tooltip("萎えぽよ(Critical)時の減少倍率")] private float _criticalDecreaseMultiplier = 3.0f;
		[SerializeField, Tooltip("Low時の減少倍率")] private float _lowDecreaseMultiplier = 1.2f;
		[SerializeField, Tooltip("Normal時の減少倍率")] private float _normalDecreaseMultiplier = 1.0f;
		[SerializeField, Tooltip("High時の減少倍率")] private float _highDecreaseMultiplier = 0.85f;
		[SerializeField, Tooltip("Hyper時の減少倍率")] private float _hyperDecreaseMultiplier = 0.7f;

        private static int _currentViewerCount;
        private int _displayViewerCount;
        private float _decreaseTimer;
        private float _lastActionTime;

        private Tweener _countTweener;
        private Sequence _changeTextSequence;
        private Tweener _shakeTweener;

        // ★元の位置を保存
        private Vector3 _originalViewerCountPosition;
        private Vector3 _originalViewerChangePosition; // ← 追加

        public int CurrentViewerCount => _currentViewerCount;

        private void Start()
        {
            _currentViewerCount = _initialViewerCount;
            _displayViewerCount = _initialViewerCount;
            _lastActionTime = Time.time;

            if (_viewerCountText != null)
            {
                _originalViewerCountPosition = _viewerCountText.transform.localPosition;
            }

            // ★ここで最初は非表示にして、位置も記録しておく
            if (_viewerChangeText != null)
            {
                _originalViewerChangePosition = _viewerChangeText.transform.localPosition;
                _viewerChangeText.gameObject.SetActive(false);
            }

            UpdateViewerCountImmediate(_currentViewerCount);
        }

        private string FormatWithSizedCommas(string numericText)
        {
            if (string.IsNullOrEmpty(numericText)) return numericText;
            string sizedComma = $"<size={_commaFontSize}>,</size>";
            return numericText.Replace(",", sizedComma);
        }

        private string FormatNumberForTMP(int value)
        {
            return FormatWithSizedCommas(value.ToString("N0"));
        }

        private void Update()
        {
            if (_enableViewerDecrease)
            {
                UpdateViewerDecrease();
            }
        }

        private void UpdateViewerDecrease()
        {
            _decreaseTimer += Time.deltaTime;

            if (_decreaseTimer >= _decreaseInterval)
            {
                _decreaseTimer = 0f;
				int baseDecrease = _decreaseAmount;
				int scaled = ApplyTensionDecreaseScaling(baseDecrease);
				DecreaseViewers(scaled);

                float timeSinceLastAction = Time.time - _lastActionTime;
                if (timeSinceLastAction >= _noActionTimeout)
                {
					int penalty = ApplyTensionDecreaseScaling(_noActionPenalty);
					DecreaseViewers(penalty);
                }
            }
        }

		private int ApplyTensionDecreaseScaling(int amount)
		{
			if (!_enableTensionScaledDecrease || amount <= 0)
				return amount;

			float mul = GetDecreaseMultiplierByTension();
			int scaled = Mathf.RoundToInt(amount * Mathf.Max(0f, mul));
			// 減少が0にならないように下限1（amountが正のとき）
			return Mathf.Max(1, scaled);
		}

		private float GetDecreaseMultiplierByTension()
		{
			float ratio = GetCurrentTensionRatio();
			// PopTensionGaugeの閾値に合わせる
			if (ratio >= 0.8f) return _hyperDecreaseMultiplier;
			if (ratio >= 0.6f) return _highDecreaseMultiplier;
			if (ratio >= 0.4f) return _normalDecreaseMultiplier;
			if (ratio >= 0.2f) return _lowDecreaseMultiplier;
			return _criticalDecreaseMultiplier; // 萎えぽよ（Critical）
		}

		private float GetCurrentTensionRatio()
		{
			// Pop優先、無ければ旧TensionGauge
			if (_popTensionGauge != null)
			{
				return Mathf.Clamp01(_popTensionGauge.TensionRatio);
			}
			if (_tensionGauge != null)
			{
				return Mathf.Clamp01(_tensionGauge.TensionRatio);
			}
			return 0f; // 情報が無ければ最低テンションとして扱う
		}

        public void OnPlayerAction()
        {
            _lastActionTime = Time.time;
        }

        private void UpdateViewerCountImmediate(int count)
        {
            _currentViewerCount = Mathf.Clamp(count, _minViewerCount, _maxViewerCount);
            _displayViewerCount = _currentViewerCount;

            if (_viewerCountText != null)
            {
                _viewerCountText.text = FormatNumberForTMP(_displayViewerCount);
            }
        }

        public void AddViewers(int amount)
        {
            int newCount = _currentViewerCount + amount;
            UpdateViewerCountAnimated(newCount, amount);
            OnPlayerAction();
        }

        public void DecreaseViewers(int amount)
        {
            int newCount = _currentViewerCount - amount;
            UpdateViewerCountAnimated(newCount, -amount);
        }

        private void UpdateViewerCountAnimated(int newCount, int change)
        {
            _currentViewerCount = Mathf.Clamp(newCount, _minViewerCount, _maxViewerCount);

            _countTweener?.Kill();
            _countTweener = DOTween.To(
                () => _displayViewerCount,
                x =>
                {
                    _displayViewerCount = x;
                    if (_viewerCountText != null)
                    {
                        _viewerCountText.text = FormatNumberForTMP(_displayViewerCount);
                    }
                },
                _currentViewerCount,
                _countAnimationDuration
            ).SetEase(Ease.OutQuad);

            ShowViewerChange(change);

            if (change < 0 && _enableShakeOnDecrease && _viewerCountText != null)
            {
                PlayShakeEffect();
            }
        }

        private void PlayShakeEffect()
        {
            if (_viewerCountText == null)
                return;

            _shakeTweener?.Kill();

            _viewerCountText.transform.localPosition = _originalViewerCountPosition;

            _shakeTweener = _viewerCountText.transform.DOShakePosition(
                duration: 0.3f,
                strength: 10f,
                vibrato: 20,
                randomness: 90,
                snapping: false,
                fadeOut: true
            ).OnComplete(() =>
            {
                if (_viewerCountText != null)
                {
                    _viewerCountText.transform.localPosition = _originalViewerCountPosition;
                }
            });
        }

            private void ShowViewerChange(int change)
        {
            if (_viewerChangeText == null)
                return;

            _changeTextSequence?.Kill();

            string sign = change > 0 ? "+" : (change < 0 ? "-" : "");
            int abs = Mathf.Abs(change);
            string body = FormatWithSizedCommas(abs.ToString("N0"));
            _viewerChangeText.text = sign + body;

            // ★位置は触らないで、その場から出す
            _viewerChangeText.transform.localScale = Vector3.one * 0.5f;
            _viewerChangeText.alpha = 1f;
            _viewerChangeText.gameObject.SetActive(true);

            // 今ある位置を起点にする
            Vector3 startPos = _viewerChangeText.transform.localPosition;
            Vector3 endPos = startPos + Vector3.up * 50f;

            _changeTextSequence = DOTween.Sequence()
                .Append(_viewerChangeText.transform.DOScale(1.5f, 0.2f).SetEase(Ease.OutBack))
                .Join(_viewerChangeText.transform.DOLocalMove(endPos, 1.5f).SetEase(Ease.OutQuad))
                .Join(_viewerChangeText.DOFade(0f, 1.5f).SetEase(Ease.InQuad))
                .OnComplete(() =>
                {
                    _viewerChangeText.gameObject.SetActive(false);
                    // 終了後も元の位置に戻したいならこれだけにしておく
                    _viewerChangeText.transform.localPosition = startPos;
                    _viewerChangeText.alpha = 1f;
                });
        }


        public void ResetViewerCount()
        {
            UpdateViewerCountImmediate(_initialViewerCount);

            if (_viewerCountText != null)
            {
                _viewerCountText.transform.localPosition = _originalViewerCountPosition;
            }

            if (_viewerChangeText != null)
            {
                _viewerChangeText.gameObject.SetActive(false);
                _viewerChangeText.transform.localPosition = _originalViewerChangePosition;
                _viewerChangeText.alpha = 1f;
            }
        }

        private void OnDestroy()
        {
            _countTweener?.Kill();
            _changeTextSequence?.Kill();
            _shakeTweener?.Kill();
        }

        [ContextMenu("Reset Original Position")]
        public void ResetOriginalPosition()
        {
            if (_viewerCountText != null)
            {
                _originalViewerCountPosition = _viewerCountText.transform.localPosition;
            }

            if (_viewerChangeText != null)
            {
                _originalViewerChangePosition = _viewerChangeText.transform.localPosition;
            }
        }
    }
}
