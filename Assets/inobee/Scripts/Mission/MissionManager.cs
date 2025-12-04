using System.Collections.Generic;
using UnityEngine;
using EmoteOrchestra.Puzzle;
using EmoteOrchestra.UI;

namespace EmoteOrchestra.Mission
{
    /// <summary>
    /// ミッション管理（AnyChain、AllClear対応）
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField] private MissionUIManager _missionUIManager;
        [SerializeField] private ViewerCountManager _viewerCountManager;
        [SerializeField] private SubscriberCountManager _subscriberCountManager;
        [SerializeField] private GridController _gridController; // ★追加：グリッドサイズ取得用
        [SerializeField] private TensionGauge _tensionGauge; // ★追加
		[SerializeField] private PopTensionGauge _popTensionGauge; // テンション（Pop）

        [Header("利用可能なミッション")]
        [SerializeField] private List<MissionData> _availableMissions;

        [Header("設定")]
        [SerializeField] private int _maxActiveMissions = 2;
        [SerializeField] private bool _usePercentageReward = true;
        [SerializeField] private float _rewardPercentage = 0.2f;
		[SerializeField, Tooltip("テンション比に応じて視聴者報酬を増やす")] private bool _scaleViewerRewardByTension = true;
		[SerializeField, Tooltip("テンション最小時の視聴者報酬倍率")] private float _viewerRewardMinMultiplier = 1.0f;
		[SerializeField, Tooltip("テンション最大時の視聴者報酬倍率")] private float _viewerRewardMaxMultiplier = 2.0f;

        [Header("全達成ボーナス")]
        [SerializeField] private bool _enableAllClearBonus = true;
        [SerializeField] private int _allClearBonusViewers = 500;
        [SerializeField] private float _allClearBonusPercentage = 1.0f;

        private List<MissionProgress> _activeMissions = new List<MissionProgress>();
        private Queue<MissionData> _missionQueue = new Queue<MissionData>();
        private int _totalMissionCount;
        private int _completedMissionCount;

        private void Start()
        {
            InitializeMissions();
        }

        private void InitializeMissions()
        {
            _totalMissionCount = _availableMissions.Count;
            _completedMissionCount = 0;

            foreach (MissionData mission in _availableMissions)
            {
                _missionQueue.Enqueue(mission);
            }

            Debug.Log($"[Mission] 全ミッション数: {_totalMissionCount}");

            for (int i = 0; i < _maxActiveMissions && _missionQueue.Count > 0; i++)
            {
                StartNextMission();
            }
        }

        private void StartNextMission()
        {
            if (_missionQueue.Count == 0)
            {
                Debug.Log("[Mission] すべてのミッションが表示されました");
                return;
            }

            MissionData nextMission = _missionQueue.Dequeue();
            MissionProgress progress = new MissionProgress(nextMission);
            _activeMissions.Add(progress);

            Debug.Log($"[Mission] 新しいミッション開始: {nextMission.missionTitle}");

            if (_missionUIManager != null)
            {
                _missionUIManager.ShowMission(progress);
            }
        }

        public void OnComboFinished(ComboResult result)
        {
            foreach (MissionProgress mission in _activeMissions)
            {
                if (mission.IsCompleted)
                    continue;

                CheckMissionProgress(mission, result);
            }

            ProcessCompletedMissions();
        }

        private void CheckMissionProgress(MissionProgress mission, ComboResult result)
        {
            switch (mission.Data.missionType)
            {
                case MissionType.Combo:
                    if (result.TotalComboCount >= mission.Data.targetValue)
                    {
                        // しきい値を満たしたら1回で達成扱いにする
                        mission.AddProgress(mission.Data.targetValue);
                    }
                    break;

                case MissionType.Chain:
                    // 特定エモートの最大チェイン数
                    int maxChain = 0;
                    foreach (ChainData chain in result.Chains)
                    {
                        if (chain.ChainCount > maxChain)
                        {
                            maxChain = chain.ChainCount;
                        }
                    }
                    if (maxChain >= mission.Data.targetValue)
                    {
                        mission.AddProgress(1);
                    }
                    break;

                case MissionType.AnyChain:
                    // ★新規：なんでもいいから指定数以上消し
                    foreach (ChainData chain in result.Chains)
                    {
                        if (chain.ChainCount >= mission.Data.targetValue)
                        {
                            mission.AddProgress(1);
                            break; // 1回のコンボで1回だけカウント
                        }
                    }
                    break;

                case MissionType.AllClear:
                    // ★新規：全消し（グリッド上のエモートが全部消えたか）
                    if (CheckAllClear())
                    {
                        mission.AddProgress(1);
                        Debug.Log("[Mission] AllClear達成: グリッド全消し！");
                    }
                    break;

                case MissionType.TotalMatches:
                    int totalMatches = 0;
                    foreach (ChainData chain in result.Chains)
                    {
                        totalMatches += chain.ChainCount;
                    }
                    mission.AddProgress(totalMatches);
                    break;

                case MissionType.SpecificEmote:
                    foreach (ChainData chain in result.Chains)
                    {
                        if (mission.Data.targetEmote == null || 
                            chain.EmoteData == mission.Data.targetEmote)
                        {
                            mission.AddProgress(chain.ChainCount);
                        }
                    }
                    break;
            }

            if (_missionUIManager != null)
            {
                _missionUIManager.UpdateMissionProgress(mission);
            }
        }

        /// <summary>
        /// 全消しチェック（グリッド上にエモートが1個もない状態）
        /// </summary>
        private bool CheckAllClear()
        {
            if (_gridController == null)
            {
                Debug.LogWarning("[Mission] GridControllerが設定されていないため全消し判定不可");
                return false;
            }

            GridCell[,] grid = _gridController.GetGrid();
            int width = _gridController.Width;
            int height = _gridController.Height;

            // せり上がり中のエモートを除外してチェック
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    EmoteController emote = grid[x, y].Emote;
                    
                    // エモートが存在し、かつせり上がり中でない場合は全消しではない
                    if (emote != null && !emote.IsRising)
                    {
                        return false;
                    }
                }
            }

            // すべてのマスが空（またはせり上がり中のみ）
            return true;
        }

        private void ProcessCompletedMissions()
        {
            List<MissionProgress> completedMissions = new List<MissionProgress>();

            foreach (MissionProgress mission in _activeMissions)
            {
                if (mission.IsCompleted)
                {
                    completedMissions.Add(mission);
                }
            }

            foreach (MissionProgress completed in completedMissions)
            {
                OnMissionCompleted(completed);
                _activeMissions.Remove(completed);
            }
        }

            private void OnMissionCompleted(MissionProgress mission)
    {
        _completedMissionCount++;

        Debug.Log($"★★★[Mission] 達成！ {mission.Data.missionTitle} ({_completedMissionCount}/{_totalMissionCount})");

        // 報酬を計算
        int rewardAmount = CalculateReward(mission.Data);

		// 視聴者数報酬を付与（テンションに応じて倍率）
        if (_viewerCountManager != null)
        {
			int finalViewers = rewardAmount;
			if (_scaleViewerRewardByTension && _popTensionGauge != null)
			{
				float t = Mathf.Clamp01(_popTensionGauge.TensionRatio);
				float mul = Mathf.Lerp(_viewerRewardMinMultiplier, _viewerRewardMaxMultiplier, t);
				finalViewers = Mathf.RoundToInt(rewardAmount * mul);
			}
			_viewerCountManager.AddViewers(finalViewers);
        }
        if (_subscriberCountManager != null)
        {
            _subscriberCountManager.AddSubscribers(rewardAmount);
        }

        // ★テンション報酬を付与
        if (_tensionGauge != null)
        {
            _tensionGauge.OnMissionCompleted(mission.Data.tensionReward);
        }
		if (_popTensionGauge != null)
		{
			_popTensionGauge.OnMissionCompleted(mission.Data.tensionReward);
		}

        // UIに完了を通知
        if (_missionUIManager != null)
        {
            _missionUIManager.OnMissionCompleted(mission);
        }

        StartNextMission();

        if (_completedMissionCount >= _totalMissionCount)
        {
            OnAllMissionsCompleted();
        }
    }

        private int CalculateReward(MissionData missionData)
        {
            if (_usePercentageReward && _viewerCountManager != null)
            {
                int currentViewers = _viewerCountManager.CurrentViewerCount;
                int percentageReward = Mathf.RoundToInt(currentViewers * _rewardPercentage);
                int finalReward = Mathf.Max(percentageReward, missionData.viewerReward);
                return finalReward;
            }
            else
            {
                return missionData.viewerReward;
            }
        }

        private void OnAllMissionsCompleted()
        {

            if (!_enableAllClearBonus)
                return;

            if (_viewerCountManager == null)
                return;

            int currentViewers = _viewerCountManager.CurrentViewerCount;
            int percentageBonus = Mathf.RoundToInt(currentViewers * _allClearBonusPercentage);
            int totalBonus = _allClearBonusViewers + percentageBonus;

            _viewerCountManager.AddViewers(totalBonus);
            if (_subscriberCountManager != null)
            {
                _subscriberCountManager.AddSubscribers(totalBonus);
            }

            if (_missionUIManager != null)
            {
                _missionUIManager.OnAllMissionsCompleted();
            }
        }

        /// <summary>
        /// 外部からのミッション失敗通知時に登録者数を減らす
        /// </summary>
        public void OnMissionFailed(int subscriberPenalty = 1)
        {
            if (_subscriberCountManager == null)
                return;

            if (subscriberPenalty <= 0)
                return;

            _subscriberCountManager.DecreaseSubscribers(subscriberPenalty);
        }

        [ContextMenu("Reset All Missions")]
        public void ResetAllMissions()
        {
            _activeMissions.Clear();
            _missionQueue.Clear();
            _completedMissionCount = 0;

            if (_missionUIManager != null)
            {
                _missionUIManager.ClearAllMissions();
            }

            InitializeMissions();
        }
    }
}