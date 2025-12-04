using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using EmoteOrchestra.Events;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// コンボ表示UI
    /// </summary>
    public class ComboDisplayUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private Slider _feverGauge;
        [SerializeField] private int _feverThreshold = 50;

        [Header("イベント")]
        [SerializeField] private GameEvent _onFeverStartEvent;

        private int _currentCombo;
        private int _feverGaugeValue;
        private bool _isFeverActive;
        private Sequence _comboAnimation;

        public void OnComboChanged(int combo)
        {
            _currentCombo = combo;
            UpdateComboDisplay();

            if (combo > 0)
            {
                _feverGaugeValue += combo;
                UpdateFeverGauge();

                if (_feverGaugeValue >= _feverThreshold && !_isFeverActive)
                {
                    StartFever();
                }
            }
            else
            {
                // コンボ途切れ
                _feverGaugeValue = 0;
                UpdateFeverGauge();
            }
        }

        private void UpdateComboDisplay()
        {
            if (_comboText == null) return;

            _comboAnimation?.Kill();

            if (_currentCombo > 0)
            {
                _comboText.text = $"COMBO x{_currentCombo}";
                _comboText.gameObject.SetActive(true);

                // パルスアニメーション
                _comboAnimation = DOTween.Sequence()
                    .Append(_comboText.transform.DOScale(1.3f, 0.1f))
                    .Append(_comboText.transform.DOScale(1.0f, 0.1f));
            }
            else
            {
                _comboText.gameObject.SetActive(false);
            }
        }

        private void UpdateFeverGauge()
        {
            if (_feverGauge == null) return;

            float normalizedValue = Mathf.Clamp01((float)_feverGaugeValue / _feverThreshold);
            _feverGauge.DOValue(normalizedValue, 0.3f);
        }

        private void StartFever()
        {
            _isFeverActive = true;
            _feverGaugeValue = 0;
            UpdateFeverGauge();

            _onFeverStartEvent?.Raise();

            // フィーバー時間後に終了
            DOVirtual.DelayedCall(15f, EndFever);

            Debug.Log("フィーバータイム開始！");
        }

        private void EndFever()
        {
            _isFeverActive = false;
            Debug.Log("フィーバータイム終了");
        }

        private void OnDestroy()
        {
            _comboAnimation?.Kill();
        }
    }
}