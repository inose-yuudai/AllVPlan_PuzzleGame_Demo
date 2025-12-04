using System.Collections.Generic;
using UnityEngine;
using TMPro;
using EmoteOrchestra.Data;
using EmoteOrchestra.Core;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// セットリストUI（視聴者数管理を ViewerCountManager に分離済み）
    /// </summary>
    public class SetlistUI : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private Transform _contentTransform;
        [SerializeField] private GameObject _songItemPrefab;

        [Header("曲リスト")]
        [SerializeField] private SongData[] _songs;

        private List<SongItemUI> _songItems = new List<SongItemUI>();
        private int _currentSongIndex = -1;
        private float _updateTimer;

        private const float k_UpdateInterval = 0.5f;

        private void Start()
        {
            InitializeSongList();
            UpdateCurrentSong();
        }

        private void Update()
        {
            _updateTimer += Time.deltaTime;
            if (_updateTimer >= k_UpdateInterval)
            {
                _updateTimer = 0f;
                UpdateCurrentSong();
            }
        }

        private void InitializeSongList()
        {
            foreach (Transform child in _contentTransform)
            {
                Destroy(child.gameObject);
            }
            _songItems.Clear();

            if (_songs == null || _songs.Length == 0)
            {
                Debug.LogWarning("SetlistUI: 曲が設定されていません");
                return;
            }

            for (int i = 0; i < _songs.Length; i++)
            {
                if (_songs[i] == null)
                {
                    Debug.LogWarning($"SetlistUI: Songs[{i}] が null です");
                    continue;
                }

                GameObject itemObj = Instantiate(_songItemPrefab, _contentTransform);
                SongItemUI songItem = itemObj.GetComponent<SongItemUI>();

                if (songItem != null)
                {
                    songItem.Initialize(_songs[i], i);
                    _songItems.Add(songItem);
                }
            }

            Debug.Log($"SetlistUI: 合計 {_songItems.Count} 曲を表示");
        }

        private void UpdateCurrentSong()
        {
            MusicGameManager gameManager = ServiceLocator.Get<MusicGameManager>();
            if (gameManager == null || gameManager.CurrentSong == null)
                return;

            int newIndex = -1;
            for (int i = 0; i < _songs.Length; i++)
            {
                if (_songs[i] == gameManager.CurrentSong)
                {
                    newIndex = i;
                    break;
                }
            }

            if (newIndex != _currentSongIndex)
            {
                if (_currentSongIndex >= 0 && _currentSongIndex < _songItems.Count)
                {
                    _songItems[_currentSongIndex].SetPlaying(false);
                }

                _currentSongIndex = newIndex;
                if (_currentSongIndex >= 0 && _currentSongIndex < _songItems.Count)
                {
                    _songItems[_currentSongIndex].SetPlaying(true);
                }
            }
        }

        /// <summary>
        /// 指定インデックスの曲名を取得
        /// </summary>
        public string GetSongTitle(int index)
        {
            if (_songs == null || index < 0 || index >= _songs.Length)
                return "不明";

            return _songs[index].songTitle;
        }

        /// <summary>
        /// 次の曲名を取得
        /// </summary>
        public string GetNextSongTitle()
        {
            MusicGameManager gameManager = ServiceLocator.Get<MusicGameManager>();
            if (gameManager == null)
                return "不明";

            int nextIndex = gameManager.CurrentSongIndex + 1;
            
            if (nextIndex >= _songs.Length)
                return "最後の曲です";

            return GetSongTitle(nextIndex);
        }

        /// <summary>
        /// 現在の曲インデックスを取得
        /// </summary>
        public int CurrentSongIndex => _currentSongIndex;

        /// <summary>
        /// 曲の総数を取得
        /// </summary>
        public int TotalSongCount => _songs?.Length ?? 0;
    }
}