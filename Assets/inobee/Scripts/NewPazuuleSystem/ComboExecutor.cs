using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EmoteOrchestra.Data;
using EmoteOrchestra.Audio;

namespace EmoteOrchestra.Puzzle
{
    /// <summary>
    /// コンボの実行処理（チェイン情報収集機能付き）
    /// </summary>
    public class ComboExecutor
    {
        private MatchDetector _matchDetector;
        private EmoteFallController _fallController;
        
        private const float k_MatchDelay = 0.4f;
        private const float k_FallDelay = 0.2f;

        public ComboExecutor(MatchDetector matchDetector, EmoteFallController fallController)
        {
            _matchDetector = matchDetector;
            _fallController = fallController;
        }

        /// <summary>
        /// すべてのコンボを実行（連鎖含む）＋ チェイン情報を収集
        /// </summary>
        public IEnumerator ExecuteAllCombos(GridController grid, ComboResult result)
        {
            int comboCount = 0;
            bool hasMatches = true;

            while (hasMatches)
            {
                // マッチ検出
                List<Vector2Int> matches = _matchDetector.FindMatches(grid.GetGrid());

                if (matches.Count == 0)
                {
                    hasMatches = false;
                    break;
                }

                comboCount++;
                Debug.Log($"[Combo] {comboCount}連鎖目 - {matches.Count}個マッチ");

                // ★チェイン情報を収集
                CollectChainData(grid, matches, result);

                // マッチしたエモートをハイライト（このタイミングでコンボSEを1回だけ再生）
                yield return HighlightMatches(grid, matches, comboCount);

                // マッチしたエモートを消去
                yield return ClearMatches(grid, matches);

                // 落下処理
                yield return _fallController.ApplyGravity(grid);

                // 少し待つ
                yield return new WaitForSeconds(k_FallDelay);
            }

            result.TotalComboCount = comboCount;
            Debug.Log($"[Combo] 完了 - 合計{comboCount}連鎖、{result.Chains.Count}種類のチェイン");
        }

        /// <summary>
        /// チェイン情報を収集（どのエモートが何個揃ったか）
        /// </summary>
        private void CollectChainData(GridController grid, List<Vector2Int> matches, ComboResult result)
        {
            // エモートごとにグループ化
            Dictionary<EmoteData, int> emoteCountMap = new Dictionary<EmoteData, int>();

            foreach (Vector2Int pos in matches)
            {
                EmoteController emote = grid.GetEmoteAt(pos.x, pos.y);
                if (emote != null && emote.Data != null)
                {
                    if (!emoteCountMap.ContainsKey(emote.Data))
                    {
                        emoteCountMap[emote.Data] = 0;
                    }
                    emoteCountMap[emote.Data]++;
                }
            }

            // ChainDataとして記録
            foreach (var pair in emoteCountMap)
            {
                ChainData chain = new ChainData(pair.Key, pair.Value);
                result.Chains.Add(chain);
                
                Debug.Log($"[Chain] {pair.Key.emoteName} x {pair.Value}個");
            }
        }

        private IEnumerator HighlightMatches(GridController grid, List<Vector2Int> matches, int comboIndex)
        {
            // Draw group lines over matched sets
            List<List<Vector2Int>> groups = FindMatchGroups(grid);
            grid.ShowMatchLines(groups);

            // コンボ段階に応じてピッチが上がるSE（1回だけ）
            int stepIndex = Mathf.Max(0, comboIndex - 1);
            AudioManager.Instance?.PlayComboStep(stepIndex);

            foreach (Vector2Int pos in matches)
            {
                EmoteController emote = grid.GetEmoteAt(pos.x, pos.y);
                if (emote != null)
                {
                    emote.SetMatchHighlight(true, Color.yellow);
                }
            }

            yield return new WaitForSeconds(k_MatchDelay);
        }

        private IEnumerator ClearMatches(GridController grid, List<Vector2Int> matches)
        {
            // Remove lines before starting the clear animation
            grid.ClearMatchLines();

            // Bottom-up clear
            List<Vector2Int> ordered = matches
                .OrderBy(p => p.y) // bottom (small y) first
                .ThenBy(p => p.x)
                .ToList();

            foreach (Vector2Int pos in ordered)
            {
                EmoteController emote = grid.GetEmoteAt(pos.x, pos.y);
                if (emote != null)
                {
                    // 消える瞬間にマッチ音。5chainなら5回鳴る。
                    AudioManager.Instance?.PlayMatch();
                    emote.PlayMatchEffect();
                    grid.RemoveEmoteAt(pos.x, pos.y);
                    Object.Destroy(emote.gameObject);
                }

                // small stagger between each clear for a wave effect
                yield return new WaitForSeconds(0.05f);
            }

            yield return new WaitForSeconds(0.2f);
        }

        /// <summary>
        /// グリッド内の現在のマッチを連続グループごとに抽出（横と縦）
        /// </summary>
        private List<List<Vector2Int>> FindMatchGroups(GridController grid)
        {
            GridCell[,] data = grid.GetGrid();
            int width = grid.Width;
            int height = grid.Height;
            var groups = new List<List<Vector2Int>>();

            // Horizontal groups
            for (int y = 0; y < height; y++)
            {
                int x = 0;
                while (x < width)
                {
                    EmoteController current = data[x, y].Emote;
                    if (current == null || current.IsRising)
                    {
                        x++;
                        continue;
                    }

                    int matchLength = 1;
                    int startX = x;
                    for (int checkX = x + 1; checkX < width; checkX++)
                    {
                        EmoteController next = data[checkX, y].Emote;
                        if (next == null || next.IsRising || next.Data != current.Data)
                            break;
                        matchLength++;
                    }

                    if (matchLength >= 3)
                    {
                        var group = new List<Vector2Int>();
                        for (int i = 0; i < matchLength; i++)
                        {
                            group.Add(new Vector2Int(startX + i, y));
                        }
                        groups.Add(group);
                    }

                    x += matchLength;
                }
            }

            // Vertical groups
            for (int x = 0; x < width; x++)
            {
                int y = 0;
                while (y < height)
                {
                    EmoteController current = data[x, y].Emote;
                    if (current == null || current.IsRising)
                    {
                        y++;
                        continue;
                    }

                    int matchLength = 1;
                    int startY = y;
                    for (int checkY = y + 1; checkY < height; checkY++)
                    {
                        EmoteController next = data[x, checkY].Emote;
                        if (next == null || next.IsRising || next.Data != current.Data)
                            break;
                        matchLength++;
                    }

                    if (matchLength >= 3)
                    {
                        var group = new List<Vector2Int>();
                        for (int i = 0; i < matchLength; i++)
                        {
                            group.Add(new Vector2Int(x, startY + i));
                        }
                        groups.Add(group);
                    }

                    y += matchLength;
                }
            }

            return groups;
        }
    }
}