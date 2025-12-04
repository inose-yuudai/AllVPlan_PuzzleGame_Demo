namespace EmoteOrchestra.Mission
{
    /// <summary>
    /// ミッションの種類
    /// </summary>
    public enum MissionType
    {
        Chain,          // チェイン数達成（例：5個以上同時消し）
        Combo,          // コンボ数達成（例：3連鎖以上）
        TotalMatches,   // 合計マッチ数（例：30個消す）
        SpecificEmote,  // 特定エモートを消す（例：かわいいを10個消す）
        TimeLimit,      // 制限時間内に達成（例：30秒以内に50個消す）
        AnyChain,       // ★新規：なんでもいいから指定数以上消し（例：なんでもいいから5個消し）
        AllClear        // ★新規：全消し（グリッド上のエモートを全部消す）
    }
}