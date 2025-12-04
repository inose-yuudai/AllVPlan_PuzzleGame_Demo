using UnityEngine;

namespace EmoteOrchestra.Puzzle
{
    /// <summary>
    /// パズル状態ごとの処理を定義するインターフェース（Strategy Pattern）
    /// </summary>
    public interface IPuzzleStateHandler
    {
        /// <summary>
        /// この状態に入った時の処理
        /// </summary>
        void OnEnter(GridController grid);
        
        /// <summary>
        /// この状態での更新処理
        /// </summary>
        void OnUpdate(GridController grid);
        
        /// <summary>
        /// この状態から出る時の処理
        /// </summary>
        void OnExit(GridController grid);
        
        /// <summary>
        /// この状態でスワップ可能か
        /// </summary>
        bool CanSwap();
        
        /// <summary>
        /// この状態で実行ボタンを押せるか
        /// </summary>
        bool CanExecute();
    }
}