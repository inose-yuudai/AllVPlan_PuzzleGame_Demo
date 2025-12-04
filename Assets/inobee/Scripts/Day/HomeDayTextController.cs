using UnityEngine;
using TMPro;

namespace EmoteOrchestra.Core
{
    /// <summary>
    /// Homeシーンなどで GameDayManager の日数を TextMeshPro に反映する
    /// </summary>
    public class HomeDayTextController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _dayText;
        [SerializeField] private bool _autoAssignFromSelf = true;

        private void Awake()
        {
            if (_dayText == null && _autoAssignFromSelf)
            {
                _dayText = GetComponent<TextMeshProUGUI>();
            }
        }

        private void OnEnable()
        {
            SubscribeEvents(true);
            RefreshText();
        }

        private void OnDisable()
        {
            SubscribeEvents(false);
        }

        private void SubscribeEvents(bool subscribe)
        {
            GameDayManager manager = GameDayManager.Instance;
            if (manager == null)
                return;

            if (subscribe)
            {
                manager.OnStateChanged += HandleDayStateChanged;
            }
            else
            {
                manager.OnStateChanged -= HandleDayStateChanged;
            }
        }

        private void HandleDayStateChanged(GameDayState state)
        {
            RefreshText();
        }

        private void RefreshText()
        {
            if (_dayText == null)
                return;

            GameDayManager manager = GameDayManager.Instance;
            if (manager == null)
                return;

            _dayText.text = manager.GetDayLabel();
        }
    }
}

