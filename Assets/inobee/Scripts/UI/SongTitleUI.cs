using UnityEngine;
using TMPro;
using EmoteOrchestra.Core;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// TopBarの曲名表示
    /// </summary>
    public class SongTitleUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _songTitleText;
        [SerializeField] private TextMeshProUGUI _artistNameText; // オプション

        private string _currentSongTitle = "";

        private void Start()
        {
            UpdateSongTitle();
        }

        private void Update()
        {
            // 定期的に曲名をチェック
            UpdateSongTitle();
        }

        private void UpdateSongTitle()
        {
            MusicGameManager gameManager = ServiceLocator.Get<MusicGameManager>();
            
            if (gameManager == null)
            {
                SetDefaultText();
                return;
            }

            if (gameManager.CurrentSong == null)
            {
                SetDefaultText();
                return;
            }

            // 曲が変わった場合のみ更新
            string newTitle = gameManager.CurrentSong.songTitle;
            if (newTitle != _currentSongTitle)
            {
                _currentSongTitle = newTitle;

                if (_songTitleText != null)
                {
                    _songTitleText.text = _currentSongTitle;
                }

                // アーティスト名も表示する場合
                if (_artistNameText != null)
                {
                    _artistNameText.text = $"- {gameManager.CurrentSong.artistName}";
                }

                Debug.Log($"SongTitleUI: 曲名更新 - {_currentSongTitle}");
            }
        }

        private void SetDefaultText()
        {
            if (_songTitleText != null && string.IsNullOrEmpty(_currentSongTitle))
            {
                _songTitleText.text = "曲を選択してください";
                _currentSongTitle = "";
            }

            if (_artistNameText != null)
            {
                _artistNameText.text = "";
            }
        }
    }
}