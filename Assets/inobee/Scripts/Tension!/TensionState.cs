using UnityEngine;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// テンションゲージの状態
    /// </summary>
    public enum TensionState
    {
        Critical,      // 危機的（萎えぽよ）
        Low,           // 低い
        Normal,        // 通常
        High,          // 高い
        Hyper          // ハイテンション
    }

    /// <summary>
    /// テンション状態の設定
    /// </summary>
    [System.Serializable]
    public class TensionStateConfig
    {
        public TensionState state;
        public string displayText;
        public Color color1;
        public Color color2;
        public float threshold; // この値以上でこの状態になる
    }
}