using System;
using UnityEngine;

namespace EmoteOrchestra.Core
{
    public enum DayPhase
    {
        Daytime,
        Night,
        Streaming,
    }

    public enum StreamType
    {
        None,
        Singing,
        TalkGame,
        DrawingGame,
    }

    /// <summary>
    /// 日数・昼夜・配信種別などのセーブ用データ
    /// </summary>
    [Serializable]
    public class GameDayState
    {
        [SerializeField] private int _currentDay = 1;
        [SerializeField] private DayPhase _currentPhase = DayPhase.Daytime;
        [SerializeField] private StreamType _currentStreamType = StreamType.None;

        public int CurrentDay => _currentDay;
        public DayPhase CurrentPhase => _currentPhase;
        public StreamType CurrentStreamType => _currentStreamType;

        public GameDayState()
        {
            _currentDay = 1;
            _currentPhase = DayPhase.Daytime;
            _currentStreamType = StreamType.None;
        }

        public void Reset()
        {
            _currentDay = 1;
            _currentPhase = DayPhase.Daytime;
            _currentStreamType = StreamType.None;
        }

        public void SetPhase(DayPhase phase)
        {
            _currentPhase = phase;

            // 昼 or 夜に戻るときは配信種別をクリア
            if (phase != DayPhase.Streaming)
            {
                _currentStreamType = StreamType.None;
            }
        }

        public void StartStream(StreamType streamType)
        {
            _currentPhase = DayPhase.Streaming;
            _currentStreamType = streamType;
        }

        /// <summary>
        /// 配信終了 → 日付を1進めて 昼 に戻す
        /// </summary>
        public void CompleteStreamAndAdvanceDay()
        {
            _currentDay = Mathf.Max(1, _currentDay + 1);
            _currentPhase = DayPhase.Daytime;
            _currentStreamType = StreamType.None;
        }

        public void ForceSetDay(int day)
        {
            _currentDay = Mathf.Max(1, day);
        }

        public GameDayState DeepCopy()
        {
            // JsonUtility で簡易ディープコピー
            string json = JsonUtility.ToJson(this);
            return JsonUtility.FromJson<GameDayState>(json);
        }
    }
}