using UnityEngine;

[DisallowMultipleComponent]
public class CursorHideOnNonMouseInput : MonoBehaviour
{
    [Header("Behavior")]
    [SerializeField] private bool _hideOnKeyboard = true;
    [SerializeField] private bool _hideOnGamepad = true;
    [Tooltip("マウス移動またはクリックでカーソルを再表示する")]
    [SerializeField] private bool _showOnMouseMove = false;

    [Header("Detection")]
    [Tooltip("アナログ軸のデッドゾーン")]
    [SerializeField] private float _axisDeadzone = 0.2f;
    [Tooltip("Mouse X/Y の再表示しきい値")]
    [SerializeField] private float _mouseMoveThreshold = 0.01f;

    [Header("Mouse Active Visibility")]
    [Tooltip("マウス操作中（移動/クリック/ドラッグ）および一定猶予時間は絶対に隠さない")]
    [SerializeField] private bool _neverHideWhileMouseActive = true;
    [Tooltip("最後のマウス操作からカーソル非表示を許可するまでの猶予秒")]
    [SerializeField] private float _mouseActiveGraceSeconds = 1.2f;
    [Tooltip("このキーは押してもカーソル非表示のトリガーにしない（例：Eキー）")]
    [SerializeField] private bool _ignoreEKeyForHide = true;

    // よくある既定の軸名（レガシーInput）
    private static readonly string[] k_NonMouseAxes = new[]
    {
        "Horizontal", "Vertical", "Submit", "Cancel", "Fire1", "Fire2", "Fire3", "Jump"
    };

    private bool _cursorHidden;
    private float _mouseActiveUntil;

    private void OnEnable()
    {
        // 起動直後は現状維持
        _cursorHidden = !Cursor.visible;
        _mouseActiveUntil = 0f;
    }

    private void Update()
    {
        // マウス操作の検出（移動 or クリック or ドラッグ中）
        bool mouseMoved = Mathf.Abs(Input.GetAxisRaw("Mouse X")) > _mouseMoveThreshold
                       || Mathf.Abs(Input.GetAxisRaw("Mouse Y")) > _mouseMoveThreshold;
        bool mouseBtnDown = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);
        bool mouseBtnHeld = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);

        if (mouseMoved || mouseBtnDown || mouseBtnHeld)
        {
            _mouseActiveUntil = Time.unscaledTime + Mathf.Max(0f, _mouseActiveGraceSeconds);
            // マウス操作検出時は即表示を保証
            ShowCursor();
        }

        // マウス操作で再表示（任意）
        if (_showOnMouseMove)
        {
            if (mouseMoved || mouseBtnDown)
            {
                ShowCursor();
            }
        }

        // マウス利用中は非表示にしない
        if (_neverHideWhileMouseActive)
        {
            bool mouseActive = mouseBtnHeld || (Time.unscaledTime < _mouseActiveUntil);
            if (mouseActive)
            {
                ShowCursor();
                return;
            }
        }

        // キー入力で非表示
        if (_hideOnKeyboard && IsKeyboardPressedThisFrame())
        {
            HideCursor();
            return;
        }

        // ゲームパッド入力（ボタン/軸）で非表示
        if (_hideOnGamepad && (IsAnyJoystickButtonDown() || IsAnyNonMouseAxisMoved()))
        {
            HideCursor();
        }
    }

    private bool IsKeyboardPressedThisFrame()
    {
        // anyKeyDown はマウスボタンでもtrueになるので除外
        if (!Input.anyKeyDown) return false;
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            return false;
        }
        if (_ignoreEKeyForHide && Input.GetKeyDown(KeyCode.E))
        {
            return false;
        }
        return true;
    }

    private bool IsAnyNonMouseAxisMoved()
    {
        float dz = Mathf.Max(0f, _axisDeadzone);
        for (int i = 0; i < k_NonMouseAxes.Length; i++)
        {
            string axis = k_NonMouseAxes[i];
            float v = 0f;
            try
            {
                v = Input.GetAxisRaw(axis);
            }
            catch
            {
                // 軸未定義は無視
                continue;
            }
            if (Mathf.Abs(v) > dz) return true;
        }
        return false;
    }

    private bool IsAnyJoystickButtonDown()
    {
        // 一般的な範囲をスキャン（0〜19）
        for (int b = 0; b <= 19; b++)
        {
            if (Input.GetKeyDown(KeyCode.JoystickButton0 + b))
            {
                return true;
            }
        }
        return false;
    }

    private void HideCursor()
    {
        if (_cursorHidden) return;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None; // パズル操作のためロックはしない
        _cursorHidden = true;
    }

    private void ShowCursor()
    {
        if (!_cursorHidden) return;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _cursorHidden = false;
    }
}


