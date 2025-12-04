// CommentItem.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Title.Comment
{
    public class CommentItem : MonoBehaviour
    {
        [SerializeField] private Image _userIcon;
        [SerializeField] private TextMeshProUGUI _userNameText;
        [SerializeField] private TextMeshProUGUI _commentText;
        [SerializeField] private TextMeshProUGUI _timestampText;
        
        [Header("Default Settings")]
        [SerializeField] private Sprite _defaultUserIcon;
        
        public void Setup(Data.CommentData data)
        {
            _userNameText.text = data.userName;
            _commentText.text = data.commentText;
            
            if (_timestampText != null)
            {
                _timestampText.text = data.timestamp;
            }
            
            // アイコン読み込み
            LoadUserIcon(data.userIconPath);
        }

        private void LoadUserIcon(string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath))
            {
                SetDefaultIcon();
                return;
            }
            
            Sprite icon = Resources.Load<Sprite>(iconPath);
            
            if (icon != null)
            {
                _userIcon.sprite = icon;
            }
            else
            {
                Debug.LogWarning($"Failed to load icon from path: {iconPath}");
                SetDefaultIcon();
            }
        }

        private void SetDefaultIcon()
        {
            if (_defaultUserIcon != null)
            {
                _userIcon.sprite = _defaultUserIcon;
            }
        }
    }
}