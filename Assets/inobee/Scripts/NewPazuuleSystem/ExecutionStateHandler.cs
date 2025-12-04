using UnityEngine;
using System.Collections;

namespace EmoteOrchestra.Puzzle
{
    /// <summary>
    /// 実行フェーズの処理（コンボ結果をGridControllerに返す）
    /// </summary>
    public class ExecutionStateHandler : IPuzzleStateHandler
    {
        private ComboExecutor _comboExecutor;
        private Coroutine _executionCoroutine;

        public ExecutionStateHandler(ComboExecutor comboExecutor)
        {
            _comboExecutor = comboExecutor;
        }

        public void OnEnter(GridController grid)
        {
            Debug.Log("[Execution] 実行フェーズ開始 - マッチ判定します");
            
            _executionCoroutine = grid.StartCoroutine(ExecuteCombo(grid));
        }

        public void OnUpdate(GridController grid)
        {
            // 実行中は特に何もしない
        }

        public void OnExit(GridController grid)
        {
            Debug.Log("[Execution] 実行フェーズ終了");
            
            if (_executionCoroutine != null)
            {
                grid.StopCoroutine(_executionCoroutine);
                _executionCoroutine = null;
            }
        }

        public bool CanSwap() => false;
        public bool CanExecute() => false;

        private IEnumerator ExecuteCombo(GridController grid)
        {
            // ★コンボ結果を収集
            ComboResult result = new ComboResult();
            
            // マッチ→落下→連鎖をすべて実行
            yield return _comboExecutor.ExecuteAllCombos(grid, result);
            
            // ★コンボ終了時にGridControllerに結果を通知
            grid.OnComboFinished(result);
            
            // 準備フェーズに戻る
            grid.ChangeState(PuzzleState.Preparation);
        }
    }
}