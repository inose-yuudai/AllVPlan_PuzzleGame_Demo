// RecommendedVideoItem.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Title.Scroll
{
    public class RecommendedVideoItem : MonoBehaviour
    {
        [SerializeField] private Image _thumbnailImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _channelText;
        [SerializeField] private TextMeshProUGUI _viewCountText;
        [SerializeField] private Button _itemButton;
        
        private string _videoId;

        private void Awake()
        {
            if (_itemButton != null)
            {
                _itemButton.onClick.AddListener(OnItemClicked);
            }
        }

        public void Setup(string title, string channelName, string videoId = "", int viewCount = 0)
        {
            _videoId = videoId;
            
            if (_titleText != null)
            {
                _titleText.text = title;
            }
            
            if (_channelText != null)
            {
                _channelText.text = channelName;
            }
            
            if (_viewCountText != null && viewCount > 0)
            {
                _viewCountText.text = FormatViewCount(viewCount);
            }
        }

        public void SetThumbnail(Sprite thumbnail)
        {
            if (_thumbnailImage != null && thumbnail != null)
            {
                _thumbnailImage.sprite = thumbnail;
            }
        }

        private void OnItemClicked()
        {
            Debug.Log($"Recommended video clicked: {_videoId}");
            // ここで動画の詳細表示やシーン遷移などの処理を実装
        }

        private string FormatViewCount(int count)
        {
            if (count >= 10000)
            {
                return $"{count / 10000}万回視聴";
            }
            else if (count >= 1000)
            {
                return $"{count / 1000}k回視聴";
            }
            else
            {
                return $"{count}回視聴";
            }
        }

        private void OnDestroy()
        {
            if (_itemButton != null)
            {
                _itemButton.onClick.RemoveListener(OnItemClicked);
            }
        }
    }
}