using System.Collections.Generic;
using UnityEngine;

namespace EmoteOrchestra.Events
{
    /// <summary>
    /// 引数なしのイベント
    /// </summary>
    [CreateAssetMenu(fileName = "GameEvent", menuName = "EmoteOrchestra/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        private readonly List<GameEventListener> _listeners = new List<GameEventListener>();

        public void Raise()
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i].OnEventRaised();
            }
        }

        public void RegisterListener(GameEventListener listener)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void UnregisterListener(GameEventListener listener)
        {
            _listeners.Remove(listener);
        }
    }

    /// <summary>
    /// int値を渡せるイベント
    /// </summary>
    [CreateAssetMenu(fileName = "IntGameEvent", menuName = "EmoteOrchestra/Events/Int Game Event")]
    public class IntGameEvent : ScriptableObject
    {
        private readonly List<IntGameEventListener> _listeners = new List<IntGameEventListener>();

        public void Raise(int value)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i].OnEventRaised(value);
            }
        }

        public void RegisterListener(IntGameEventListener listener)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void UnregisterListener(IntGameEventListener listener)
        {
            _listeners.Remove(listener);
        }
    }
}