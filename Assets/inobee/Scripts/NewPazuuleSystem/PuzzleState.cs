namespace EmoteOrchestra.Puzzle
{
    /// <summary>
    /// パズルの状態
    /// </summary>
    public enum PuzzleState
    {
        Preparation,  // 準備フェーズ（自由に入れ替え可能）
        Execution,    // 実行フェーズ（マッチ判定→連鎖）
        Locked        // ロック状態（アニメーション中など）
    }
}