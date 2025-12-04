using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using EmoteOrchestra.Data;

namespace EmoteOrchestra.Puzzle
{
    [RequireComponent(typeof(RectTransform))]
    public class EmoteController : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Image _highlightImage;
        [SerializeField] private GameObject _matchEffectPrefab;
        [Header("Idle Sway Settings")]
        [SerializeField, Range(0f, 20f)] private float _idleSwayAngleDeg = 5f; // 左右に傾く角度
        [SerializeField, Range(0f, 0.3f)] private float _idleSwayScaleAmplitude = 0.05f; // 横：+、縦：- のゆらぎ
        [SerializeField, Range(0.1f, 3f)] private float _idleSwayDuration = 1.2f;
        [SerializeField] private Ease _idleSwayEase = Ease.InOutSine;
        
        private RectTransform _rectTransform;
        private EmoteData _data;
        private int _gridX;
        private int _gridY;
        private Tween _currentTween;
        private Color _originalColor;
        private Tween _idleTween;
        private bool _isChainActive = false;
        private Vector3 _imageLocalDefaultPos;
        private Vector3 _imageLocalDefaultEuler;
        private Vector3 _imageLocalDefaultScale;
        
        // せり上がり状態
        private bool _isRising = false;
        private float _spawnRiseOffset = 0f; // ★生成時のオフセットを記録
        private const float k_RisingAlpha = 0.5f;

        public EmoteData Data => _data;
        public int GridX => _gridX;
        public int GridY => _gridY;
        public bool IsMoving => _currentTween != null && _currentTween.IsActive();
        public bool IsRising => _isRising;
        public float SpawnRiseOffset => _spawnRiseOffset; // ★公開

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Initialize(EmoteData data)
        {
            _data = data;
            
            if (_image != null)
            {
                _image.sprite = data.sprite;
                _image.color = data.emoteColor;
                _originalColor = data.emoteColor;
                _imageLocalDefaultPos = _image.rectTransform.localPosition;
                _imageLocalDefaultEuler = _image.rectTransform.localEulerAngles;
                _imageLocalDefaultScale = _image.rectTransform.localScale;
            }
            
            if (_highlightImage != null)
            {
                _highlightImage.enabled = false;
            }
            
            transform.localScale = Vector3.one;
            _isRising = false;
            _spawnRiseOffset = 0f;

            // Start idle sway if not in chain state
            StartIdleSway();
        }

        public void SetGridPosition(int x, int y)
        {
            _gridX = x;
            _gridY = y;
        }

        /// <summary>
        /// せり上がり状態を設定（生成時のオフセットも記録）
        /// </summary>
        public void SetRising(bool isRising, float spawnOffset = 0f)
        {
            _isRising = isRising;
            
            if (_isRising)
            {
                _spawnRiseOffset = spawnOffset; // ★記録
                
                // せり上がり中：暗くする
                if (_image != null)
                {
                    _image.color = _originalColor * k_RisingAlpha;
                }
            }
            else
            {
                // せり上がり完了：通常の色に戻す
                if (_image != null)
                {
                    // アニメーションで明るくする
                    _image.DOColor(_originalColor, 0.2f);
                }
            }
        }

        public void MoveTo(Vector2 targetAnchoredPosition, float duration)
        {
            _currentTween?.Kill();

            if (_rectTransform != null)
            {
                _currentTween = _rectTransform
                    .DOAnchorPos(targetAnchoredPosition, duration)
                    .SetEase(Ease.OutQuad);
            }
        }

        /// <summary>
        /// 重力っぽい落下で目的地へ移動し、着地時に少し弾む
        /// </summary>
        public void FallTo(Vector2 targetAnchoredPosition, float duration)
        {
            _currentTween?.Kill();
            if (_rectTransform == null)
                return;

            _currentTween = _rectTransform
                .DOAnchorPos(targetAnchoredPosition, duration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    // small squash & stretch bounce
                    var seq = DOTween.Sequence();
                    seq.Append(transform.DOScale(new Vector3(1.08f, 0.94f, 1f), 0.06f).SetEase(Ease.OutQuad));
                    seq.Append(transform.DOScale(Vector3.one, 0.12f).SetEase(Ease.InQuad));
                });
        }

        public void SetMatchHighlight(bool enabled, Color highlightColor)
        {
            if (_highlightImage != null)
            {
                _highlightImage.enabled = enabled;
                _highlightImage.color = highlightColor;
            }

            if (enabled && _image != null)
            {
                _image.DOColor(_originalColor * 1.3f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo);
            }
            else if (_image != null)
            {
                _image.DOKill();
                _image.color = _originalColor;
            }
        }

        public void PlayMatchEffect()
        {
            SetMatchHighlight(false, Color.white);

            if (_matchEffectPrefab != null)
            {
                // Instantiate effect as a sibling under the same parent (so it stays visible after this is destroyed)
                Transform parentTransform = transform.parent;
                GameObject effect = null;

                if (parentTransform != null)
                {
                    effect = Instantiate(_matchEffectPrefab, parentTransform);

                    // If it's a UI effect, align by anchored position; otherwise place at world position
                    RectTransform myRect = GetComponent<RectTransform>();
                    RectTransform effectRect = effect.GetComponent<RectTransform>();
                    if (myRect != null && effectRect != null)
                    {
                        effectRect.anchoredPosition = myRect.anchoredPosition;
                        effectRect.localRotation = Quaternion.identity;
                        effectRect.localScale = Vector3.one;
                    }
                    else
                    {
                        effect.transform.position = transform.position;
                    }
                }
                else
                {
                    effect = Instantiate(_matchEffectPrefab, transform.position, Quaternion.identity);
                }

                if (effect != null)
                {
                    Destroy(effect, 2f);
                }
            }

            _currentTween?.Kill();
            transform.localScale = Vector3.one;
            
            _currentTween = transform
                .DOScale(1.3f, 0.15f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    transform.localScale = Vector3.one;
                });
        }

        /// <summary>
        /// 連鎖中かどうかの状態を設定。連鎖中はアイドル揺れを止める。
        /// </summary>
        public void SetChainActive(bool isActive)
        {
            _isChainActive = isActive;
            if (_isChainActive)
            {
                StopIdleSway();
            }
            else
            {
                StartIdleSway();
            }
        }

        private void StartIdleSway()
        {
            if (_image == null) return;
            if (_isChainActive) return;
            if (_idleSwayAngleDeg <= 0f && _idleSwayScaleAmplitude <= 0f) return;

            _idleTween?.Kill();

            // 位置は固定し、回転とスケールのみで「左右に揺れる感じ」を表現
            _image.rectTransform.localEulerAngles = _imageLocalDefaultEuler;
            _image.rectTransform.localScale = _imageLocalDefaultScale;

            var seq = DOTween.Sequence();
            if (_idleSwayAngleDeg > 0f)
            {
                seq.Join(
                    _image.rectTransform
                        .DOLocalRotate(new Vector3(0f, 0f, _idleSwayAngleDeg), _idleSwayDuration)
                        .SetEase(_idleSwayEase)
                );
            }

            if (_idleSwayScaleAmplitude > 0f)
            {
                var targetScale = new Vector3(
                    _imageLocalDefaultScale.x + _idleSwayScaleAmplitude,
                    _imageLocalDefaultScale.y - _idleSwayScaleAmplitude,
                    _imageLocalDefaultScale.z
                );
                seq.Join(
                    _image.rectTransform
                        .DOScale(targetScale, _idleSwayDuration)
                        .SetEase(_idleSwayEase)
                );
            }

            _idleTween = seq.SetLoops(-1, LoopType.Yoyo);
        }

        private void StopIdleSway()
        {
            _idleTween?.Kill();
            _idleTween = null;
            if (_image != null)
            {
                _image.rectTransform.localEulerAngles = _imageLocalDefaultEuler;
                _image.rectTransform.localScale = _imageLocalDefaultScale;
            }
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();
            _image?.DOKill();
            _idleTween?.Kill();
        }
    }
}