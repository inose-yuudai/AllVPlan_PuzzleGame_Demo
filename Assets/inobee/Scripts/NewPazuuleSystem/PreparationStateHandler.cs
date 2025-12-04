using UnityEngine;

namespace EmoteOrchestra.Puzzle
{
    /// <summary>
    /// 準備フェーズの処理
    /// </summary>
    public class PreparationStateHandler : IPuzzleStateHandler
    {
        public void OnEnter(GridController grid)
        {
            Debug.Log("[Preparation] 準備フェーズ開始 - 自由に入れ替えできます");
        }

        public void OnUpdate(GridController grid)
        {
            // 準備フェーズでは特に何もしない
        }

        public void OnExit(GridController grid)
        {
            Debug.Log("[Preparation] 準備フェーズ終了");
        }

        public bool CanSwap() => true;  // スワップ可能
        public bool CanExecute() => true; // 実行ボタンを押せる
    }
}