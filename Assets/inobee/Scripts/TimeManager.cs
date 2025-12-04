using UnityEngine;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;
using EmoteOrchestra.Core;

namespace EmoteOrchestra.Core
{
    /// <summary>
    /// 時間管理（スローモーション等）
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        [Header("Time Scale")]
        private float _normalTimeScale = 1.0f;
        private float _slowTimeScale = 0.7f;

        [Header("Countdown Settings")]
        [SerializeField] private float _initialSeconds = 60f;
        [SerializeField] private bool _autoStart = true;
        [SerializeField] private bool _useUnscaledTime = true; // スローモーションに影響されない
        [SerializeField] private float _warningThreshold = 10f; // 残りこの秒数以下で警告演出

        [Header("UI (TextMeshPro)")]
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _warningColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private float _warningScale = 1.15f;
        [SerializeField] private float _pulseSpeed = 6f; // 高いほど点滅が速い

        [Header("Events")]
        [SerializeField] private UnityEvent _onTimeUp = new UnityEvent();

		[Header("Game Over Panel (Optional)")]
		[SerializeField] private bool _showPanelOnTimeUp = false;
		[SerializeField] private RectTransform _gameOverPanel;
		[SerializeField] private float _panelSlideDuration = 1.0f;
		[SerializeField] private Ease _panelSlideEase = Ease.OutBounce;

        private float _remainingSeconds;
        private bool _isRunning;
        private bool _isWarningPhase;
        private Vector3 _baseScale = Vector3.one;
        private Color _baseColor = Color.white;

        private void Awake()
        {
            ServiceLocator.Register<TimeManager>(this);
            if (_timerText != null)
            {
                _baseScale = _timerText.rectTransform.localScale;
                _baseColor = _timerText.color;
            }
        }

        private void Start()
        {
            ResetCountdown();
            if (_autoStart)
            {
                StartCountdown();
            }
        }

        private void Update()
        {
            if (!_isRunning)
                return;

            float dt = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (dt <= 0f)
                return;

            _remainingSeconds -= dt;
            if (_remainingSeconds <= 0f)
            {
                _remainingSeconds = 0f;
                _isRunning = false;
                UpdateTimerVisuals(0f); // 0で最終表示
				// オプション：GameOverパネルをスライドイン表示
				if (_showPanelOnTimeUp)
				{
					ShowGameOverPanelSlideIn();
				}
				if (_onTimeUp != null)
                {
                    _onTimeUp.Invoke();
                }
                return;
            }

            UpdateTimerVisuals(dt);
        }

        public void SetSlowMotion(bool enabled)
        {
            Time.timeScale = enabled ? _slowTimeScale : _normalTimeScale;
        }

        public void SetTimeScale(float scale)
        {
            Time.timeScale = Mathf.Clamp(scale, 0f, 2f);
        }

        public void ResetTimeScale()
        {
            Time.timeScale = _normalTimeScale;
        }

        // ===== Countdown API =====

        public void StartCountdown()
        {
            _isRunning = true;
        }

        public void StartCountdown(float seconds)
        {
            _remainingSeconds = Mathf.Max(0f, seconds);
            _isRunning = true;
            UpdateTimerVisuals(0f);
        }

        public void StopCountdown()
        {
            _isRunning = false;
        }

        public void ResetCountdown()
        {
            _remainingSeconds = Mathf.Max(0f, _initialSeconds);
            _isWarningPhase = false;
            UpdateTimerVisuals(0f);
        }

        public void AddTime(float seconds)
        {
            _remainingSeconds = Mathf.Max(0f, _remainingSeconds + seconds);
            UpdateTimerVisuals(0f);
        }

        public float GetRemainingSeconds()
        {
            return _remainingSeconds;
        }

        public void SetRemainingSeconds(float seconds)
        {
            _remainingSeconds = Mathf.Max(0f, seconds);
            UpdateTimerVisuals(0f);
        }

        public void SetOnTimeUpListener(UnityAction action)
        {
            if (_onTimeUp != null && action != null)
            {
                _onTimeUp.AddListener(action);
            }
        }

		/// <summary>
		/// GameOverパネルを下からスライドさせて表示（GameManagerを触らずに同等演出）
		/// </summary>
		public void ShowGameOverPanelSlideIn()
		{
			if (_gameOverPanel == null) return;
			_gameOverPanel.gameObject.SetActive(true);
			_gameOverPanel.anchoredPosition = new Vector2(0, -Screen.height);
			_gameOverPanel.DOAnchorPos(Vector2.zero, _panelSlideDuration)
				.SetEase(_panelSlideEase);
		}

        // ===== Visuals =====

        private void UpdateTimerVisuals(float dt)
        {
            if (_timerText != null)
            {
                _timerText.text = FormatTime(_remainingSeconds);

                bool nowWarning = _remainingSeconds <= _warningThreshold;
                if (nowWarning)
                {
                    float timeForAnim = _useUnscaledTime ? Time.unscaledTime : Time.time;
                    float t = (Mathf.Sin(timeForAnim * _pulseSpeed) + 1f) * 0.5f; // 0..1
                    float scale = Mathf.Lerp(1f, _warningScale, t);
                    _timerText.rectTransform.localScale = _baseScale * scale;
                    _timerText.color = Color.Lerp(_normalColor, _warningColor, t);
                }
                else
                {
                    _timerText.rectTransform.localScale = _baseScale;
                    _timerText.color = _normalColor;
                }

                _isWarningPhase = nowWarning;
            }
        }

        private string FormatTime(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            int totalSeconds = Mathf.FloorToInt(seconds);
            int minutes = totalSeconds / 60;
            int secs = totalSeconds % 60;
            return string.Format("{0:00}:{1:00}", minutes, secs);
        }
    }
}