using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// コメント流しUI（テンション連動版、ChatMove風の流れ）
    /// </summary>
    public class CommentStreamUI : MonoBehaviour
    {
        [Header("基本設定")]
        [SerializeField] private ScrollRect _scrollRect;          // ←使わないが残しておく（機能欠落防止）
        [SerializeField] private Transform _contentTransform;
        [SerializeField] private float _commentLifetime = 10f;
        [SerializeField] private int _maxComments = 50;

        [Header("テキストコメント設定")]
        [SerializeField] private GameObject _commentItemPrefab;
        [SerializeField] private float _baseCommentSpeed = 2.0f;

        [Header("エモートコメント設定")]
        [SerializeField] private GameObject _emoteCommentPrefab;
        [SerializeField] private Sprite _specialIcon;

        [Header("サイズ設定")]
        [SerializeField] private Vector2 _emoteIconSize = new Vector2(56f, 56f);
        [SerializeField] private Vector2 _specialIconSize = new Vector2(56f, 56f);
        [SerializeField] private bool _overridePrefabIconSize = false;

        [Header("★テンション連動設定")]
        [SerializeField] private PopTensionGauge _popTensionGauge; // ← PopTensionGaugeに変更
        [SerializeField] private float _tensionPerComment = 2f; // コメント1つあたりのテンション増加量
        [SerializeField] private float _tensionPerEmote = 5f; // エモート1つあたりのテンション増加量

        // 登場頻度（生成間隔）の調整用
        [Header("登場頻度設定")]
        [SerializeField] private float _spawnIntervalMin = 0.2f;   // 最短間隔（高テンション時）
        [SerializeField] private float _spawnIntervalMax = 3.0f;   // 最長間隔（低テンション時）
        [SerializeField, Range(0f, 2f)] private float _tensionWeight = 1.0f; // テンションの影響度（0=無効, 1=通常, 2=強め）

        [Header("ポジティブワード設定")]
        [SerializeField] private List<string> _positiveWords = new List<string>
        {
            "すごい", "いいね", "最高", "88888", "草", "w", "かわいい", "好き", "推せる", "天才"
        };
        [SerializeField] private float _positiveWordBonusTension = 3f; // ポジティブワードのボーナス

        // ChatMove風に上に動かすときの係数（実際の速度はテンションで変わる）
        [Header("移動設定（ChatMove風）")]
        [SerializeField] private float _moveSpeedBase = 100f;

        private List<string> _commentTemplates = new List<string>
        {
            "かわいい！", "888888", "最高！", "推せる", "いいね", "泣ける",
            "天才！", "すごい", "待ってた！", "草", "wwww",
            "やったー", "応援してる", "ファイト！", "神回","歌うまい！"
        };

        // テンションで補正された「コメント生成間隔」用の値として使ってたやつ
        // 実際に使う現在の生成間隔（秒）
        private float _currentCommentSpeed;

        private void Start()
        {
            // 初期は最長間隔から開始
            _currentCommentSpeed = Mathf.Max(_spawnIntervalMin, _spawnIntervalMax);

            if (_popTensionGauge != null)
            {
                _popTensionGauge.OnTensionChanged += OnTensionChanged;
            }

            StartCoroutine(AutoCommentCoroutine());
        }

        private void OnDestroy()
        {
            if (_popTensionGauge != null)
            {
                _popTensionGauge.OnTensionChanged -= OnTensionChanged;
            }
        }

        private void Update()
        {
            // ChatMove と同じように「そこにあるコメントを全部上に動かす」
            MoveCommentsLikeChatMove();
        }

        /// <summary>
        /// テンション変化時の処理
        /// </summary>
        private void OnTensionChanged(float newTension)
        {
            if (_popTensionGauge == null) return;

            // テンションに応じて「生成間隔（小さいほど高頻度）」を調整
            float tensionRatio = Mathf.Clamp01(_popTensionGauge.TensionRatio);
            float effective = Mathf.Clamp01(tensionRatio * _tensionWeight);
            // 低テンション=最長間隔, 高テンション=最短間隔
            _currentCommentSpeed = Mathf.Lerp(_spawnIntervalMax, _spawnIntervalMin, effective);
        }

        private IEnumerator AutoCommentCoroutine()
        {
            while (true)
            {
                // テンションが高いほど短い間隔で出る
                float wait = Random.Range(_spawnIntervalMin, Mathf.Max(_spawnIntervalMin, _currentCommentSpeed));
                yield return new WaitForSeconds(wait);
                AddRandomComment();
            }
        }

        public void AddRandomComment()
        {
            string comment = _commentTemplates[Random.Range(0, _commentTemplates.Count)];
            AddComment(comment);
        }

        /// <summary>
        /// コメントを追加（テンション連動）
        /// </summary>
        public void AddComment(string text)
        {
            if (_commentItemPrefab == null) return;

            GameObject commentObj = Instantiate(_commentItemPrefab, _contentTransform);

            // 寿命
            CommentLifetime lifetime = commentObj.AddComponent<CommentLifetime>();
            lifetime._lifetime = _commentLifetime;

            TextMeshProUGUI textComponent = commentObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = text;
            }

            // テンションに反映
            if (_popTensionGauge != null)
            {
                float tensionGain = _tensionPerComment;

                if (ContainsPositiveWord(text))
                {
                    tensionGain += _positiveWordBonusTension;
                    Debug.Log($"[Comment] ポジティブコメント検出！「{text}」→ +{tensionGain}");
                }

                _popTensionGauge.OnCommentReceived(text);
            }

            TrimOverflowComments();
            // AutoScrollToBottom(); ← スクロール式じゃなくなったので呼ばない
        }

        /// <summary>
        /// エモートコメントを追加（テンション連動）
        /// </summary>
        public void AddEmoteComment(Sprite emoteSprite, int count)
        {
            if (emoteSprite == null || count <= 0) return;

            GameObject item = Instantiate(_emoteCommentPrefab, _contentTransform);

            CommentLifetime lifetime = item.AddComponent<CommentLifetime>();
            lifetime._lifetime = _commentLifetime;

			EmoteCommentItem commentItem = item.GetComponent<EmoteCommentItem>();
            if (commentItem == null)
            {
                Debug.LogError($"_emoteCommentPrefab に EmoteCommentItem.cs がアタッチされていません！", item);
            }
            else
            {
				// 表示上は最大5個まで、アイコンサイズはプレハブに合わせる
				int displayCount = Mathf.Min(count, 5);
				commentItem.SetComment(
					_specialIcon,
					emoteSprite,
					displayCount,
					false, // 常にプレハブのアイコンサイズに合わせる
					_specialIconSize,
					_emoteIconSize
				);
            }

            // エモートはテンション高め
            if (_popTensionGauge != null)
            {
                float tensionGain = _tensionPerEmote * count;
                _popTensionGauge.AddTension(tensionGain);
            }

            TrimOverflowComments();
            // AutoScrollToBottom(); ← スクロール式じゃなくなったので呼ばない
        }

        /// <summary>
        /// ChatMove の「上に流す」動きをまとめたところ
        /// </summary>
        private void MoveCommentsLikeChatMove()
        {
            if (_contentTransform == null) return;

            // テンションで移動速度もノリで上げる
            float tensionRatio = (_popTensionGauge != null) ? _popTensionGauge.TensionRatio : 0f;
            float moveSpeed = _moveSpeedBase * Mathf.Lerp(0.5f, 1.5f, tensionRatio);

            float delta = moveSpeed * Time.deltaTime;

            // 子を全部ちょっとずつ上に動かす
            int childCount = _contentTransform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = _contentTransform.GetChild(i);
                // ChatMove と同じくローカル基準で上へ
                child.Translate(Vector3.up * delta);
            }
        }

        /// <summary>
        /// ポジティブワードが含まれているかチェック
        /// </summary>
        private bool ContainsPositiveWord(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            foreach (var word in _positiveWords)
            {
                if (text.Contains(word))
                {
                    return true;
                }
            }

            return false;
        }

        private void TrimOverflowComments()
        {
            while (_contentTransform.childCount > _maxComments)
            {
                Destroy(_contentTransform.GetChild(0).gameObject);
            }
        }

        // ↓スクロール方式の時に使ってたやつは残しておく（呼んでないだけ）
        private void AutoScrollToBottom()
        {
            Canvas.ForceUpdateCanvases();
            StartCoroutine(ScrollToBottomNextFrame());
        }

        private IEnumerator ScrollToBottomNextFrame()
        {
            yield return null;
            if (_scrollRect != null)
            {
                _scrollRect.verticalNormalizedPosition = 0f;
            }
        }
    }
}
