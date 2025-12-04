namespace EmoteOrchestra.Puzzle
{
    /// <summary>
    /// グリッドの各セル情報
    /// </summary>
    public class GridCell
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public EmoteController Emote { get; set; }

        public GridCell(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}