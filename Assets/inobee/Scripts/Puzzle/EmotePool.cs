using System.Collections.Generic;
using UnityEngine;

namespace EmoteOrchestra.Puzzle
{
    /// <summary>
    /// エモートのオブジェクトプール（Awake対応・親管理 修正版）
    /// </summary>
    [RequireComponent(typeof(RectTransform))] // UIであることを明示
    public class EmotePool : MonoBehaviour
    {
        [SerializeField] private EmoteController _emotePrefab;
        [SerializeField] private int _initialPoolSize = 54;
        
        // Poolの親（自分自身）を保持
        private RectTransform _poolContainer; 

        private readonly Queue<EmoteController> _availableEmotes = new Queue<EmoteController>();

        // ★ Start() ではなく Awake() で初期化
        private void Awake()
        {
            _poolContainer = GetComponent<RectTransform>();
            if (_poolContainer == null)
            {
                Debug.LogError("EmotePool に RectTransform がありません！");
            }
            
            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreateNewEmote();
            }
        }

        public EmoteController Get()
        {
            if (_availableEmotes.Count == 0)
            {
                Debug.LogWarning("EmotePool: プールが枯渇したため、新規生成します。");
                CreateNewEmote();
            }

            EmoteController emote = _availableEmotes.Dequeue();
            
            // ★ SetActive(true) は GridController 側で行う
            // emote.gameObject.SetActive(true);
            
            return emote;
        }

        public void Return(EmoteController emote)
        {
            if (emote == null) return;

            // ★ 親を PoolContainer に戻す
            emote.transform.SetParent(_poolContainer, false);
            
            emote.gameObject.SetActive(false);
            _availableEmotes.Enqueue(emote);
        }

        private void CreateNewEmote()
        {
            // ★ 親を _poolContainer (自分) に設定
            EmoteController emote = Instantiate(_emotePrefab, _poolContainer);
            emote.gameObject.SetActive(false);
            _availableEmotes.Enqueue(emote);
        }
    }
}