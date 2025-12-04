using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using EmoteOrchestra.Data;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// セトリの個別曲アイテム
    /// </summary>
    public class SongItemUI : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private GameObject _normalBackground;
        [SerializeField] private GameObject _activeBackground;
        [SerializeField] private GameObject _playingIndicator;
        [SerializeField] private TextMeshProUGUI _numberText;
        [SerializeField] private TextMeshProUGUI _titleText;

        private SongData _songData;
        private int _songIndex;
        private bool _isPlaying;
        private Sequence _blinkSequence;

        /// <summary>
        /// 曲情報を設定
        /// </summary>
        public void Initialize(SongData songData, int index)
        {
            _songData = songData;
            _songIndex = index;

            // デバッグログ
            if (songData == null)
            {
                Debug.LogError("SongItemUI: songData が null です");
                return;
            }

            Debug.Log($"SongItemUI: Initialize - {songData.songTitle}");

            // 番号と曲名を設定
            if (_numberText != null)
            {
                _numberText.text = $"{index + 1}.";
               // Debug.Log($"SongItemUI: 番号設定 - {_numberText.text}");
            }
            else
            {
                Debug.LogError("SongItemUI: _numberText が null です");
            }

            if (_titleText != null)
            {
                _titleText.text = songData.songTitle;
               // Debug.Log($"SongItemUI: 曲名設定 - {_titleText.text}");
            }
            else
            {
                Debug.LogError("SongItemUI: _titleText が null です");
            }

            // 初期状態は非アクティブ
            SetPlaying(false);
        }

        /// <summary>
        /// 再生中状態を設定
        /// </summary>
        public void SetPlaying(bool isPlaying)
        {
            _isPlaying = isPlaying;

            if (_normalBackground != null)
                _normalBackground.SetActive(!isPlaying);

            if (_activeBackground != null)
                _activeBackground.SetActive(isPlaying);

            if (_playingIndicator != null)
                _playingIndicator.SetActive(isPlaying);

            // 再生中は文字色を明るく
            if (_titleText != null)
            {
                _titleText.color = isPlaying ? Color.white : new Color(0.8f, 0.8f, 0.8f);
            }

            if (_numberText != null)
            {
                _numberText.color = isPlaying ? Color.white : new Color(0.6f, 0.6f, 0.6f);
            }

            // 点滅アニメーション
            _blinkSequence?.Kill();

            if (isPlaying && _playingIndicator != null)
            {
                Image indicator = _playingIndicator.GetComponent<Image>();
                if (indicator != null)
                {
                    _blinkSequence = DOTween.Sequence()
                        .Append(indicator.DOFade(0.3f, 0.5f))
                        .Append(indicator.DOFade(1.0f, 0.5f))
                        .SetLoops(-1, LoopType.Restart);
                }
            }
        }

        /// <summary>
        /// この曲が再生中かどうか
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// 曲データ
        /// </summary>
        public SongData SongData => _songData;

        private void OnDestroy()
        {
            _blinkSequence?.Kill();
        }
    }
}