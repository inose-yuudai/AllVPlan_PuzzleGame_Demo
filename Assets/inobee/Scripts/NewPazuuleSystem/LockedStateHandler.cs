using UnityEngine;

namespace EmoteOrchestra.Puzzle
{
    /// <summary>
    /// ロック状態（何もできない）
    /// </summary>
    public class LockedStateHandler : IPuzzleStateHandler
    {
        public void OnEnter(GridController grid)
        {
            Debug.Log("[Locked] ロック状態に入りました");
        }

        public void OnUpdate(GridController grid)
        {
            // 何もしない
        }

        public void OnExit(GridController grid)
        {
            Debug.Log("[Locked] ロック状態を解除しました");
        }

        public bool CanSwap() => false;
        public bool CanExecute() => false;
    }
}