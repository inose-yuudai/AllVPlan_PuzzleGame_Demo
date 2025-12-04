using UnityEngine;
using UnityEngine.InputSystem;
using EmoteOrchestra.Audio;

namespace EmoteOrchestra.Puzzle
{
    /// <summary>
    /// 入力制御（PC + コントローラー対応）
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class InputController : MonoBehaviour
    {
        [SerializeField] private CursorController _cursorController;
        [SerializeField] private GridController _gridController;

        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _selectAction;
        private InputAction _cancelAction;
        private InputAction _executeAction; // ★実行ボタン

        private Vector2Int? _selectedPosition;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            
            _moveAction = _playerInput.actions["Move"];
            _selectAction = _playerInput.actions["Select"];
            _cancelAction = _playerInput.actions["Cancel"];
            _executeAction = _playerInput.actions["Execute"]; // ★追加
        }

        private void OnEnable()
        {
            _selectAction.performed += OnSelectPerformed;
            _cancelAction.performed += OnCancelPerformed;
            _executeAction.performed += OnExecutePerformed; // ★追加
        }

        private void OnDisable()
        {
            _selectAction.performed -= OnSelectPerformed;
            _cancelAction.performed -= OnCancelPerformed;
            _executeAction.performed -= OnExecutePerformed; // ★追加
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            Vector2 input = _moveAction.ReadValue<Vector2>();

            // デジタル入力に変換（0.5以上で反応）
            int horizontal = Mathf.Abs(input.x) > 0.5f ? (int)Mathf.Sign(input.x) : 0;
            int vertical = Mathf.Abs(input.y) > 0.5f ? (int)Mathf.Sign(input.y) : 0;

            if (horizontal != 0 || vertical != 0)
            {
                Vector2Int direction = new Vector2Int(horizontal, vertical);
                bool moved = _cursorController.Move(direction);

                // 仕様変更: 選択中に移動したら即スワップ
                if (moved && _selectedPosition != null)
                {
                    Vector2Int start = _selectedPosition.Value;
                    Vector2Int end = _cursorController.CurrentPosition;

                    if (IsAdjacent(start, end))
                    {
                        Debug.Log($"スワップ(移動で即時): {start} ↔ {end}");
                        _gridController.SwapEmotes(start.x, start.y, end.x, end.y);
                        _selectedPosition = null;
                        _cursorController.FinishSelection();
                        AudioManager.Instance?.PlaySwap();
                    }
                }
            }
        }

        private void OnSelectPerformed(InputAction.CallbackContext context)
        {
            Vector2Int currentPos = _cursorController.CurrentPosition;

            if (_selectedPosition == null)
            {
                // 1回目の選択
                _selectedPosition = currentPos;
                _cursorController.StartSelection();
                AudioManager.Instance?.PlaySelect();
                Debug.Log($"選択開始: {currentPos}");
            }
            else
            {
                // 仕様変更: 2回目のスペースはキャンセル
                Debug.Log("選択キャンセル(スペース)");
                _selectedPosition = null;
                _cursorController.CancelSelection();
            }
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            if (_selectedPosition != null)
            {
                Debug.Log("選択キャンセル");
                _selectedPosition = null;
                _cursorController.CancelSelection();
            }
        }

        /// <summary>
        /// ★実行ボタンが押された
        /// </summary>
        private void OnExecutePerformed(InputAction.CallbackContext context)
        {
            Debug.Log("実行ボタン押下！");
            _gridController.OnExecuteButtonPressed();
        }

        private bool IsAdjacent(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }
    }
}