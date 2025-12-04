using System.Collections.Generic;
using UnityEngine;

namespace EmoteOrchestra.Puzzle
{
    /// <summary>
    /// マッチ検出（せり上がり中のエモートを除外）
    /// </summary>
    public class MatchDetector
    {
        private int _width;
        private int _height;

        public MatchDetector(int width, int height)
        {
            _width = width;
            _height = height;
        }

        /// <summary>
        /// 全マッチを検出（横・縦両方）
        /// </summary>
        public List<Vector2Int> FindMatches(GridCell[,] grid)
        {
            HashSet<Vector2Int> matchedCells = new HashSet<Vector2Int>();

            // 横方向のマッチ検出
            FindHorizontalMatches(grid, matchedCells);

            // 縦方向のマッチ検出
            FindVerticalMatches(grid, matchedCells);

            return new List<Vector2Int>(matchedCells);
        }

        /// <summary>
        /// 横方向のマッチ検出（せり上がり中を除外）
        /// </summary>
        private void FindHorizontalMatches(GridCell[,] grid, HashSet<Vector2Int> matchedCells)
        {
            for (int y = 0; y < _height; y++)
            {
                int x = 0;
                while (x < _width)
                {
                    EmoteController current = grid[x, y].Emote;

                    // エモートがない、またはせり上がり中の場合はスキップ
                    if (current == null || current.IsRising)
                    {
                        x++;
                        continue;
                    }

                    // 連続した同じエモートをカウント
                    int matchLength = 1;
                    int startX = x;

                    for (int checkX = x + 1; checkX < _width; checkX++)
                    {
                        EmoteController next = grid[checkX, y].Emote;

                        // null、せり上がり中、または違うエモートならストップ
                        if (next == null || next.IsRising || next.Data != current.Data)
                            break;

                        matchLength++;
                    }

                    // 3つ以上なら登録
                    if (matchLength >= 3)
                    {
                        for (int i = 0; i < matchLength; i++)
                        {
                            matchedCells.Add(new Vector2Int(startX + i, y));
                        }
                    }

                    // 次の開始位置に移動
                    x += matchLength;
                }
            }
        }

        /// <summary>
        /// 縦方向のマッチ検出（せり上がり中を除外）
        /// </summary>
        private void FindVerticalMatches(GridCell[,] grid, HashSet<Vector2Int> matchedCells)
        {
            for (int x = 0; x < _width; x++)
            {
                int y = 0;
                while (y < _height)
                {
                    EmoteController current = grid[x, y].Emote;

                    // エモートがない、またはせり上がり中の場合はスキップ
                    if (current == null || current.IsRising)
                    {
                        y++;
                        continue;
                    }

                    // 連続した同じエモートをカウント
                    int matchLength = 1;
                    int startY = y;

                    for (int checkY = y + 1; checkY < _height; checkY++)
                    {
                        EmoteController next = grid[x, checkY].Emote;

                        // null、せり上がり中、または違うエモートならストップ
                        if (next == null || next.IsRising || next.Data != current.Data)
                            break;

                        matchLength++;
                    }

                    // 3つ以上なら登録
                    if (matchLength >= 3)
                    {
                        for (int i = 0; i < matchLength; i++)
                        {
                            matchedCells.Add(new Vector2Int(x, startY + i));
                        }
                    }

                    // 次の開始位置に移動
                    y += matchLength;
                }
            }
        }
    }
}