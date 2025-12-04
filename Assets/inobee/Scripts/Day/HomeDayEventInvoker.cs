using UnityEngine;
using System.Collections;
using EmoteOrchestra.Core;

namespace EmoteOrchestra.Core
{
    /// <summary>
    /// Homeシーンで有効化されたタイミングで GameDayManager の Day Event を再チェックする
    /// </summary>
    public class HomeDayEventInvoker : MonoBehaviour
    {
        [SerializeField] private bool _invokeOnStart = true;
        [SerializeField] private float _delaySeconds = 0.1f;

        private void Start()
        {
            if (_invokeOnStart)
            {
                StartCoroutine(InvokeCurrentDayEventsDelayed());
            }
        }

        private IEnumerator InvokeCurrentDayEventsDelayed()
        {
            // GameDayManager の初期化を待つ
            yield return new WaitForSeconds(_delaySeconds);
            InvokeCurrentDayEvents();
        }

        public void InvokeCurrentDayEvents()
        {
            // ServiceLocator 経由でも取得を試みる（フォールバック）
            GameDayManager manager = GameDayManager.Instance;
            if (manager == null)
            {
                manager = ServiceLocator.Get<GameDayManager>();
            }

            if (manager == null)
            {
                Debug.LogWarning("HomeDayEventInvoker: GameDayManager not found. Retrying in next frame...");
                StartCoroutine(RetryInvokeNextFrame());
                return;
            }

            manager.TriggerEventsForCurrentDay();
        }

        private IEnumerator RetryInvokeNextFrame()
        {
            yield return null;
            InvokeCurrentDayEvents();
        }
    }
}

