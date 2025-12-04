using UnityEngine;

namespace EmoteOrchestra.Vtuber
{
    /// <summary>
    /// Vtuberの表情制御
    /// </summary>
    public class ExpressionController : MonoBehaviour
    {
        // Live2D Cubism SDKとの統合用
        // 実際のLive2Dモデルがあれば、CubismExpressionControllerを使用

        public void ShowHappyExpression()
        {
            // Live2Dの表情パラメータを変更
            Debug.Log("表情: 嬉しい");
        }

        public void ShowExcitedExpression()
        {
            Debug.Log("表情: 興奮");
        }

        public void ShowMaxExcitedExpression()
        {
            Debug.Log("表情: 最高潮");
        }

        public void ShowNeutralExpression()
        {
            Debug.Log("表情: 通常");
        }

        public void ShowSurprisedExpression()
        {
            Debug.Log("表情: 驚き");
        }
    }
}