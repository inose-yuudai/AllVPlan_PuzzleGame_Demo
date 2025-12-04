using UnityEngine;
using DG.Tweening;

namespace EmoteOrchestra.Puzzle
{
    /// <summary>
    /// パズルカーソルの制御（空欄選択可能版）
    /// </summary>
    public class CursorController : MonoBehaviour
    {
        [SerializeField] private RectTransform _cursorVisual;
        [SerializeField] private GridController _gridController;
        [SerializeField] private float _moveSpeed = 0.1f;

        private Vector2Int _currentGridPos;
        private Vector2Int _selectionStartPos;
        private bool _isSelecting;

        public Vector2Int CurrentPosition => _currentGridPos;
        public Vector2Int SelectionStartPosition => _selectionStartPos;
        public bool IsSelecting => _isSelecting;
        public bool IsVisible => _cursorVisual != null && _cursorVisual.gameObject.activeSelf;

        private void Start()
        {
            _currentGridPos = new Vector2Int(4, 3);
            UpdateVisualPosition();
        }

        private void Update()
        {
            UpdateVisualPositionContinuous();
        }

        /// <summary>
        /// カーソルを移動（空欄も移動可能）
        /// </summary>
        public bool Move(Vector2Int direction)
        {
            Vector2Int newPos = _currentGridPos + direction;

            // グリッド範囲内チェックのみ
            if (!_gridController.IsValidPosition(newPos))
                return false;

            _currentGridPos = newPos;
            UpdateVisualPosition();
            return true;
        }

        private void UpdateVisualPosition()
        {
            Vector2 anchoredPos = _gridController.GetAnchoredPosition(_currentGridPos.x, _currentGridPos.y);
            anchoredPos += Vector2.up * _gridController.CurrentRiseOffset;
            
            _cursorVisual.DOAnchorPos(anchoredPos, _moveSpeed).SetEase(Ease.OutQuad);

            float scale = _isSelecting ? 1.3f : 1.0f;
            _cursorVisual.DOScale(scale, 0.1f);
        }

        private void UpdateVisualPositionContinuous()
        {
            Vector2 anchoredPos = _gridController.GetAnchoredPosition(_currentGridPos.x, _currentGridPos.y);
            anchoredPos += Vector2.up * _gridController.CurrentRiseOffset;
            
            _cursorVisual.anchoredPosition = anchoredPos;
        }

        public void StartSelection()
        {
            _isSelecting = true;
            _selectionStartPos = _currentGridPos;
            UpdateVisualPosition();
        }

        public void FinishSelection()
        {
            _isSelecting = false;
            UpdateVisualPosition();
        }

        public void CancelSelection()
        {
            _isSelecting = false;
            _currentGridPos = _selectionStartPos;
            UpdateVisualPosition();
        }

        public void SetVisible(bool visible)
        {
            if (_cursorVisual == null) return;
            if (_cursorVisual.gameObject.activeSelf == visible) return;
            _cursorVisual.gameObject.SetActive(visible);
        }
    }
}