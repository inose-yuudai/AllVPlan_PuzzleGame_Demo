using System.Collections;
using UnityEngine;

/// <summary>
/// フレームアニメーションでマウスカーソルを制御するクラス
/// </summary>
public class AnimatedCursorController : MonoBehaviour
{
    // アニメーションさせるカーソルのフレーム画像群
    [SerializeField]
    private Texture2D[] _cursorFrames;

    // クリック位置を調整するためのオフセット
    [SerializeField]
    private Vector2 _hotspot = Vector2.zero;

    // アニメーションのフレームレート（1秒間に何回切り替えるか）
    [SerializeField]
    private float _frameRate = 12.0f;

    private Coroutine _animationCoroutine;

    private void OnEnable()
    {
        if (_cursorFrames != null && _cursorFrames.Length > 0)
        {
            // アニメーション再生用のコルーチンを開始
            _animationCoroutine = StartCoroutine(AnimateCursor());
        }
    }

    private void OnDisable()
    {
        // このコンポーネントが無効になったらコルーチンを停止し、カーソルを元に戻す
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    /// <summary>
    /// カーソルをアニメーションさせるコルーチン
    /// </summary>
    private IEnumerator AnimateCursor()
    {
        int currentFrame = 0;
        while (true) // 無限ループ
        {
            // 配列の範囲を超えないように剰余演算子(%)を使用
            currentFrame %= _cursorFrames.Length;

            // 現在のフレームのテクスチャをカーソルに設定
            Cursor.SetCursor(_cursorFrames[currentFrame], _hotspot, CursorMode.Auto);

            // 次のフレームに更新
            currentFrame++;

            // 指定されたフレームレートに応じた時間だけ待機
            yield return new WaitForSeconds(1.0f / _frameRate);
        }
    }
}
