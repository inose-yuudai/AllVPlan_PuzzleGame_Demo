using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Title.UI
{
    public class LikeButtonController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button likeButton;
        [SerializeField] private TMP_Text likeCountText;
        [SerializeField] private Animator likeAnimator; // Optional. Trigger name should be in likeAnimatorTrigger

        [Header("Animation")]
        [SerializeField] private string likeAnimatorTrigger = "Like";
        [SerializeField] private float popScale = 1.2f; // fallback pop animation
        [SerializeField] private float popDuration = 0.12f; // seconds for each half of pop

        private const string PlayerPrefsKeyLikeCount = "title_like_count";
        private int likeCount;
        private bool isPopping;

        private Vector3 initialScale;

        private void Awake()
        {
            initialScale = transform.localScale;
            likeCount = PlayerPrefs.GetInt(PlayerPrefsKeyLikeCount, 0);
            UpdateLikeCountText();

            if (likeButton != null)
            {
                likeButton.onClick.AddListener(OnLikeClicked);
            }
        }

        private void OnDestroy()
        {
            if (likeButton != null)
            {
                likeButton.onClick.RemoveListener(OnLikeClicked);
            }
        }

        private void OnLikeClicked()
        {
            likeCount++;
            PlayerPrefs.SetInt(PlayerPrefsKeyLikeCount, likeCount);
            PlayerPrefs.Save();
            UpdateLikeCountText();

            PlayLikeAnimation();
        }

        private void UpdateLikeCountText()
        {
            if (likeCountText != null)
            {
                likeCountText.text = likeCount.ToString();
            }
        }

        private void PlayLikeAnimation()
        {
            if (likeAnimator != null)
            {
                likeAnimator.ResetTrigger(likeAnimatorTrigger);
                likeAnimator.SetTrigger(likeAnimatorTrigger);
                return;
            }

            // Fallback simple pop animation if no Animator is provided
            if (!isPopping && gameObject.activeInHierarchy)
            {
                StartCoroutine(PopCoroutine());
            }
        }

        private IEnumerator PopCoroutine()
        {
            isPopping = true;

            // Scale up
            float t = 0f;
            while (t < popDuration)
            {
                t += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(t / popDuration);
                float scale = Mathf.Lerp(1f, popScale, normalized);
                transform.localScale = initialScale * scale;
                yield return null;
            }

            // Scale back
            t = 0f;
            while (t < popDuration)
            {
                t += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(t / popDuration);
                float scale = Mathf.Lerp(popScale, 1f, normalized);
                transform.localScale = initialScale * scale;
                yield return null;
            }

            transform.localScale = initialScale;
            isPopping = false;
        }
    }
}


