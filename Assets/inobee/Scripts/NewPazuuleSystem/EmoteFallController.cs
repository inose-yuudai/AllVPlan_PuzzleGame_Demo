using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmoteOrchestra.Puzzle
{
    /// <summary>
    /// エモートの落下処理
    /// </summary>
    public class EmoteFallController
    {
        private const float k_MinFallDuration = 0.1f;
        private const float k_MaxFallDuration = 0.6f;
        private const float k_PerCellFallTime = 0.08f;

        /// <summary>
        /// 重力を適用（空いたマスに上から落とす）
        /// </summary>
        public IEnumerator ApplyGravity(GridController grid)
        {
            GridCell[,] gridData = grid.GetGrid();
            int width = grid.Width;
            int height = grid.Height;

            float maxDuration = 0f;

            // 列ごとに圧縮して一気に下まで落とす
            for (int x = 0; x < width; x++)
            {
                int nextFillY = 0;
                for (int y = 0; y < height; y++)
                {
                    EmoteController emote = gridData[x, y].Emote;
                    if (emote == null) continue;
                    if (emote.IsRising) { nextFillY++; continue; }

                    if (y != nextFillY)
                    {
                        gridData[x, nextFillY].Emote = emote;
                        gridData[x, y].Emote = null;
                        emote.SetGridPosition(x, nextFillY);

                        Vector2 targetPos = grid.GetAnchoredPosition(x, nextFillY);
                        int cells = y - nextFillY;
                        float duration = Mathf.Clamp(k_PerCellFallTime * Mathf.Max(1, cells), k_MinFallDuration, k_MaxFallDuration);
                        emote.FallTo(targetPos, duration);
                        if (duration > maxDuration) maxDuration = duration;
                    }
                    nextFillY++;
                }
            }

            if (maxDuration > 0f)
            {
                yield return new WaitForSeconds(maxDuration);
            }

            // 落下完了後、空いているマスに新しいエモートを生成（上から落とす）
            yield return grid.FillEmptySpaces();
        }
    }
}