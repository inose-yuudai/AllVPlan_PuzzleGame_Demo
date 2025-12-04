using UnityEngine;
using TMPro;
using DG.Tweening;
using EmoteOrchestra.Core;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// 曲間休憩のUI
    /// </summary>
    public class IntermissionUI : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private GameObject _intermissionPanel;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private TextMeshProUGUI _nextSongText;
        [SerializeField] private TextMeshProUGUI _countdownText;

        private Sequence _fadeSequence;

        private void Update()
        {
            MusicGameManager musicGameManager = ServiceLocator.Get<MusicGameManager>();
            if (musicGameManager == null)
                return;

            // 曲間休憩中かチェック
            if (musicGameManager.IsIntermission)
            {
                ShowIntermission(musicGameManager);
            }
            else
            {
                HideIntermission();
            }
        }

        // IntermissionUI.cs の ShowIntermission メソッドを改善

private void ShowIntermission(MusicGameManager gameManager)
{
    if (_intermissionPanel != null && !_intermissionPanel.activeSelf)
    {
        _intermissionPanel.SetActive(true);

        CanvasGroup canvasGroup = _intermissionPanel.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.5f);
        }
    }

    if (_messageText != null)
    {
        _messageText.text = "お疲れ様でした！";
    }

    // 次の曲の情報を取得
    if (_nextSongText != null)
    {
        SetlistUI setlistUI = FindObjectOfType<SetlistUI>();
        if (setlistUI != null)
        {
            string nextSong = setlistUI.GetNextSongTitle();
            _nextSongText.text = $"次の曲: {nextSong}";
        }
        else
        {
            _nextSongText.text = "次の曲を準備中...";
        }
    }

    // カウントダウン
    if (_countdownText != null)
    {
        float remaining = gameManager.IntermissionRemaining;
        int countdown = Mathf.CeilToInt(remaining);
        _countdownText.text = countdown > 0 ? countdown.ToString() : "START!";
    }
}

        private void HideIntermission()
        {
            if (_intermissionPanel != null && _intermissionPanel.activeSelf)
            {
                // フェードアウト
                CanvasGroup canvasGroup = _intermissionPanel.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
                    {
                        _intermissionPanel.SetActive(false);
                    });
                }
                else
                {
                    _intermissionPanel.SetActive(false);
                }
            }
        }

        private void OnDestroy()
        {
            _fadeSequence?.Kill();
        }
    }
}