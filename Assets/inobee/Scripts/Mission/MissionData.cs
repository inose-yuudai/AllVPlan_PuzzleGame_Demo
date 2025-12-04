using UnityEngine;
using EmoteOrchestra.Data;

namespace EmoteOrchestra.Mission
{
    [CreateAssetMenu(fileName = "MissionData", menuName = "EmoteOrchestra/Mission Data")]
    public class MissionData : ScriptableObject
    {
         [Header("ミッションアイコン")]
        public Sprite missionIcon;
        [Header("基本情報")]
        public string missionTitle = "5個同時消し";
        [TextArea(2, 4)]
        public string missionDescription = "エモートを5個以上同時に消そう！";

        [Header("条件")]
        public MissionType missionType = MissionType.Chain;
        public int targetValue = 5;

        [Header("特定エモート指定")]
        public EmoteData targetEmote;

        [Header("報酬")]
        public int viewerReward = 100;
        public int subscriberReward = 10;
        public float tensionReward = 20f;

        [Header("表示設定")]
        public Color missionColor = Color.yellow;

        
    }
}
