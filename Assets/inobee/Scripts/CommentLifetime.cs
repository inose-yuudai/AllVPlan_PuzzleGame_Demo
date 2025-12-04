using UnityEngine;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// アタッチされたGameObjectを、指定された秒数後に自動で破棄するコンポーネント
    /// </summary>
    public class CommentLifetime : MonoBehaviour
    {
        // CommentStreamUIから設定される
        public float _lifetime = 10f; 
        
        private void Start()
        {
            // _lifetime 秒後に自動的にこのGameObjectを破棄する
            Destroy(gameObject, _lifetime);
        }
    }
}