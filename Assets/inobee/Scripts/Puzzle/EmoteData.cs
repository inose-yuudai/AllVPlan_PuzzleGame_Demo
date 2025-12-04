using UnityEngine;
using UnityEngine.Video; // ★ VideoClip を使うために追加

namespace EmoteOrchestra.Data
{
    [CreateAssetMenu(fileName = "EmoteData", menuName = "EmoteOrchestra/Emote Data")]
    public class EmoteData : ScriptableObject
    {
        [Header("基本情報")]
        public string emoteName;
        public Sprite sprite; // ★パズル用のスプライト
        public Color emoteColor = Color.white;
        
        [Header("ゲームバランス")]
        public int baseScore = 100;
        public float spawnWeight = 1.0f; // 出現確率の重み
        
        [Header("エフェクト")]
        public GameObject matchEffectPrefab;
        public AudioClip matchSound;
        
        [Header("Vtuberリアクション")]
        [TextArea] public string reactionText = "ありがとう♪";
        public string animationTrigger = "Happy";

        // --- ▼▼▼ 修正 ▼▼▼ ---
        [Header("演出用（カットイン）")]
        // public Sprite cutInSprite; // 以前のスプライト（不要なら削除）
        public VideoClip cutInVideo; // ★カットイン表示用の動画に変更
        // --- ▲▲▲ 修正 ▲▲▲ ---
    }
}