// CommentSystem.cs - すでに正しいので変更なし
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Title.Comment.Data;

namespace Title.Comment
{
    public class CommentSystem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform _commentContainer;
        [SerializeField] private GameObject _commentItemPrefab;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _submitButton;
        [SerializeField] private ScrollRect _scrollRect;
        
        [Header("Settings")]
        [SerializeField] private string _defaultUserName = "匿名";
        [SerializeField] private int _maxComments = 100;
        [SerializeField] private string _defaultUserIconPath = "";
        
        private CommentDataCollection _commentCollection;
        private List<CommentItem> _commentItems = new List<CommentItem>();

        private void Awake()
        {
            if (_commentItemPrefab == null)
            {
                Debug.LogError("CommentItemPrefab is not assigned!");
                return;
            }
            
            _commentCollection = CommentDataManager.LoadComments();
            
            _submitButton.onClick.AddListener(OnSubmitComment);
            _inputField.onSubmit.AddListener(_ => OnSubmitComment());
        }

        private void Start()
        {
            LoadAndDisplayComments();
        }

        private void LoadAndDisplayComments()
        {
            foreach (var commentData in _commentCollection.comments)
            {
                CreateCommentItem(commentData);
            }
            
            // 最新コメントまでスクロール
            ScrollToBottom();
        }

        private void OnSubmitComment()
        {
            string text = _inputField.text.Trim();
            
            if (string.IsNullOrEmpty(text))
            {
                Debug.Log("Comment text is empty.");
                return;
            }
            
            // 最大コメント数制限
            if (_commentCollection.comments.Count >= _maxComments)
            {
                // 古いコメントを削除
                _commentCollection.comments.RemoveAt(0);
                if (_commentItems.Count > 0)
                {
                    Destroy(_commentItems[0].gameObject);
                    _commentItems.RemoveAt(0);
                }
            }
            
            Title.Comment.Data.CommentData newComment = new Title.Comment.Data.CommentData(_defaultUserName, text, _defaultUserIconPath);
            _commentCollection.comments.Add(newComment);
            
            // UI生成
            CreateCommentItem(newComment);
            
            // 保存
            CommentDataManager.SaveComments(_commentCollection);
            
            // 入力欄クリア
            _inputField.text = "";
            _inputField.ActivateInputField();
            
            // 最新コメントまでスクロール
            ScrollToBottom();
        }

        private void CreateCommentItem(Title.Comment.Data.CommentData data)
        {
            GameObject itemObj = Instantiate(_commentItemPrefab, _commentContainer);
            CommentItem item = itemObj.GetComponent<CommentItem>();
            
            if (item != null)
            {
                item.Setup(data);
                _commentItems.Add(item);
            }
            else
            {
                Debug.LogError("CommentItem component not found on prefab!");
            }
        }

        private void ScrollToBottom()
        {
            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = 0f;
        }

        private void OnDestroy()
        {
            _submitButton.onClick.RemoveListener(OnSubmitComment);
        }

        [ContextMenu("Clear All Comments")]
        private void ClearAllComments()
        {
            foreach (var item in _commentItems)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _commentItems.Clear();
            _commentCollection.comments.Clear();
            
            CommentDataManager.SaveComments(_commentCollection);
            
            Debug.Log("All comments cleared.");
        }
    }
}