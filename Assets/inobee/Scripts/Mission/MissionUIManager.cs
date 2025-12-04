using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace EmoteOrchestra.Mission
{
    /// <summary>
    /// ミッションUI管理
    /// ・最大2つ表示（MissionManager側の数に依存）
    /// ・1つだけ達成したらチェックを付けて残す
    /// ・2つとも達成していたら、前の「消える演出」を両方にかけてから破棄する
    /// ・達成済みの枠は次のミッション表示時に再利用する
    /// </summary>
    public class MissionUIManager : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField] private Transform _missionContainer;
        [SerializeField] private GameObject _missionUIItemPrefab;

        [Header("全達成演出（オプション）")]
        [SerializeField] private GameObject _allClearEffectPrefab; // パーティクルなど
        [SerializeField] private TextMeshProUGUI _allClearText;    // "全ミッション達成！"
        [SerializeField] private AudioSource _allClearAudio;    // "全ミッション達成！"

        // MissionProgress → UIItem
        private Dictionary<MissionProgress, MissionUIItem> _missionUIItems =
            new Dictionary<MissionProgress, MissionUIItem>();

        public void ShowMission(MissionProgress mission)
        {
            if (_missionUIItemPrefab == null || _missionContainer == null)
                return;

            // 1. 達成済みの枠を探して再利用
            MissionProgress oldKey;
            MissionUIItem reusable = FindCompletedItem(out oldKey);
            if (reusable != null)
            {
                // 前に紐づいてたMissionProgressを外す
                if (oldKey != null)
                {
                    _missionUIItems.Remove(oldKey);
                }

                reusable.Initialize(mission);
                _missionUIItems[mission] = reusable;
                return;
            }

            // 2. 見つからなければ新しく生成
            GameObject itemObj = Instantiate(_missionUIItemPrefab, _missionContainer);
            MissionUIItem uiItem = itemObj.GetComponent<MissionUIItem>();
            if (uiItem != null)
            {
                uiItem.Initialize(mission);
                _missionUIItems[mission] = uiItem;
            }
        }

        public void UpdateMissionProgress(MissionProgress mission)
        {
            if (_missionUIItems.TryGetValue(mission, out MissionUIItem uiItem))
            {
                uiItem.UpdateProgress();
            }
        }

        /// <summary>
        /// 1つのミッションが達成されたときに呼ばれる
        /// </summary>
        public void OnMissionCompleted(MissionProgress mission)
        {
            if (!_missionUIItems.TryGetValue(mission, out MissionUIItem uiItem))
                return;

            // まずは「チェックだけ付ける」軽い演出
            uiItem.MarkCompleted();

            // 今表示されているミッションが全部達成済みなら、ここで2つとも消す演出に移行する
            if (AreAllCurrentMissionsCompleted())
            {
                // いきなり辞書を回しながら消すと壊れるので一旦コピー
                var itemsSnapshot = new List<KeyValuePair<MissionProgress, MissionUIItem>>(_missionUIItems);

                foreach (var kv in itemsSnapshot)
                {
                    MissionProgress prog = kv.Key;
                    MissionUIItem item = kv.Value;
                    if (item == null)
                        continue;

                    item.PlayCompleteAnimation(() =>
                    {
                        _missionUIItems.Remove(prog);
                        Destroy(item.gameObject);
                    });
                }
            }
        }

        /// <summary>
        /// 今表示されているUIがすべて IsCompleted になっているかチェック
        /// </summary>
        private bool AreAllCurrentMissionsCompleted()
        {
            bool hasAny = false;
            foreach (var kv in _missionUIItems)
            {
                hasAny = true;
                MissionUIItem item = kv.Value;
                if (item == null)
                    continue;

                if (!item.IsCompleted)
                {
                    return false;
                }
            }
            // 1個もないときはfalseでいい
            return hasAny;
        }

        /// <summary>
        /// 達成済みのUIアイテムを1つ探す（次のミッション表示時に再利用するため）
        /// </summary>
        private MissionUIItem FindCompletedItem(out MissionProgress keyOfItem)
        {
            foreach (var kv in _missionUIItems)
            {
                MissionProgress progress = kv.Key;
                MissionUIItem item = kv.Value;
                if (item != null && item.IsCompleted)
                {
                    keyOfItem = progress;
                    return item;
                }
            }
            keyOfItem = null;
            return null;
        }

        /// <summary>
        /// 全ミッション達成時の演出
        /// </summary>
public void OnAllMissionsCompleted()
        {
            Debug.Log("[MissionUI] 全ミッション達成演出を再生");
            _allClearAudio.Play();

                // OnAllMissionsCompleted メソッド内

        if (_allClearEffectPrefab != null)
        {
            // 1. 基準位置 = このスクリプトがついているオブジェクトのワールド座標
            Vector3 basePosition = transform.position;

            // 2. 「ちょっと右上」へのオフセット（ズラす量）
            // (ワールド座標系での 1.0f, 1.0f) ※シーンに合わせて調整してください
            const float k_OffsetX = 6.0f;
            const float k_OffsetY = 4.5f;
            Vector3 offset = new Vector3(k_OffsetX, k_OffsetY, 0f);

            // 3. 最終的な生成位置を計算
            Vector3 spawnPosition = basePosition + offset;

            // 4. 親を指定せず、計算したワールド座標に生成
            GameObject effect = Instantiate(_allClearEffectPrefab, spawnPosition, Quaternion.identity);
            
            Destroy(effect, 3f);
        }

            if (_allClearText != null)
            {
                _allClearText.gameObject.SetActive(true);
                _allClearText.alpha = 0f;
                _allClearText.transform.localScale = Vector3.zero;

                DOTween.Sequence()
                    .Append(_allClearText.transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack))
                    .Join(_allClearText.DOFade(1f, 0.3f))
                    .AppendInterval(2f)
                    .Append(_allClearText.DOFade(0f, 0.5f))
                    .OnComplete(() => _allClearText.gameObject.SetActive(false));
            }
        }


        /// <summary>
        /// すべてのミッションUIをクリア
        /// </summary>
        public void ClearAllMissions()
        {
            foreach (var item in _missionUIItems.Values)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _missionUIItems.Clear();
        }
    }
}
