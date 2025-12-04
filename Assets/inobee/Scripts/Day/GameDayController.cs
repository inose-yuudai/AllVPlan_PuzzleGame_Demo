using UnityEngine;

namespace EmoteOrchestra.Core
{
    /// <summary>
    /// GameDayManager を操作するためのコントローラー (ScriptableObject)
    /// UIボタンなどはこのアセットを参照してメソッドを呼び出す。
    /// GameDayManager (Singleton) が自身をここに登録することで、機能がリンクする。
    /// </summary>
    [CreateAssetMenu(fileName = "GameDayController", menuName = "EmoteOrchestra/GameDayController")]
    public class GameDayController : ScriptableObject
    {
        private GameDayManager _activeManager;

        /// <summary>
        /// GameDayManager が Awake 時に自身を登録する
        /// </summary>
        public void RegisterManager(GameDayManager manager)
        {
            _activeManager = manager;
        }

        /// <summary>
        /// GameDayManager が Destroy 時に登録解除する
        /// </summary>
        public void UnregisterManager(GameDayManager manager)
        {
            if (_activeManager == manager)
            {
                _activeManager = null;
            }
        }

        // --- Public API (UIから呼ばれる) ---

        public void ResetProgress()
        {
            if (_activeManager != null) _activeManager.ResetProgress();
        }

        public void SaveState()
        {
            if (_activeManager != null) _activeManager.SaveState();
        }

        public void GoToNight()
        {
            if (_activeManager != null) _activeManager.GoToNight();
        }

        public void CompleteStreamAndAdvanceDay()
        {
            if (_activeManager != null) _activeManager.CompleteStreamAndAdvanceDay();
        }

        public void TriggerEventsForCurrentDay()
        {
            if (_activeManager != null) _activeManager.TriggerEventsForCurrentDay();
        }
    }
}
