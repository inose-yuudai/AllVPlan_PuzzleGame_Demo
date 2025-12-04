using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace EmoteOrchestra.Core
{
    /// <summary>
    /// 特定の日付にイベントを仕込める定義
    /// </summary>
    [Serializable]
    public class DayEventDefinition
    {
        public int day;
        public string eventId;
        public UnityEvent onTriggered;
    }

    /// <summary>
    /// 日数・昼夜サイクル・配信終了時の進行管理 + JSON永続化
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameDayManager : MonoBehaviour
    {
        private const string k_SaveFileName = "gameday_state.json";

        [Header("Initial State")]
        [SerializeField] private GameDayState _initialState = new GameDayState();

        [Header("Day Events (Optional)")]
        [SerializeField] private DayEventDefinition[] _dayEvents;

        [Header("Debug")]
        [SerializeField] private bool _loadOnAwake = true;
        [SerializeField] private bool _saveOnChange = true;

        private static GameDayManager _instance;
        private GameDayState _currentState;

        public static GameDayManager Instance => _instance;

        public GameDayState CurrentState => _currentState;
        public int CurrentDay => _currentState != null ? _currentState.CurrentDay : 1;
        public DayPhase CurrentPhase => _currentState != null ? _currentState.CurrentPhase : DayPhase.Daytime;
        public StreamType CurrentStreamType => _currentState != null ? _currentState.CurrentStreamType : StreamType.None;

        public event Action<GameDayState> OnStateChanged;
        public event Action<GameDayState> OnDayAdvanced;
        public event Action<GameDayState> OnPhaseChanged;

        private static string SaveFilePath
        {
            get
            {
                return Path.Combine(Application.persistentDataPath, k_SaveFileName);
            }
        }

        [Header("Architecture (ScriptableObjects)")]
        [SerializeField] private GameDayEventChannel _eventChannel;
        [SerializeField] private GameDayController _controller;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Controllerに自身を登録
            if (_controller != null)
            {
                _controller.RegisterManager(this);
            }

            // 既存の ServiceLocator への登録
            ServiceLocator.Register<GameDayManager>(this);

            if (_loadOnAwake)
            {
                LoadState();
            }
            else
            {
                _currentState = _initialState != null ? _initialState.DeepCopy() : new GameDayState();
                SaveStateIfNeeded();
            }

            NotifyAllChanged();
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.UnregisterManager(this);
            }
        }

        #region Public API

        /// <summary>
        /// Home などで「〇日目」表示用
        /// </summary>
        public string GetDayLabel()
        {
            return $"{CurrentDay}日目";
        }

        /// <summary>
        /// 昼 → 夜へ遷移
        /// </summary>
        public void GoToNight()
        {
            if (_currentState == null) return;

            DayPhase prev = _currentState.CurrentPhase;
            _currentState.SetPhase(DayPhase.Night);
            HandlePhaseChanged(prev, _currentState.CurrentPhase);
        }

        /// <summary>
        /// 夜に配信開始（歌/雑談ゲーム/お絵描きゲームなど）
        /// </summary>
        public void StartStream(StreamType streamType)
        {
            if (_currentState == null) return;

            DayPhase prev = _currentState.CurrentPhase;
            _currentState.StartStream(streamType);
            HandlePhaseChanged(prev, _currentState.CurrentPhase);
        }

        /// <summary>
        /// 配信終了時に呼ぶ。日数 +1 して 昼 に戻す。
        /// </summary>
        public void CompleteStreamAndAdvanceDay()
        {
            if (_currentState == null) return;

            int prevDay = _currentState.CurrentDay;
            DayPhase prevPhase = _currentState.CurrentPhase;

            _currentState.CompleteStreamAndAdvanceDay();

            if (_currentState.CurrentDay != prevDay)
            {
                SaveStateIfNeeded();
                NotifyDayAdvanced();
                CheckAndTriggerDayEvents();
            }
            else
            {
                SaveStateIfNeeded();
            }

            if (_currentState.CurrentPhase != prevPhase)
            {
                NotifyPhaseChanged();
            }

            NotifyStateChanged();
        }

        /// <summary>
        /// デバッグ・タイトル画面用：日数/フェーズを初期状態に戻し、セーブもリセット。
        /// </summary>
        public void ResetProgress()
        {
            if (_currentState == null)
            {
                _currentState = new GameDayState();
            }

            _currentState.Reset();
            SaveStateToDisk();
            NotifyAllChanged();
        }

        /// <summary>
        /// 強制的に日数をセット（デバッグ用）
        /// </summary>
        public void ForceSetDay(int day)
        {
            if (_currentState == null) return;

            int prevDay = _currentState.CurrentDay;
            _currentState.ForceSetDay(day);

            if (_currentState.CurrentDay != prevDay)
            {
                SaveStateIfNeeded();
                NotifyDayAdvanced();
                NotifyStateChanged();
                CheckAndTriggerDayEvents();
            }
        }

        /// <summary>
        /// 手動セーブが欲しい時用
        /// </summary>
        public void SaveState()
        {
            SaveStateToDisk();
        }

        /// <summary>
        /// セーブデータが存在するか
        /// </summary>
        public bool HasSaveData()
        {
            return File.Exists(SaveFilePath);
        }

        /// <summary>
        /// 現在の日付に紐づく Day Event を明示的に呼び出す（Homeシーン復帰時など）
        /// </summary>
        public void TriggerEventsForCurrentDay()
        {
            CheckAndTriggerDayEvents();
        }

        #endregion

        #region Persistence

        private void LoadState()
        {
            if (!File.Exists(SaveFilePath))
            {
#if UNITY_EDITOR
                Debug.Log($"GameDayManager: Save file not found. Creating default state. Path={SaveFilePath}");
#endif
                _currentState = _initialState != null ? _initialState.DeepCopy() : new GameDayState();
                SaveStateIfNeeded();
                return;
            }

            try
            {
                string json = File.ReadAllText(SaveFilePath);
                if (string.IsNullOrEmpty(json))
                {
                    _currentState = _initialState != null ? _initialState.DeepCopy() : new GameDayState();
                    SaveStateIfNeeded();
                    return;
                }

                GameDayState loaded = JsonUtility.FromJson<GameDayState>(json);
                if (loaded == null)
                {
                    Debug.LogWarning("GameDayManager: Failed to deserialize GameDayState. Using default.");
                    _currentState = _initialState != null ? _initialState.DeepCopy() : new GameDayState();
                    SaveStateIfNeeded();
                }
                else
                {
                    _currentState = loaded;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"GameDayManager: Failed to load state: {e.Message}");
                _currentState = _initialState != null ? _initialState.DeepCopy() : new GameDayState();
                SaveStateIfNeeded();
            }
        }

        private void SaveStateIfNeeded()
        {
            if (_saveOnChange)
            {
                SaveStateToDisk();
            }
        }

        private void SaveStateToDisk()
        {
            if (_currentState == null)
            {
                Debug.LogWarning("GameDayManager: No state to save.");
                return;
            }

            try
            {
                string json = JsonUtility.ToJson(_currentState, true);
                File.WriteAllText(SaveFilePath, json);
#if UNITY_EDITOR
                Debug.Log($"GameDayManager: Saved state to: {SaveFilePath}");
#endif
            }
            catch (Exception e)
            {
                Debug.LogError($"GameDayManager: Failed to save state: {e.Message}");
            }
        }

        #endregion

        #region Events & Helpers

        private void HandlePhaseChanged(DayPhase prev, DayPhase current)
        {
            if (prev != current)
            {
                SaveStateIfNeeded();
                NotifyPhaseChanged();
                NotifyStateChanged();
            }
            else
            {
                SaveStateIfNeeded();
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged()
        {
            OnStateChanged?.Invoke(_currentState);
        }

        private void NotifyDayAdvanced()
        {
            OnDayAdvanced?.Invoke(_currentState);
        }

        private void NotifyPhaseChanged()
        {
            OnPhaseChanged?.Invoke(_currentState);
        }

        private void NotifyAllChanged()
        {
            NotifyDayAdvanced();
            NotifyPhaseChanged();
            NotifyStateChanged();
        }

        /// <summary>
        /// 指定日付イベントをチェックして発火
        /// </summary>
        private void CheckAndTriggerDayEvents()
        {
            if (_dayEvents == null || _dayEvents.Length == 0 || _currentState == null)
                return;

            int day = _currentState.CurrentDay;
            for (int i = 0; i < _dayEvents.Length; i++)
            {
                DayEventDefinition def = _dayEvents[i];
                if (def != null && def.day == day)
                {
                    // UnityEvent (Inspector設定用)
                    if (def.onTriggered != null)
                    {
                        def.onTriggered.Invoke();
                    }

                    // IDベースのイベント通知 (SO経由)
                    if (!string.IsNullOrEmpty(def.eventId) && _eventChannel != null)
                    {
                        _eventChannel.RaiseEvent(def.eventId);
                    }
                }
            }
        }

        #endregion
    }
}