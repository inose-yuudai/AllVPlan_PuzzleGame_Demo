using UnityEngine;
using EmoteOrchestra.Events;

namespace EmoteOrchestra.Vtuber
{
    /// <summary>
    /// Vtuberの制御（Live2D統合）
    /// </summary>
    public class VtuberController : MonoBehaviour
    {
        [Header("コンポーネント")]
        [SerializeField] private GameObject _live2dModel;
        [SerializeField] private ReactionSystem _reactionSystem;
        [SerializeField] private ExpressionController _expressionController;

        [Header("イベント")]
        [SerializeField] private IntGameEvent _onEmoteMatchedEvent;
        [SerializeField] private IntGameEvent _onComboChangedEvent;
        [SerializeField] private GameEvent _onFeverStartEvent;

        private void Start()
        {
            if (_onEmoteMatchedEvent != null)
            {
                // イベントリスナーの代わりに直接登録はできないので
                // GameEventListenerコンポーネントを使用
            }
        }

        public void OnEmoteMatched(int matchCount)
        {
            if (_reactionSystem != null)
            {
                _reactionSystem.PlayReaction(matchCount);
            }

            if (_expressionController != null)
            {
                _expressionController.ShowHappyExpression();
            }
        }

        public void OnComboChanged(int combo)
        {
            if (combo > 10 && _expressionController != null)
            {
                _expressionController.ShowExcitedExpression();
            }
        }

        public void OnFeverStart()
        {
            if (_expressionController != null)
            {
                _expressionController.ShowMaxExcitedExpression();
            }

            Debug.Log("Vtuber: フィーバータイム！");
        }

        public void PlayIdleAnimation()
        {
            // Live2Dのアイドルアニメーション再生
            Debug.Log("Vtuber: アイドル状態");
        }

        public void PlaySingingAnimation()
        {
            // Live2Dの歌唱アニメーション再生
            Debug.Log("Vtuber: 歌唱中");
        }
    }
}