// SceneTransitionController.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

namespace Title.Transition
{
    public class SceneTransitionController : MonoBehaviour
    {
        [Header("Fade Settings")]
        [SerializeField] private Image _fadeImage;
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private Ease _fadeEase = Ease.InOutQuad;
        
        [Header("Loading Settings")]
        [SerializeField] private bool _useAsyncLoading = true;
        
        private bool _isTransitioning;

        private void Awake()
        {
            if (_fadeImage == null)
            {
                Debug.LogError("Fade Image is not assigned!");
                return;
            }
            
            // 初期状態は透明
            _fadeImage.color = new Color(0, 0, 0, 0);
            _fadeImage.raycastTarget = false;
        }

        public void TransitionToScene(string sceneName)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("Transition already in progress!");
                return;
            }
            
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("Scene name is empty!");
                return;
            }
            
            _isTransitioning = true;
            _fadeImage.raycastTarget = true;
            
            // フェードアウト
            _fadeImage.DOFade(1f, _fadeDuration)
                .SetEase(_fadeEase)
                .OnComplete(() =>
                {
                    if (_useAsyncLoading)
                    {
                        LoadSceneAsync(sceneName);
                    }
                    else
                    {
                        SceneManager.LoadScene(sceneName);
                    }
                });
        }

        private async void LoadSceneAsync(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            
            // ロード進捗を待つ
            while (asyncLoad.progress < 0.9f)
            {
                await System.Threading.Tasks.Task.Yield();
            }
            
            // シーンアクティベーション
            asyncLoad.allowSceneActivation = true;
        }

        private void OnDestroy()
        {
            DOTween.Kill(_fadeImage);
        }
    }
}
