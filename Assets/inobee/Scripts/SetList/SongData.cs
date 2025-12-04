using UnityEngine;

namespace EmoteOrchestra.Data
{
    [CreateAssetMenu(fileName = "SongData", menuName = "EmoteOrchestra/Song Data")]
    public class SongData : ScriptableObject
    {
        [Header("楽曲情報")]
        public string songTitle;
        public string artistName;
        public AudioClip audioClip;
        public Sprite albumArt;
        
        [Header("ゲーム設定")]
        public float bpm = 120f;
        
        [Header("曲の長さ（秒）")]
        [Tooltip("0以下の場合はAudioClipから自動取得")]
        public float songDuration = 0f;
        
        public int targetScore = 50000;
        
        [Header("難易度")]
        [Range(1, 5)] public int difficulty = 3;
        public float emoteSpawnSpeed = 1.0f;
        
        [Header("おすすめエモート")]
        public EmoteData[] recommendedEmotes;
        public float recommendedEmoteBonus = 1.5f;

        [Header("歌詞")]
        [Tooltip("歌詞CSVでの曲ID")]
        public string lyricsId = "";

        /// <summary>
        /// 実際の曲の長さを取得（AudioClipから自動取得）
        /// </summary>
        public float GetActualDuration()
        {
            if (songDuration > 0)
            {
                return songDuration;
            }

            if (audioClip != null)
            {
                return audioClip.length;
            }

            return 180f;
        }

        /// <summary>
        /// 曲の長さを分:秒形式で取得
        /// </summary>
        public string GetDurationString()
        {
            float duration = GetActualDuration();
            int minutes = Mathf.FloorToInt(duration / 60f);
            int seconds = Mathf.FloorToInt(duration % 60f);
            return $"{minutes}:{seconds:00}";
        }

        private void OnValidate()
        {
            if (audioClip != null && songDuration <= 0)
            {
                Debug.Log($"{songTitle}: 曲の長さ = {GetDurationString()} (自動取得)");
            }
        }
    }
}