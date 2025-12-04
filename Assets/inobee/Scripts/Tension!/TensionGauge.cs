using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// テンションゲージ管理
    /// </summary>
    public class TensionGauge : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private Image _gaugeFillImage;
        [SerializeField] private TextMeshProUGUI _tensionText;
        [SerializeField] private Image _gaugeBackgroundImage;

        [Header("ゲージ設定")]
        [SerializeField] private float _maxTension = 100f;
        [SerializeField] private float _initialTension = 50f;
        
        [Header("増減設定")]
        [SerializeField] private float _tensionPerMatch = 1f; // 1個消すごとに増加
        [SerializeField] private float _tensionPerCombo = 5f; // 1連鎖ごとに増加
        [SerializeField] private float _decreasePerSecond = 0.5f; // 毎秒減少量
        [SerializeField] private float _noActionPenalty = 5f; // 長時間操作なしペナルティ
        [SerializeField] private float _noActionTimeout = 10f; // ペナルティ発動までの時間

        [Header("ゲームオーバー設定")]
        [SerializeField] private float _gameOverThreshold = 0f; // この値以下でゲームオーバー
        [SerializeField] private float _gameOverGracePeriod = 3f; // ゲームオーバー猶予時間

        [Header("色設定")]
        [SerializeField] private Color _highTensionColor = Color.green;
        [SerializeField] private Color _mediumTensionColor = Color.yellow;
        [SerializeField] private Color _lowTensionColor = Color.red;
        [SerializeField] private Color _criticalTensionColor = new Color(0.5f, 0f, 0f); // 濃い赤

        [Header("演出設定")]
        [SerializeField] private bool _enableShakeOnLow = true;
        [SerializeField] private GameObject _lowTensionEffectPrefab; // 低テンション時のエフェクト

        private float _currentTension;
        private float _lastActionTime;
        private float _gameOverTimer;
        private bool _isGameOver;
        
        private Tweener _gaugeTweener;
        private Sequence _shakeSequence;

        public float CurrentTension => _currentTension;
        public float TensionRatio => _currentTension / _maxTension;
        public bool IsLowTension => _currentTension < _maxTension * 0.3f;
        public bool IsCriticalTension => _currentTension < _maxTension * 0.1f;

        // イベント（将来的にVtuberに影響を与える用）
        public event System.Action<float> OnTensionChanged;
        public event System.Action OnLowTension;
        public event System.Action OnCriticalTension;
        public event System.Action OnGameOver;

        private void Start()
        {
            _currentTension = _initialTension;
            _lastActionTime = Time.time;
            _isGameOver = false;
            _gameOverTimer = 0f;

            UpdateGaugeVisual();
        }

        private void Update()
        {
            if (_isGameOver)
                return;

            // 自然減少
            DecreaseTension(_decreasePerSecond * Time.deltaTime);

            // 長時間操作なしペナルティ
            if (Time.time - _lastActionTime > _noActionTimeout)
            {
                DecreaseTension(_noActionPenalty * Time.deltaTime);
            }

            // ゲームオーバー判定
            if (_currentTension <= _gameOverThreshold)
            {
                _gameOverTimer += Time.deltaTime;
                if (_gameOverTimer >= _gameOverGracePeriod)
                {
                    TriggerGameOver();
                }
            }
            else
            {
                _gameOverTimer = 0f;
            }
        }

        /// <summary>
        /// パズルマッチ時のテンション増加
        /// </summary>
        public void OnPuzzleMatched(int matchCount)
        {
            float gain = matchCount * _tensionPerMatch;
            AddTension(gain);
            _lastActionTime = Time.time;

            Debug.Log($"[Tension] パズルマッチ: +{gain} ({matchCount}個消し)");
        }

        /// <summary>
        /// コンボ時のテンション増加
        /// </summary>
        public void OnComboAchieved(int comboCount)
        {
            float gain = comboCount * _tensionPerCombo;
            AddTension(gain);
            _lastActionTime = Time.time;

            Debug.Log($"[Tension] コンボ: +{gain} ({comboCount}連鎖)");
        }

        /// <summary>
        /// ミッション達成時のテンション増加
        /// </summary>
        public void OnMissionCompleted(float tensionReward)
        {
            AddTension(tensionReward);
            _lastActionTime = Time.time;

            Debug.Log($"[Tension] ミッション達成: +{tensionReward}");
        }

        /// <summary>
        /// プレイヤーアクション（スワップなど）
        /// </summary>
        public void OnPlayerAction()
        {
            _lastActionTime = Time.time;
        }

        /// <summary>
        /// テンションを増加
        /// </summary>
        private void AddTension(float amount)
        {
            float oldTension = _currentTension;
            _currentTension = Mathf.Clamp(_currentTension + amount, 0f, _maxTension);

            UpdateGaugeVisual();
            OnTensionChanged?.Invoke(_currentTension);

            // ゲームオーバータイマーをリセット
            if (_currentTension > _gameOverThreshold)
            {
                _gameOverTimer = 0f;
            }
        }

        /// <summary>
        /// テンションを減少
        /// </summary>
        private void DecreaseTension(float amount)
        {
            float oldTension = _currentTension;
            _currentTension = Mathf.Clamp(_currentTension - amount, 0f, _maxTension);

            UpdateGaugeVisual();
            OnTensionChanged?.Invoke(_currentTension);

            // 低テンション警告
            if (!IsLowTension && _currentTension < _maxTension * 0.3f)
            {
                OnLowTensionEnter();
            }

            // 危機的状態警告
            if (!IsCriticalTension && _currentTension < _maxTension * 0.1f)
            {
                OnCriticalTensionEnter();
            }
        }

        /// <summary>
        /// ゲージの見た目を更新
        /// </summary>
        private void UpdateGaugeVisual()
        {
            // ゲージの量
            if (_gaugeFillImage != null)
            {
                _gaugeTweener?.Kill();
                _gaugeTweener = DOTween.To(
                    () => _gaugeFillImage.fillAmount,
                    x => _gaugeFillImage.fillAmount = x,
                    TensionRatio,
                    0.3f
                ).SetEase(Ease.OutQuad);
            }

            // 色の変更
            Color targetColor = GetTensionColor();
            if (_gaugeFillImage != null)
            {
                _gaugeFillImage.color = targetColor;
            }

            // テキスト更新
            if (_tensionText != null)
            {
                _tensionText.text = $"{Mathf.RoundToInt(_currentTension)} / {Mathf.RoundToInt(_maxTension)}";
                _tensionText.color = targetColor;
            }

            // 低テンション時のシェイク
            if (IsLowTension && _enableShakeOnLow)
            {
                PlayShakeEffect();
            }
        }

        /// <summary>
        /// テンションに応じた色を取得
        /// </summary>
        private Color GetTensionColor()
        {
            float ratio = TensionRatio;

            if (ratio > 0.6f)
            {
                return _highTensionColor;
            }
            else if (ratio > 0.3f)
            {
                return Color.Lerp(_mediumTensionColor, _highTensionColor, (ratio - 0.3f) / 0.3f);
            }
            else if (ratio > 0.1f)
            {
                return Color.Lerp(_lowTensionColor, _mediumTensionColor, (ratio - 0.1f) / 0.2f);
            }
            else
            {
                return _criticalTensionColor;
            }
        }

        /// <summary>
        /// シェイクエフェクト
        /// </summary>
        private void PlayShakeEffect()
        {
            if (_gaugeBackgroundImage == null)
                return;

            _shakeSequence?.Kill();
            _shakeSequence = DOTween.Sequence()
                .Append(_gaugeBackgroundImage.transform.DOShakePosition(0.2f, strength: 5f, vibrato: 10))
                .SetLoops(-1, LoopType.Restart);
        }

        /// <summary>
        /// 低テンション状態に入った
        /// </summary>
        private void OnLowTensionEnter()
        {
            Debug.Log("★[Tension] 低テンション警告！");
            OnLowTension?.Invoke();

            // エフェクト表示（将来実装）
            // TODO: 画面暗くなる、コメント流れが遅くなる
        }

        /// <summary>
        /// 危機的状態に入った
        /// </summary>
        private void OnCriticalTensionEnter()
        {
            Debug.Log("★★★[Tension] 危機的状態！ゲームオーバー間近！");
            OnCriticalTension?.Invoke();

            // 強い警告エフェクト（将来実装）
            // TODO: 画面が真っ暗に、コメントほぼ止まる
        }

        /// <summary>
        /// ゲームオーバー
        /// </summary>
        private void TriggerGameOver()
        {
            if (_isGameOver)
                return;

            _isGameOver = true;

            Debug.Log("★★★★★★★★★★★★★★★★★★★★★★");
            Debug.Log("★★★ ゲームオーバー！ ★★★");
            Debug.Log("★★★ テンションが0になりました ★★★");
            Debug.Log("★★★★★★★★★★★★★★★★★★★★★★");

            OnGameOver?.Invoke();

            // ゲームオーバー処理（将来実装）
            // TODO: ゲームを停止、リザルト画面表示など
        }

        /// <summary>
        /// テンションをリセット
        /// </summary>
        public void ResetTension()
        {
            _currentTension = _initialTension;
            _lastActionTime = Time.time;
            _gameOverTimer = 0f;
            _isGameOver = false;

            UpdateGaugeVisual();

            _shakeSequence?.Kill();

            Debug.Log("[Tension] テンションをリセットしました");
        }

        private void OnDestroy()
        {
            _gaugeTweener?.Kill();
            _shakeSequence?.Kill();
        }
    }
}