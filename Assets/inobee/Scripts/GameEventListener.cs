using UnityEngine;
using UnityEngine.Events;

namespace EmoteOrchestra.Events
{
    /// <summary>
    /// GameEventを受信するコンポーネント
    /// </summary>
    public class GameEventListener : MonoBehaviour
    {
        [SerializeField] private GameEvent _event;
        [SerializeField] private UnityEvent _response;

        private void OnEnable()
        {
            if (_event != null)
                _event.RegisterListener(this);
        }

        private void OnDisable()
        {
            if (_event != null)
                _event.UnregisterListener(this);
        }

        public void OnEventRaised()
        {
            _response?.Invoke();
        }
    }

    /// <summary>
    /// Int値を受け取るリスナー
    /// </summary>
    public class IntGameEventListener : MonoBehaviour
    {
        [SerializeField] private IntGameEvent _event;
        [SerializeField] private UnityEvent<int> _response;

        private void OnEnable()
        {
            if (_event != null)
                _event.RegisterListener(this);
        }

        private void OnDisable()
        {
            if (_event != null)
                _event.UnregisterListener(this);
        }

        public void OnEventRaised(int value)
        {
            _response?.Invoke(value);
        }
    }
}