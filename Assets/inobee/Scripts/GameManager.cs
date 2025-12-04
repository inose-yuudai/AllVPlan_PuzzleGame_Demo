using UnityEngine;
using EmoteOrchestra.Core;
using EmoteOrchestra.Data;
using DG.Tweening; // using を忘れずに

namespace EmoteOrchestra.Core
{
    /// <summary>
    /// ゲーム全体の管理
    /// </summary>
    public class MusicGameManager : MonoBehaviour
    {
        [Header("プレイリスト")]
        [SerializeField] private SongData[] _playlist;
        [SerializeField] private bool _loopPlaylist = true; // プレイリストをループするか

        [Header("曲間の待機時間")]
        [SerializeField] private float _intermissionDuration = 3f; // 曲間の休憩時間（秒）

        [Header("現在の状態")]
        [SerializeField] private int _currentSongIndex = 0;
        [SerializeField] private bool _isPaused;

        private SongData _currentSong;
        private int _currentScore;
        private float _currentTime;
        private float _songDuration;
        private bool _isIntermission; // 曲間休憩中かどうか
        private float _intermissionTimer;

        public SongData CurrentSong => _currentSong;
        public int CurrentScore => _currentScore;
        public float CurrentTime => _currentTime;
        public float SongDuration => _songDuration;
        public bool IsPaused => _isPaused;
        public int CurrentSongIndex => _currentSongIndex;
        public int TotalSongs => _playlist != null ? _playlist.Length : 0;

        private void Awake()
        {
            ServiceLocator.Register<MusicGameManager>(this);
            DOTween.Init(); // ★ これが必須です
        }

        private void Start()
        {
            // プレイリストの最初の曲を開始
            if (_playlist != null && _playlist.Length > 0)
            {
                StartSong(_playlist[_currentSongIndex]);
            }
            else
            {
                Debug.LogError("GameManager: プレイリストが設定されていません");
            }
        }

        private void Update()
        {
            if (_isPaused)
                return;

            // 曲間休憩中
            if (_isIntermission)
            {
                UpdateIntermission();
                return;
            }

            // 通常の曲再生中
            if (_currentSong != null)
            {
                _currentTime += Time.deltaTime;

                // 曲が終了したら次の曲へ
                if (_currentTime >= _songDuration)
                {
                    OnSongEnd();
                }
            }
        }

        /// <summary>
        /// 曲を開始
        /// </summary>
        public void StartSong(SongData song)
        {
            if (song == null)
            {
                Debug.LogError("GameManager: 曲データがnullです");
                return;
            }

            _currentSong = song;
            _currentTime = 0f;
            _currentScore = 0;
            _isPaused = false;
            _isIntermission = false;

            // 曲の長さを自動取得
            _songDuration = song.GetActualDuration();

            // BGMを再生
            // AudioManager audioManager = ServiceLocator.Get<AudioManager>();
            // if (audioManager != null && song.audioClip != null)
            // {
            //     audioManager.PlayBGM(song.audioClip);
            // }

            Debug.Log($"GameManager: 曲開始 [{_currentSongIndex + 1}/{_playlist.Length}] {song.songTitle} - 長さ: {song.GetDurationString()}");
        }

        /// <summary>
        /// 曲が終了した時の処理
        /// </summary>
        private void OnSongEnd()
        {
            Debug.Log($"GameManager: 曲終了 - {_currentSong.songTitle} - スコア: {_currentScore}");

            // BGMを停止
            // AudioManager audioManager = ServiceLocator.Get<AudioManager>();
            // if (audioManager != null)
            // {
            //     audioManager.StopBGM();
            // }

            // 次の曲があるかチェック
            if (HasNextSong())
            {
                // 曲間休憩を開始
                StartIntermission();
            }
            else
            {
                // 全曲終了
                OnAllSongsComplete();
            }
        }

        /// <summary>
        /// 曲間休憩を開始
        /// </summary>
        private void StartIntermission()
        {
            _isIntermission = true;
            _intermissionTimer = 0f;

            Debug.Log($"GameManager: 曲間休憩 ({_intermissionDuration}秒)");
        }

        /// <summary>
        /// 曲間休憩の更新
        /// </summary>
        private void UpdateIntermission()
        {
            _intermissionTimer += Time.deltaTime;

            if (_intermissionTimer >= _intermissionDuration)
            {
                // 次の曲へ
                PlayNextSong();
            }
        }

        /// <summary>
        /// 次の曲を再生
        /// </summary>
        public void PlayNextSong()
        {
            _currentSongIndex++;

            // プレイリストの範囲チェック
            if (_currentSongIndex >= _playlist.Length)
            {
                if (_loopPlaylist)
                {
                    // ループする場合は最初に戻る
                    _currentSongIndex = 0;
                    Debug.Log("GameManager: プレイリストをループします");
                }
                else
                {
                    // ループしない場合は終了
                    OnAllSongsComplete();
                    return;
                }
            }

            // 次の曲を開始
            StartSong(_playlist[_currentSongIndex]);
        }

        /// <summary>
        /// 前の曲を再生
        /// </summary>
        public void PlayPreviousSong()
        {
            _currentSongIndex--;

            if (_currentSongIndex < 0)
            {
                _currentSongIndex = _loopPlaylist ? _playlist.Length - 1 : 0;
            }

            StartSong(_playlist[_currentSongIndex]);
        }

        /// <summary>
        /// 指定したインデックスの曲を再生
        /// </summary>
        public void PlaySongAtIndex(int index)
        {
            if (index < 0 || index >= _playlist.Length)
            {
                Debug.LogError($"GameManager: 無効な曲インデックス: {index}");
                return;
            }

            _currentSongIndex = index;
            StartSong(_playlist[_currentSongIndex]);
        }

        /// <summary>
        /// 次の曲があるかチェック
        /// </summary>
        private bool HasNextSong()
        {
            if (_loopPlaylist)
                return true;

            return _currentSongIndex < _playlist.Length - 1;
        }

        /// <summary>
        /// 全曲終了時の処理
        /// </summary>
        private void OnAllSongsComplete()
        {
            _isPaused = true;
            Debug.Log("GameManager: 全曲終了！");

            // TODO: リザルト画面を表示
            // 例: 総合スコア、ランク、統計など
        }

        /// <summary>
        /// スコアを追加
        /// </summary>
        public void AddScore(int score)
        {
            _currentScore += score;
        }

        /// <summary>
        /// ゲームをポーズ
        /// </summary>
        public void PauseGame()
        {
            _isPaused = true;
            Time.timeScale = 0f;

            // AudioManager audioManager = ServiceLocator.Get<AudioManager>();
            // if (audioManager != null)
            // {
            //     // BGMの一時停止機能があれば使用
            // }
        }

        /// <summary>
        /// ゲームを再開
        /// </summary>
        public void ResumeGame()
        {
            _isPaused = false;
            Time.timeScale = 1f;

            // AudioManager audioManager = ServiceLocator.Get<AudioManager>();
            // if (audioManager != null)
            // {
            //     // BGMの再開機能があれば使用
            // }
        }

        /// <summary>
        /// 現在の再生時間を分:秒形式で取得
        /// </summary>
        public string GetCurrentTimeString()
        {
            float time = _isIntermission ? 0f : _currentTime;
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            return $"{minutes}:{seconds:00}";
        }

        /// <summary>
        /// 曲の残り時間を取得
        /// </summary>
        public float GetRemainingTime()
        {
            if (_isIntermission)
                return 0f;

            return Mathf.Max(0, _songDuration - _currentTime);
        }

        /// <summary>
        /// 曲間休憩中かどうか
        /// </summary>
        public bool IsIntermission => _isIntermission;

        /// <summary>
        /// 曲間休憩の残り時間
        /// </summary>
        public float IntermissionRemaining => Mathf.Max(0, _intermissionDuration - _intermissionTimer);
    }
}