using UnityEngine;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// 実際に使われたデバイスに応じて表示を切り替える
    /// ・Padを触ったらすぐGamepad表示
    /// ・その後は「明確な」キーボード/マウス操作があるまでGamepad表示のまま
    /// </summary>
    public class ControlSchemeTextSwitcher : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private TextMeshProUGUI _targetText;

        [Header("Texts per Scheme")]
        [SerializeField] private string _keyboardMouseText = "Press E";
        [SerializeField] private string _gamepadText = "Press A";
        [SerializeField] private bool _hideIfEmpty = false;

#if ENABLE_INPUT_SYSTEM
        [Header("Input System (optional)")]
        [SerializeField] private PlayerInput _playerInput;

        private enum LastDevice
        {
            Unknown,
            KeyboardMouse,
            Gamepad
        }
        private LastDevice _lastDevice = LastDevice.Unknown;

        // マウスの微小な移動で切り替わらないようにするための前フレーム位置
        private Vector2 _lastMousePos;
        private bool _mousePosInitialized = false;
#endif

        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            if (_playerInput == null)
            {
                _playerInput = FindObjectOfType<PlayerInput>();
            }

            if (_playerInput != null)
            {
                _playerInput.onControlsChanged += OnControlsChanged;
            }
#endif
            // デフォルトはキーボードにしておく
            ApplyKeyboard();
        }

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            if (_playerInput != null)
            {
                _playerInput.onControlsChanged -= OnControlsChanged;
            }
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private void Update()
        {
            // 1) まずゲームパッドを最優先で見る
            bool usedGamepad = false;
            for (int i = 0; i < Gamepad.all.Count; i++)
            {
                var g = Gamepad.all[i];
                if (g != null && g.wasUpdatedThisFrame)
                {
                    usedGamepad = true;
                    break;
                }
            }

            if (usedGamepad && _lastDevice != LastDevice.Gamepad)
            {
                ApplyGamepad();
                return;
            }

            // 2) ゲームパッドを最後に使っているときは
            //    「はっきりしたキーボード/マウス操作」があるときだけ切り替える
            if (_lastDevice == LastDevice.Gamepad)
            {
                if (HasStrongKeyboardOrMouseInput())
                {
                    ApplyKeyboard();
                }
                return;
            }

            // 3) もともとキーボード表示のときは素直に切り替えでいい
            if (HasStrongKeyboardOrMouseInput() && _lastDevice != LastDevice.KeyboardMouse)
            {
                ApplyKeyboard();
            }
        }

        private void OnControlsChanged(PlayerInput playerInput)
        {
            // ControlSchemeをあとで足したとき用。今のプロジェクトだとここはあんまり動かないはず
            var scheme = playerInput.currentControlScheme;
            if (!string.IsNullOrEmpty(scheme) && scheme.ToLower().Contains("gamepad"))
            {
                ApplyGamepad();
            }
            else if (!string.IsNullOrEmpty(scheme))
            {
                ApplyKeyboard();
            }
        }

        /// <summary>
        /// 「はっきりとした」キーボード/マウス入力があったかどうか
        /// </summary>
        private bool HasStrongKeyboardOrMouseInput()
        {
            // キーボードのどれかが押された
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                return true;

            // マウスボタンが押された
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame ||
                    Mouse.current.rightButton.wasPressedThisFrame ||
                    Mouse.current.middleButton.wasPressedThisFrame)
                {
                    return true;
                }

                // スクロール
                if (Mouse.current.scroll.ReadValue().sqrMagnitude > 0.001f)
                    return true;

                // 大きめのマウス移動（ほんのちょっとの揺れは無視）
                const float moveThreshold = 4f;
                Vector2 currentPos = Mouse.current.position.ReadValue();
                if (_mousePosInitialized)
                {
                    if ((currentPos - _lastMousePos).sqrMagnitude > moveThreshold * moveThreshold)
                    {
                        _lastMousePos = currentPos;
                        return true;
                    }
                }
                _lastMousePos = currentPos;
                _mousePosInitialized = true;
            }

            return false;
        }
#endif

        private void ApplyKeyboard()
        {
            if (_targetText == null) return;

            _targetText.text = _keyboardMouseText;
            if (_hideIfEmpty) _targetText.gameObject.SetActive(!string.IsNullOrEmpty(_keyboardMouseText));

#if ENABLE_INPUT_SYSTEM
            _lastDevice = LastDevice.KeyboardMouse;
#endif
        }

        private void ApplyGamepad()
        {
            if (_targetText == null) return;

            _targetText.text = _gamepadText;
            if (_hideIfEmpty) _targetText.gameObject.SetActive(!string.IsNullOrEmpty(_gamepadText));

#if ENABLE_INPUT_SYSTEM
            _lastDevice = LastDevice.Gamepad;
#endif
        }
    }
}
