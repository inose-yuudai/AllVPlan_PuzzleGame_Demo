namespace EmoteOrchestra.UI
{
    /// <summary>
    /// ゲージの描画パターン
    /// </summary>
    public enum GaugePattern
    {
        Solid,              // 単色塗りつぶし
        Gradient,           // グラデーション
        Striped,            // ストライプ（縞模様）
        Dotted,             // ドット模様
        Checkered,          // チェック模様
        Wave,               // 波模様
        Hexagon,            // 六角形パターン
        Circuit,            // 回路風
        Digital,            // デジタル風
        Energy,             // エネルギー風（パルス）
        Neon,               // ネオン風
        Hologram,           // ホログラム風
        Glitch              // グリッチ風
    }
}