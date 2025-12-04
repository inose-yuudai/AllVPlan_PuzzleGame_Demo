// AnimatedImageController.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections;
using Title.Animation.States;
using Title.Transition;

namespace Title.Animation
{
    public class AnimatedImageController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Animation Components")]
        [SerializeField] private Image _animationImage;
        [SerializeField] private Sprite[] _animationFrames;
        [SerializeField] private float _frameRate = 30f;
        
        [Header("UI References")]
        [SerializeField] private GameObject _thumbnailImage;
        [SerializeField] private CanvasGroup _controlsGroup;
        [SerializeField] private RectTransform _animationContainer;
        
        [Header("Transition Settings")]
        [SerializeField] private float _suckInDuration = 1.5f;
        [SerializeField] private Ease _suckInEase = Ease.InCubic;
        [SerializeField] private string _nextSceneName = "GameScene";

		[Header("Simple Float Settings")]
		[SerializeField] private bool _useSimpleFloat = true;
		[SerializeField] private RectTransform _floatTarget;
		[SerializeField] private float _floatAmplitude = 12f; // pixels
		[SerializeField] private float _floatDuration = 1.4f; // seconds (half cycle)
		[SerializeField] private bool _fadeDuringFloat = false;
		[SerializeField] private float _fadeMinAlpha = 0.7f;
        
        private IAnimationState _currentState;
        private SceneTransitionController _transitionController;
        private bool _isTransitioning;
        private Coroutine _animationCoroutine;
        private int _currentFrameIndex;

		private Vector2 _floatInitialAnchoredPos;
		private CanvasGroup _floatCanvasGroup;

        private void Awake()
        {
            _transitionController = FindObjectOfType<SceneTransitionController>();
            
            if (_transitionController == null)
            {
                Debug.LogError("SceneTransitionController not found in scene!");
            }
            
            _controlsGroup.alpha = 0f;
            ChangeState(new AnimationIdleState());

			// Setup simple float (optional, animator不要のふわっと動き)
			if (_floatTarget == null)
			{
				_floatTarget = _animationContainer != null ? _animationContainer : GetComponent<RectTransform>();
			}
			if (_useSimpleFloat && _floatTarget != null)
			{
				StartSimpleFloat();
			}
        }

        public void ChangeState(IAnimationState newState)
        {
            _currentState?.Exit(this);
            _currentState = newState;
            _currentState.Enter(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isTransitioning) return;
            _currentState.OnHoverEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isTransitioning) return;
            _currentState.OnHoverExit(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isTransitioning) return;
            _currentState.OnClick(this);
        }

        public void PlayAnimation()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }
            _animationCoroutine = StartCoroutine(AnimationLoop());
        }

        public void StopAnimation()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }
            _currentFrameIndex = 0;
            UpdateFrame();
        }

		private void StartSimpleFloat()
		{
			_floatInitialAnchoredPos = _floatTarget.anchoredPosition;
			DOTween.Kill(_floatTarget);
			_floatTarget.anchoredPosition = _floatInitialAnchoredPos;
			_floatTarget
				.DOAnchorPosY(_floatInitialAnchoredPos.y + _floatAmplitude, _floatDuration)
				.SetEase(Ease.InOutSine)
				.SetLoops(-1, LoopType.Yoyo);

			if (_fadeDuringFloat)
			{
				// 優先: アニメーション画像に CanvasGroup、無ければターゲットに付与
				if (_animationImage != null)
				{
					_floatCanvasGroup = _animationImage.GetComponent<CanvasGroup>();
					if (_floatCanvasGroup == null) _floatCanvasGroup = _animationImage.gameObject.AddComponent<CanvasGroup>();
				}
				else
				{
					_floatCanvasGroup = _floatTarget.GetComponent<CanvasGroup>();
					if (_floatCanvasGroup == null) _floatCanvasGroup = _floatTarget.gameObject.AddComponent<CanvasGroup>();
				}
				_floatCanvasGroup.alpha = 1f;
				DOTween.Kill(_floatCanvasGroup);
				_floatCanvasGroup
					.DOFade(_fadeMinAlpha, _floatDuration)
					.SetEase(Ease.InOutSine)
					.SetLoops(-1, LoopType.Yoyo);
			}
		}

        private IEnumerator AnimationLoop()
        {
            float frameDuration = 1f / _frameRate;
            WaitForSeconds wait = new WaitForSeconds(frameDuration);
            
            while (true)
            {
                UpdateFrame();
                _currentFrameIndex = (_currentFrameIndex + 1) % _animationFrames.Length;
                yield return wait;
            }
        }

        private void UpdateFrame()
        {
            if (_animationFrames.Length > 0)
            {
                _animationImage.sprite = _animationFrames[_currentFrameIndex];
            }
        }

        public void ShowThumbnail()
        {
            _thumbnailImage.SetActive(true);
        }

        public void HideThumbnail()
        {
            _thumbnailImage.SetActive(false);
        }

        public void ShowControls()
        {
            _controlsGroup.DOFade(1f, 0.3f);
        }

        public void HideControls()
        {
            _controlsGroup.DOFade(0f, 0.3f);
        }

        public void StartPlayTransition()
        {
            _isTransitioning = true;
            
            // アニメーション開始
            PlayAnimation();
            HideThumbnail();
            
            // 吸い込み演出
            Sequence suckSequence = DOTween.Sequence();
            
            // 拡大してから縮小（吸い込まれる感じ）
            suckSequence.Append(_animationContainer.DOScale(1.2f, _suckInDuration * 0.3f).SetEase(Ease.OutQuad));
            suckSequence.Append(_animationContainer.DOScale(0f, _suckInDuration * 0.7f).SetEase(_suckInEase));
            
            // コントロールUIをフェードアウト
            suckSequence.Join(_controlsGroup.DOFade(0f, _suckInDuration * 0.5f));
            
            // 完了時にシーン遷移
            suckSequence.OnComplete(() =>
            {
                if (_transitionController != null)
                {
                    _transitionController.TransitionToScene(_nextSceneName);
                }
                else
                {
                    Debug.LogError("Cannot transition: SceneTransitionController is null!");
                }
            });
        }

        private void OnDestroy()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }
			DOTween.Kill(_animationContainer);
			DOTween.Kill(_controlsGroup);
			if (_floatTarget != null) DOTween.Kill(_floatTarget);
			if (_floatCanvasGroup != null) DOTween.Kill(_floatCanvasGroup);
        }
    }
}