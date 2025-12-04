using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EmoteOrchestra.Puzzle
{
    /// <summary>
    /// マウスのドラッグ＆ドロップで隣接スワップを実行
    /// - ドラッグ中とドラッグ直後はカーソルを非表示
    /// - 発火（Eキー）は従来通り GameInputHandler 側で処理
    /// </summary>
    public class MouseDragSwapHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private GridController _gridController;
        [SerializeField] private CursorController _cursorController;
        [SerializeField] private Camera _uiCamera; // Canvas が ScreenSpace-Camera の場合に指定
        [SerializeField] private Image _raycastCatcher; // 透明なキャッチャー（自動付与）

        [Header("Directional Drag Swap Settings")]
        [Tooltip("ドラッグ方向だけで隣にスワップを許可（4方向）。リリース位置が隣マス上でなくてもOKにする")]
        [SerializeField] private bool _allowDirectionalSwap = true;
        [Tooltip("スワップ判定に必要なドラッグ距離（ピクセル）。小さすぎるドラッグは無視")]
        [SerializeField] private float _dragSwapThresholdPx = 30f;

        private bool _dragging;
        private Vector2Int _startGrid;
        private Vector2Int _currentGrid;
        private Vector2 _pointerDownScreenPos;

        private void Awake()
        {
            // Raycast を受け取れる UI を確保（既存の Image があればそれを使い、色は変更しない）
            if (_raycastCatcher == null)
            {
                _raycastCatcher = GetComponent<Image>();
            }

            if (_raycastCatcher != null)
            {
                // 親の既存 Image を使う。見た目は変更しない。
                if (_raycastCatcher.raycastTarget == false)
                {
                    _raycastCatcher.raycastTarget = true;
                }
            }
            else
            {
                // Graphic が無い場合のみ、子に透明キャッチャーを作る（親の見た目に影響を与えない）
                var catcherGO = new GameObject("RaycastCatcher", typeof(RectTransform), typeof(Image));
                catcherGO.transform.SetParent(transform, false);
                var rt = (RectTransform)catcherGO.transform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                var img = catcherGO.GetComponent<Image>();
                img.color = new Color(0f, 0f, 0f, 0f);
                img.raycastTarget = true;
                _raycastCatcher = img;
            }

            // UI カメラの自動設定
            if (_uiCamera == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    _uiCamera = canvas.worldCamera;
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_gridController == null) return;

            // 左クリックのみドラッグを開始
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            Vector2Int cell;
            var cam = eventData.pressEventCamera != null ? eventData.pressEventCamera : _uiCamera;
            if (_gridController.TryGetGridFromScreenPoint(eventData.position, cam, out cell))
            {
                _startGrid = cell;
                _currentGrid = cell;
                _dragging = true;
                _cursorController?.SetVisible(false);
                _pointerDownScreenPos = eventData.position;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging || _gridController == null) return;

            Vector2Int cell;
            var cam = eventData.pressEventCamera != null ? eventData.pressEventCamera : _uiCamera;
            if (_gridController.TryGetGridFromScreenPoint(eventData.position, cam, out cell))
            {
                _currentGrid = cell;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!_dragging || _gridController == null)
            {
                _dragging = false;
                return;
            }

            _dragging = false;
            _cursorController?.SetVisible(false); // ドラッグ後もしばらく非表示維持

            // 隣接ならスワップを試行
            if (IsAdjacent(_startGrid, _currentGrid))
            {
                _gridController.TrySwap(_startGrid, _currentGrid);
                EmoteOrchestra.Audio.AudioManager.Instance?.PlaySwap();
                return;
            }

            // 方向ドラッグによるスワップ（リリース位置が隣マスでなくてもOK）
            if (_allowDirectionalSwap)
            {
                Vector2 delta = (Vector2)eventData.position - _pointerDownScreenPos;
                if (delta.sqrMagnitude >= (_dragSwapThresholdPx * _dragSwapThresholdPx))
                {
                    Vector2Int dir = GetCardinalDirection(delta);
                    if (dir != Vector2Int.zero)
                    {
                        Vector2Int target = _startGrid + dir;
                        if (_gridController.IsValidPosition(target))
                        {
                            if (_gridController.TrySwap(_startGrid, target))
                            {
                                EmoteOrchestra.Audio.AudioManager.Instance?.PlaySwap();
                            }
                        }
                    }
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_gridController == null) return;

            // 右クリックで発火（Eキー相当）
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                _gridController.OnExecuteButtonPressed();
            }
        }

        private bool IsAdjacent(Vector2Int a, Vector2Int b)
        {
            return (Mathf.Abs(a.x - b.x) == 1 && a.y == b.y) ||
                   (Mathf.Abs(a.y - b.y) == 1 && a.x == b.x);
        }

        private Vector2Int GetCardinalDirection(Vector2 delta)
        {
            // 4方向のみ。より大きい軸に沿って決定
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else if (Mathf.Abs(delta.y) > 0f)
            {
                return delta.y > 0 ? Vector2Int.up : Vector2Int.down;
            }
            return Vector2Int.zero;
        }
    }
}


