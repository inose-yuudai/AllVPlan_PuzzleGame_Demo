using EmoteOrchestra.Data;

namespace EmoteOrchestra.Puzzle
{
    /// <summary>
    /// チェイン情報（1回のマッチで揃った情報）
    /// </summary>
    public class ChainData
    {
        public EmoteData EmoteData { get; private set; }
        public int ChainCount { get; private set; } // 揃った個数（3, 4, 5, 6...）

        public ChainData(EmoteData emoteData, int chainCount)
        {
            EmoteData = emoteData;
            ChainCount = chainCount;
        }
    }
}