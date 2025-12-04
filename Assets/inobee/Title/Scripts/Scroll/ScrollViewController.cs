// ScrollViewController.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Title.Scroll
{
    public class ScrollViewController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;
        [SerializeField] private Transform _recommendedVideosContainer;
        [SerializeField] private GameObject _recommendedVideoItemPrefab;
        
        [Header("Scroll Settings")]
        [SerializeField] private int _recommendedVideoCount = 10;
        [SerializeField] private float _itemSpacing = 20f;
        
        private float _maxScrollExtent;
        private List<GameObject> _videoItems = new List<GameObject>();

        private void Start()
        {
            if (_recommendedVideoItemPrefab != null)
            {
                GenerateRecommendedVideos();
            }
            
            CalculateMaxScrollExtent();
            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        private void GenerateRecommendedVideos()
        {
            for (int i = 0; i < _recommendedVideoCount; i++)
            {
                GameObject item = Instantiate(_recommendedVideoItemPrefab, _recommendedVideosContainer);
                _videoItems.Add(item);
                
                // おすすめ動画アイテムの初期化
                RecommendedVideoItem videoItem = item.GetComponent<RecommendedVideoItem>();
                if (videoItem != null)
                {
                    videoItem.Setup($"おすすめ動画 {i + 1}", $"チャンネル名 {i + 1}");
                }
            }
            
            // レイアウト更新
            Canvas.ForceUpdateCanvases();
        }

        private void CalculateMaxScrollExtent()
        {
            // コンテンツサイズの更新を待つ
            Canvas.ForceUpdateCanvases();
            
            float contentHeight = _content.rect.height;
            float viewportHeight = _scrollRect.viewport.rect.height;
            
            _maxScrollExtent = Mathf.Max(0, contentHeight - viewportHeight);
        }

        private void OnScrollValueChanged(Vector2 scrollPosition)
        {
            // スクロール位置の計算（0 = 最下部, 1 = 最上部）
            float currentScroll = (1f - scrollPosition.y) * _maxScrollExtent;
            
            // スクロール範囲制限
            if (currentScroll > _maxScrollExtent)
            {
                _scrollRect.verticalNormalizedPosition = 0f;
            }
            else if (currentScroll < 0)
            {
                _scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private void OnDestroy()
        {
            _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
        }

        [ContextMenu("Recalculate Scroll Extent")]
        private void RecalculateScrollExtent()
        {
            CalculateMaxScrollExtent();
            Debug.Log($"Max Scroll Extent: {_maxScrollExtent}");
        }
    }
}