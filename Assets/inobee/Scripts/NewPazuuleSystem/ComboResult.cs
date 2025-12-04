using System.Collections.Generic;

namespace EmoteOrchestra.Puzzle
{
    /// <summary>
    /// コンボ実行の結果
    /// </summary>
    public class ComboResult
    {
        public int TotalComboCount { get; set; } // 合計コンボ数（連鎖数）
        public List<ChainData> Chains { get; set; } // 各チェインの情報

        public ComboResult()
        {
            Chains = new List<ChainData>();
        }
    }
}