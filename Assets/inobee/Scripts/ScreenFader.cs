using System.Threading.Tasks;
using UnityEngine;

namespace EmoteOrchestra.Core
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ScreenFader : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _defaultFadeOut = 0.35f;
        [SerializeField] private float _defaultFadeIn = 0.2f;

        private void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.gameObject.SetActive(false);
        }

        public async Task FadeOut(float duration = -1f)
        {
            if (duration <= 0f) duration = _defaultFadeOut;
            _canvasGroup.gameObject.SetActive(true);
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / duration);
                await Task.Yield();
            }
            _canvasGroup.alpha = 1f;
        }

        public async Task FadeIn(float duration = -1f)
        {
            if (duration <= 0f) duration = _defaultFadeIn;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / duration);
                await Task.Yield();
            }
            _canvasGroup.alpha = 0f;
            _canvasGroup.gameObject.SetActive(false);
        }
    }
}


