using UnityEngine;
using UnityEngine.InputSystem;
using EmoteOrchestra.Puzzle;
using EmoteOrchestra.Input;
using EmoteOrchestra.Audio;

namespace EmoteOrchestra.Core
{
    /// <summary>
    /// ゲーム入力管理（Input System + コントローラー対応）
    /// </summary>
    public class GameInputHandler : MonoBehaviour, GameInputActions.IGameplayActions
    {
        [Header("参照")]
        [SerializeField] private CursorController _cursorController;
        [SerializeField] private GridController _gridController;

        [Header("移動設定")]
        [SerializeField] private float _moveRepeatDelay = 0.2f;
        [SerializeField] private float _moveRepeatRate = 0.1f;

        private GameInputActions _inputActions;
        private Vector2 _moveInput;
        private float _moveTimer;
        private bool _isFirstMove = true;
        private Vector2Int _lastMoveDirection;

        private void Awake()
        {
            _inputActions = new GameInputActions();
            _inputActions.Gameplay.SetCallbacks(this);
        }

        private void OnEnable()
        {
            _inputActions.Gameplay.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Gameplay.Disable();
        }

        private void Update()
        {
            if (_moveInput != Vector2.zero)
            {
                ProcessContinuousMove();
            }
        }

        #region Input System コールバック

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();

            if (context.performed && _moveInput != Vector2.zero)
            {
                _cursorController?.SetVisible(true);
                _isFirstMove = true;
                _moveTimer = 0f;
                ProcessMove();
            }
            else if (context.canceled)
            {
                _moveInput = Vector2.zero;
                _isFirstMove = true;
            }
        }

        public void OnSelect(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            if (_cursorController == null)
                return;

            _cursorController.SetVisible(true);

            if (!_cursorController.IsSelecting)
            {
                _cursorController.StartSelection();
                EmoteOrchestra.Audio.AudioManager.Instance?.PlaySelect();
                Debug.Log("選択開始");
            }
            else
            {
                // 仕様変更: 2回目のスペースはキャンセルとして扱う
                _cursorController.CancelSelection();
                Debug.Log("選択キャンセル(スペース)");
            }
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            if (_cursorController != null && _cursorController.IsSelecting)
            {
                _cursorController.CancelSelection();
                Debug.Log("選択キャンセル");
            }
        }

        /// <summary>
        /// ★実行ボタン（Enter / E / Xボタン）
        /// </summary>
        public void OnExecute(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            Debug.Log("★実行ボタンが押されました！");

            if (_gridController != null)
            {
                _gridController.OnExecuteButtonPressed();
            }
            else
            {
                Debug.LogError("GridControllerが設定されていません！");
            }
        }

        public void OnHint(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            Debug.Log("ヒント機能（未実装）");
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            Debug.Log("ポーズ機能（未実装）");
        }

        public void OnSlowMotion(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Debug.Log("スローモーション開始");
            }
            else if (context.canceled)
            {
                Debug.Log("スローモーション終了");
            }
        }

        public void OnCheer(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            Debug.Log("応援機能（未実装）");
        }

        public void OnRaise(InputAction.CallbackContext context)
        {
            // せり上がり加速（未実装）
        }

        #endregion

        #region 移動処理

        private void ProcessMove()
        {
            if (_cursorController == null)
                return;

            Vector2Int direction = GetDirectionFromInput(_moveInput);

            if (direction == Vector2Int.zero)
                return;

            bool moved = _cursorController.Move(direction);

            if (moved)
            {
                _lastMoveDirection = direction;

                // 仕様変更: 選択中に移動したら即スワップ
                if (_cursorController.IsSelecting)
                {
                    Vector2Int startPos = _cursorController.SelectionStartPosition;
                    Vector2Int currentPos = _cursorController.CurrentPosition;

                    bool isAdjacent =
                        (Mathf.Abs(startPos.x - currentPos.x) == 1 && startPos.y == currentPos.y) ||
                        (Mathf.Abs(startPos.y - currentPos.y) == 1 && startPos.x == currentPos.x);

                    if (isAdjacent)
                    {
                        ExecuteSwap();
                        _cursorController.FinishSelection();
                        Debug.Log("移動により即時スワップ実行");
                    }
                }
            }
        }

        private void ProcessContinuousMove()
        {
            if (_isFirstMove)
            {
                _isFirstMove = false;
                return;
            }

            _moveTimer += Time.deltaTime;

            float threshold = _moveTimer < _moveRepeatDelay ? _moveRepeatDelay : _moveRepeatRate;

            if (_moveTimer >= threshold)
            {
                _moveTimer = 0f;
                ProcessMove();
            }
        }

        private Vector2Int GetDirectionFromInput(Vector2 input)
        {
            if (input.magnitude < 0.3f)
                return Vector2Int.zero;

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                return input.y > 0 ? Vector2Int.up : Vector2Int.down;
            }
        }

        #endregion

        #region スワップ処理

        private void ExecuteSwap()
        {
            if (_cursorController == null || _gridController == null)
                return;

            Vector2Int startPos = _cursorController.SelectionStartPosition;
            Vector2Int currentPos = _cursorController.CurrentPosition;

            bool swapped = false;

            if (Mathf.Abs(startPos.x - currentPos.x) == 1 && startPos.y == currentPos.y)
            {
                swapped = _gridController.TrySwap(startPos, currentPos);
            }
            else if (startPos.x == currentPos.x && Mathf.Abs(startPos.y - currentPos.y) == 1)
            {
                swapped = _gridController.TrySwap(startPos, currentPos);
            }
            else if (_lastMoveDirection != Vector2Int.zero)
            {
                Vector2Int targetPos = startPos + _lastMoveDirection;
                swapped = _gridController.TrySwap(startPos, targetPos);
            }
            else if (_moveInput != Vector2.zero)
            {
                Vector2Int direction = GetDirectionFromInput(_moveInput);
                if (direction != Vector2Int.zero)
                {
                    Vector2Int targetPos = startPos + direction;
                    swapped = _gridController.TrySwap(startPos, targetPos);
                }
            }

            if (swapped)
            {
                EmoteOrchestra.Audio.AudioManager.Instance?.PlaySwap();
            }
        }

        #endregion

        private void OnDestroy()
        {
            _inputActions?.Dispose();
        }
    }
}
