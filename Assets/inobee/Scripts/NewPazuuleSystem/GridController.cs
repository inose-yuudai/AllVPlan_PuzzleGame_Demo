using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmoteOrchestra.Data;
using EmoteOrchestra.Audio;
using EmoteOrchestra.UI; 
using EmoteOrchestra.Mission;
using UnityEngine.UI;

namespace EmoteOrchestra.Puzzle
{
    /// <summary>
    /// グリッド管理（準備/実行の2フェーズ制）
    /// </summary>
    public class GridController : MonoBehaviour
    {
        public enum RefillPattern
        {
            Immediate,
            AfterCombo
        }
        [Header("Grid Settings")]
        [SerializeField] private int _width = 6;
        [SerializeField] private int _height = 5;
        [SerializeField] private float _cellSize = 100f;
        [SerializeField] private float _spacing = 10f;

        [Header("References")]
        [SerializeField] private RectTransform _gridContainer;
        [SerializeField] private EmoteController _emotePrefab;
        [Header("Visuals")]
        [SerializeField] private Image _matchLinePrefab;
        [SerializeField] private RectTransform _effectsContainer;
        
        [Header("Emote Data")]
        [SerializeField] private List<EmoteData> _availableEmotes; // ★ここで管理

        [Header("UI References")]
        [SerializeField] private CommentStreamUI _commentStreamUI;

        [Header("Comment Settings")]
        [SerializeField] private float _commentInterval = 0.1f; // コメントを流す間隔（秒）
        [SerializeField] private int _maxCommentsPerEmote = 10; // 1種類のエモートで流す最大コメント数

        [Header("Mission")]
        [SerializeField] private MissionManager _missionManager;
        [Header("Tension")]
        [SerializeField] private PopTensionGauge _tensionGauge;
		[SerializeField, Tooltip("消した個数あたりのテンション増加倍率")] private float _tensionPerMatchedEmote = 0.3f;
		[SerializeField, Tooltip("コンボ段階あたりのテンション増加倍率")] private float _tensionPerComboStep = 0.5f;
        
        [Header("Refill Pattern")]
        [SerializeField] private RefillPattern _refillPattern = RefillPattern.Immediate;

        private GridCell[,] _grid;
        private MatchDetector _matchDetector;
        private ComboExecutor _comboExecutor;
        private EmoteFallController _fallController;
        private readonly List<GameObject> _activeMatchLines = new List<GameObject>();

        // State Pattern
        private PuzzleState _currentState;
        private Dictionary<PuzzleState, IPuzzleStateHandler> _stateHandlers;
        
        // When true, allow a single refill even during Execution (used at combo end)
        private bool _forceRefillOnce;

        // せり上がり関連
        private float _currentRiseOffset = 0f;
        private const float k_RiseSpeed = 50f;

        public int Width => _width;
        public int Height => _height;
        public PuzzleState CurrentState => _currentState;
        public float CurrentRiseOffset => _currentRiseOffset;

        private void Awake()
        {
            InitializeGrid();
            InitializeStateHandlers();
            _currentState = PuzzleState.Preparation;
        }

        private void Start()
        {
            FillInitialGrid();
            ChangeState(PuzzleState.Preparation);
        }

        private void Update()
        {
            _stateHandlers[_currentState].OnUpdate(this);
        }

        private void InitializeGrid()
        {
            _grid = new GridCell[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _grid[x, y] = new GridCell(x, y);
                }
            }

            _matchDetector = new MatchDetector(_width, _height);
            _fallController = new EmoteFallController();
            _comboExecutor = new ComboExecutor(_matchDetector, _fallController);
        }

        private void InitializeStateHandlers()
        {
            _stateHandlers = new Dictionary<PuzzleState, IPuzzleStateHandler>
            {
                { PuzzleState.Preparation, new PreparationStateHandler() },
                { PuzzleState.Execution, new ExecutionStateHandler(_comboExecutor) },
                { PuzzleState.Locked, new LockedStateHandler() }
            };
        }

        /// <summary>
        /// 状態を変更（State Pattern）
        /// </summary>
        public void ChangeState(PuzzleState newState)
        {
            if (_currentState == newState)
                return;

            _stateHandlers[_currentState].OnExit(this);
            _currentState = newState;
            _stateHandlers[_currentState].OnEnter(this);
        }

        /// <summary>
        /// 2つのエモートをスワップ
        /// </summary>
        public bool SwapEmotes(int x1, int y1, int x2, int y2)
        {
            if (!_stateHandlers[_currentState].CanSwap())
            {
                Debug.Log("現在の状態ではスワップできません");
                return false;
            }

            if (!IsValidPosition(x1, y1) || !IsValidPosition(x2, y2))
                return false;

            EmoteController emote1 = _grid[x1, y1].Emote;
            EmoteController emote2 = _grid[x2, y2].Emote;

            // 両方nullの場合はスワップしない
            if (emote1 == null && emote2 == null)
                return false;

            // グリッドデータを入れ替え
            _grid[x1, y1].Emote = emote2;
            _grid[x2, y2].Emote = emote1;

            // エモートの位置を更新
            if (emote1 != null)
            {
                emote1.SetGridPosition(x2, y2);
                emote1.MoveTo(GetAnchoredPosition(x2, y2), 0.2f);
            }

            if (emote2 != null)
            {
                emote2.SetGridPosition(x1, y1);
                emote2.MoveTo(GetAnchoredPosition(x1, y1), 0.2f);
            }

            return true;
        }
        /// <summary>
        /// スワップを試行（GameInputHandler用）
        /// </summary>
        public bool TrySwap(Vector2Int startPos, Vector2Int targetPos)
        {
            return SwapEmotes(startPos.x, startPos.y, targetPos.x, targetPos.y);
        }

        /// <summary>
        /// 実行ボタンが押された
        /// </summary>
        public void OnExecuteButtonPressed()
        {
            if (!_stateHandlers[_currentState].CanExecute())
            {
                Debug.Log("現在の状態では実行できません");
                return;
            }

            Debug.Log("実行ボタン押下 - コンボ開始！");
            ChangeState(PuzzleState.Execution);
        }

        public GridCell[,] GetGrid() => _grid;
        
        public EmoteController GetEmoteAt(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return null;
            return _grid[x, y].Emote;
        }

        public void RemoveEmoteAt(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return;
            _grid[x, y].Emote = null;
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        public bool IsValidPosition(Vector2Int pos)
        {
            return IsValidPosition(pos.x, pos.y);
        }

        public Vector2 GetAnchoredPosition(int x, int y)
        {
            float totalWidth = _width * _cellSize + (_width - 1) * _spacing;
            float totalHeight = _height * _cellSize + (_height - 1) * _spacing;

            float startX = -totalWidth / 2f + _cellSize / 2f;
            float startY = -totalHeight / 2f + _cellSize / 2f;

            return new Vector2(
                startX + x * (_cellSize + _spacing),
                startY + y * (_cellSize + _spacing)
            );
        }

        /// <summary>
        /// 画面座標からグリッド座標へ変換（グリッド矩形外は false）
        /// </summary>
        public bool TryGetGridFromScreenPoint(Vector2 screenPos, Camera uiCamera, out Vector2Int gridPos)
        {
            gridPos = default;
            if (_gridContainer == null) return false;

            Vector2 localPoint;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_gridContainer, screenPos, uiCamera, out localPoint))
                return false;

            float totalWidth = _width * _cellSize + (_width - 1) * _spacing;
            float totalHeight = _height * _cellSize + (_height - 1) * _spacing;

            float startX = -totalWidth / 2f + _cellSize / 2f;
            float startY = -totalHeight / 2f + _cellSize / 2f;

            float unitX = (_cellSize + _spacing);
            float unitY = (_cellSize + _spacing);

            float fx = (localPoint.x - startX) / unitX;
            float fy = (localPoint.y - startY) / unitY;

            int ix = Mathf.FloorToInt(fx + 0.5f);
            int iy = Mathf.FloorToInt(fy + 0.5f);

            if (!IsValidPosition(ix, iy))
                return false;

            gridPos = new Vector2Int(ix, iy);
            return true;
        }

        /// <summary>
        /// マッチした並びにラインを描画
        /// </summary>
        public void ShowMatchLines(List<List<Vector2Int>> groups)
        {
            ClearMatchLines();

            if (groups == null || groups.Count == 0) return;

            RectTransform parent = _effectsContainer != null ? _effectsContainer : _gridContainer;
            const float lineThickness = 12f;

            foreach (var group in groups)
            {
                if (group == null || group.Count < 3) continue;

                bool isHorizontal = true;
                bool isVertical = true;
                int baseX = group[0].x;
                int baseY = group[0].y;
                foreach (var p in group)
                {
                    if (p.y != baseY) isHorizontal = false;
                    if (p.x != baseX) isVertical = false;
                }

                if (!isHorizontal && !isVertical) continue;

                Vector2 a = GetAnchoredPosition(group[0].x, group[0].y);
                Vector2 b = GetAnchoredPosition(group[group.Count - 1].x, group[group.Count - 1].y);
                Vector2 center = (a + b) * 0.5f;
                float length = isHorizontal ? Mathf.Abs(b.x - a.x) + _cellSize : Mathf.Abs(b.y - a.y) + _cellSize;

                Image lineImage;
                if (_matchLinePrefab != null)
                {
                    lineImage = Instantiate(_matchLinePrefab, parent);
                }
                else
                {
                    var go = new GameObject("MatchLine", typeof(RectTransform), typeof(Image));
                    lineImage = go.GetComponent<Image>();
                    lineImage.color = new Color(1f, 0.9f, 0.2f, 0.9f);
                    lineImage.raycastTarget = false;
                    go.transform.SetParent(parent, false);
                }

                RectTransform rt = lineImage.rectTransform;
                rt.anchoredPosition = center;
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;
                rt.sizeDelta = isHorizontal ? new Vector2(length + (group.Count - 1) * _spacing, lineThickness)
                                            : new Vector2(lineThickness, length + (group.Count - 1) * _spacing);

                _activeMatchLines.Add(lineImage.gameObject);
            }
        }

        /// <summary>
        /// マッチラインをすべて消去
        /// </summary>
        public void ClearMatchLines()
        {
            if (_activeMatchLines.Count == 0) return;
            foreach (var go in _activeMatchLines)
            {
                if (go != null)
                {
                    Destroy(go);
                }
            }
            _activeMatchLines.Clear();
        }

        private void FillInitialGrid()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    SpawnEmoteAt(x, y);
                }
            }
        }

        public IEnumerator FillEmptySpaces()
        {
            // Delay refills during combo if set to AfterCombo, unless explicitly forced once
            if (_refillPattern == RefillPattern.AfterCombo && _currentState == PuzzleState.Execution && !_forceRefillOnce)
            {
                yield break;
            }

            bool wasForced = _forceRefillOnce;
            _forceRefillOnce = false;

            List<EmoteController> newEmotes = new List<EmoteController>();
            float maxFallDuration = 0f;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_grid[x, y].Emote == null)
                    {
                        EmoteController newEmote = SpawnEmoteAt(x, y);
                        newEmotes.Add(newEmote);

                        // spawn above the grid and fall with bounce
                        if (newEmote != null)
                        {
                            RectTransform rt = newEmote.GetComponent<RectTransform>();
                            Vector2 targetPos = GetAnchoredPosition(x, y);
                            float topY = GetAnchoredPosition(x, _height - 1).y;
                            float startY = topY + (_cellSize + _spacing) * 2f; // 2 rows above
                            rt.anchoredPosition = new Vector2(targetPos.x, startY);

                            float distance = Mathf.Abs(startY - targetPos.y);
                            float duration = Mathf.Clamp(distance / 1800f, 0.1f, 0.6f); // pixels/sec approx
                            newEmote.FallTo(targetPos, duration);
                            if (duration > maxFallDuration) maxFallDuration = duration;
                        }
                    }
                }
            }

            if (newEmotes.Count > 0 && maxFallDuration > 0f)
            {
                yield return new WaitForSeconds(maxFallDuration);
            }
        }
        public void OnComboFinished(ComboResult result)
        {
            Debug.Log($"[GridController] コンボ終了 - {result.TotalComboCount}連鎖");

            // マッチSE（コンボ開始合図）
            AudioManager.Instance?.PlayMatch();

            // ★テンション（PopTensionGauge）に加算
            if (_tensionGauge != null)
            {
                int totalMatches = 0;
                foreach (ChainData chain in result.Chains)
                {
                    totalMatches += chain.ChainCount;
                }
				// マッチ数ぶん加算（倍率で調整）
				if (_tensionPerMatchedEmote > 0f)
				{
					_tensionGauge.AddTension(totalMatches * _tensionPerMatchedEmote);
				}

                // コンボ段階も少し加算
                if (result.TotalComboCount > 1)
                {
					if (_tensionPerComboStep > 0f)
					{
						_tensionGauge.AddTension(result.TotalComboCount * _tensionPerComboStep);
					}
                }
            }

            // コメント流し
            if (_commentStreamUI != null)
            {
                StartCoroutine(FlowCommentsCoroutine(result));
            }

            // コンボ段階ごとに徐々にピッチが上がるSE
            // ShowMatchLinesタイミングで1回鳴らす仕様に変更したため、ここでは鳴らさない
            // StartCoroutine(PlayComboSfxCoroutine(result));

            // ミッション進行状況を更新
            if (_missionManager != null)
            {
                _missionManager.OnComboFinished(result);
            }

            // If refills were delayed during combo, perform a single refill now
            if (_refillPattern == RefillPattern.AfterCombo)
            {
                _forceRefillOnce = true;
                StartCoroutine(FillEmptySpaces());
            }
        }

/// <summary>
/// コンボ結果をコメント欄に流す
/// </summary>
private IEnumerator FlowCommentsCoroutine(ComboResult result)
{
    foreach (ChainData chain in result.Chains)
    {
        if (chain.EmoteData == null || chain.EmoteData.sprite == null)
            continue;

        Debug.Log($"[Comment] {chain.EmoteData.emoteName} x{chain.ChainCount}個のコメントを流します");

        // ★チェインごとに1回だけ流す（アイコンは個数分横並び）
        _commentStreamUI.AddEmoteComment(chain.EmoteData.sprite, chain.ChainCount);

        // 次のチェインまで少し待つ
        yield return new WaitForSeconds(_commentInterval);
    }
}

        private IEnumerator PlayComboSfxCoroutine(ComboResult result)
        {
            // 各チェイン（=各コンボ段階）でSEを鳴らす
            for (int i = 0; i < result.Chains.Count; i++)
            {
                AudioManager.Instance?.PlayComboStep(i);
                yield return new WaitForSeconds(_commentInterval);
            }
        }

        /// <summary>
        /// ★重み付きランダムでEmoteDataを取得
        /// </summary>
        private EmoteData GetRandomEmoteData()
        {
            if (_availableEmotes == null || _availableEmotes.Count == 0)
            {
                Debug.LogError("利用可能なEmoteDataがありません！");
                return null;
            }

            // 重みの合計を計算
            float totalWeight = 0f;
            foreach (EmoteData emote in _availableEmotes)
            {
                totalWeight += emote.spawnWeight;
            }

            // ランダムな値を取得
            float randomValue = Random.Range(0f, totalWeight);

            // 重みに基づいて選択
            float cumulativeWeight = 0f;
            foreach (EmoteData emote in _availableEmotes)
            {
                cumulativeWeight += emote.spawnWeight;
                if (randomValue <= cumulativeWeight)
                {
                    return emote;
                }
            }

            // フォールバック（念のため）
            return _availableEmotes[0];
        }

        private EmoteController SpawnEmoteAt(int x, int y)
        {
            EmoteData randomData = GetRandomEmoteData();
            
            if (randomData == null)
            {
                Debug.LogError($"EmoteDataの取得に失敗しました at ({x}, {y})");
                return null;
            }
            
            GameObject emoteObj = Instantiate(_emotePrefab.gameObject, _gridContainer);
            EmoteController emote = emoteObj.GetComponent<EmoteController>();

            emote.Initialize(randomData);
            emote.SetGridPosition(x, y);

            RectTransform rect = emoteObj.GetComponent<RectTransform>();
            rect.anchoredPosition = GetAnchoredPosition(x, y);

            _grid[x, y].Emote = emote;

            return emote;
        }

    }
    
}