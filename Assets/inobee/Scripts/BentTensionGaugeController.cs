using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// 曲がったテンションゲージのコントローラー
    /// </summary>
    public class BentTensionGaugeController : MonoBehaviour
    {
        [Header("Mesh参照")]
        [SerializeField] private BentGaugeMesh _gaugeMesh;
        [SerializeField] private Material _gaugeMaterial;

        [Header("UI要素")]
        [SerializeField] private TextMeshProUGUI _valueText;
        [SerializeField] private Image _iconImage;

        [Header("エフェクト")]
        [SerializeField] private ParticleSystem _particles;
        [SerializeField] private Image _glowEffect;

        [Header("設定")]
        [SerializeField] private float _maxTension = 100f;
        [SerializeField] private float _initialTension = 50f;
        [SerializeField] private float _decreasePerSecond = 0.5f;

        [Header("色設定")]
        [SerializeField] private Color _highColor = new Color(0f, 1f, 1f);
        [SerializeField] private Color _mediumColor = new Color(0f, 0.5f, 1f);
        [SerializeField] private Color _lowColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color _criticalColor = new Color(1f, 0f, 0f);

        private float _currentTension;
        private Tweener _fillTweener;

        // Shader Property IDs
        private static readonly int k_GlowColor = Shader.PropertyToID("_GlowColor");
        private static readonly int k_GlowIntensity = Shader.PropertyToID("_GlowIntensity");
        private static readonly int k_GradientStart = Shader.PropertyToID("_GradientStart");
        private static readonly int k_GradientEnd = Shader.PropertyToID("_GradientEnd");

        public float CurrentTension => _currentTension;
        public float TensionRatio => _currentTension / _maxTension;

        private void Start()
        {
            _currentTension = _initialTension;
            
            // Materialのインスタンスを作成（共有を避ける）
            if (_gaugeMesh != null && _gaugeMaterial != null)
            {
                _gaugeMesh.material = Instantiate(_gaugeMaterial);
            }

            UpdateGauge();
        }

        private void Update()
        {
            // 自然減少
            _currentTension -= _decreasePerSecond * Time.deltaTime;
            _currentTension = Mathf.Clamp(_currentTension, 0f, _maxTension);

            UpdateGauge();
        }

        public void AddTension(float amount)
        {
            _currentTension = Mathf.Clamp(_currentTension + amount, 0f, _maxTension);
            
            // パーティクルバースト
            if (_particles != null)
            {
                _particles.Emit((int)(amount * 2));
            }

            UpdateGauge();
        }

        private void UpdateGauge()
        {
            float ratio = TensionRatio;
            Color currentColor = GetColorForValue(ratio);

            // Meshの fillAmount を更新
            if (_gaugeMesh != null)
            {
                _fillTweener?.Kill();
                _fillTweener = DOTween.To(
                    () => _gaugeMesh.FillAmount,
                    x => _gaugeMesh.FillAmount = x,
                    ratio,
                    0.3f
                ).SetEase(Ease.OutQuad);
            }

            // Shaderのパラメータを更新
            if (_gaugeMesh != null && _gaugeMesh.material != null)
            {
                Material mat = _gaugeMesh.material;
                mat.SetColor(k_GlowColor, currentColor);
                
                // テンションに応じてグローの強さを変更
                float glowIntensity = 1f + ratio * 2f;
                mat.SetFloat(k_GlowIntensity, glowIntensity);

                // グラデーションの色も更新
                Color gradStart = Color.Lerp(_criticalColor, currentColor, ratio);
                Color gradEnd = currentColor;
                mat.SetColor(k_GradientStart, gradStart);
                mat.SetColor(k_GradientEnd, gradEnd);
            }

            // 外側のグローエフェクト
            if (_glowEffect != null)
            {
                Color glowColor = currentColor;
                glowColor.a = 0.5f * ratio;
                _glowEffect.DOColor(glowColor, 0.3f);
            }

            // 数値テキスト
            if (_valueText != null)
            {
                _valueText.text = Mathf.RoundToInt(_currentTension).ToString();
                _valueText.color = currentColor;
            }

            // アイコンの色
            if (_iconImage != null)
            {
                _iconImage.color = currentColor;
            }
        }

        private Color GetColorForValue(float value)
        {
            if (value > 0.6f)
            {
                return _highColor;
            }
            else if (value > 0.3f)
            {
                return Color.Lerp(_mediumColor, _highColor, (value - 0.3f) / 0.3f);
            }
            else if (value > 0.1f)
            {
                return Color.Lerp(_lowColor, _mediumColor, (value - 0.1f) / 0.2f);
            }
            else
            {
                return _criticalColor;
            }
        }

        private void OnDestroy()
        {
            _fillTweener?.Kill();
        }
    }
}