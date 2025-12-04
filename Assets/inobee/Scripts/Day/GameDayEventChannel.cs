using System;
using UnityEngine;

namespace EmoteOrchestra.Core
{
    /// <summary>
    /// イベント通知用のチャンネル (ScriptableObject)
    /// GameDayManager がイベントを発行し、各シーンのオブジェクトがこれを購読する。
    /// </summary>
    [CreateAssetMenu(fileName = "GameDayEventChannel", menuName = "EmoteOrchestra/GameDayEventChannel")]
    public class GameDayEventChannel : ScriptableObject
    {
        /// <summary>
        /// イベントが発行されたときに通知されるアクション (EventID)
        /// </summary>
        public event Action<string> OnEventTriggered;

        /// <summary>
        /// イベントを発行する
        /// </summary>
        public void RaiseEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                Debug.LogWarning("GameDayEventChannel: Raised event with empty ID.");
                return;
            }

            OnEventTriggered?.Invoke(eventId);
        }
    }
}
