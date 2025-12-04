using UnityEngine;
using UnityEngine.UI;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// エモートコメント表示アイテム（複数アイコン横並び対応版）
    /// </summary>
    public class EmoteCommentItem : MonoBehaviour
    {
        [Header("必須参照")]
        [SerializeField] private Transform _iconContainer; // アイコンを並べる親（HorizontalLayoutGroup推奨）
        [SerializeField] private GameObject _iconPrefab; // アイコン1つ分のPrefab（Image）
        
        [Header("オプション：特別アイコン")]
        [SerializeField] private Image _specialIconImage; // 3個以上の時に表示する特別アイコン（任意）

        /// <summary>
        /// コメントを設定（複数アイコン対応）
        /// </summary>
        /// <param name="specialIcon">特別アイコン（3個以上の時に表示）</param>
        /// <param name="emoteSprite">エモートのスプライト</param>
        /// <param name="count">表示する個数</param>
        /// <param name="overrideSize">サイズを上書きするか</param>
        /// <param name="specialIconSize">特別アイコンのサイズ</param>
        /// <param name="emoteIconSize">エモートアイコンのサイズ</param>
        public void SetComment(
            Sprite specialIcon,
            Sprite emoteSprite,
            int count,
            bool overrideSize,
            Vector2 specialIconSize,
            Vector2 emoteIconSize)
        {
            // 特別アイコンの表示（3個以上の場合）
                if (_specialIconImage != null)
            {
                if (count >= 3 && specialIcon != null)
                {
                    _specialIconImage.sprite = specialIcon;
                    _specialIconImage.enabled = true;

                    if (overrideSize)
                    {
                        RectTransform rt = _specialIconImage.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            rt.sizeDelta = specialIconSize;
                        }
                    }
                }
                else
                {
                    _specialIconImage.enabled = false;
                }
            }

            // エモートアイコンを個数分生成
            if (_iconContainer != null && _iconPrefab != null && emoteSprite != null)
            {
                // 既存のアイコンをクリア
                foreach (Transform child in _iconContainer)
                {
                    Destroy(child.gameObject);
                }

                // プレハブの基準サイズ（overrideが無ければこれを使う）
                Vector2 baseSize = Vector2.zero;
                RectTransform prefabRTForIcon = _iconPrefab.GetComponent<RectTransform>();
                if (prefabRTForIcon != null)
                {
                    baseSize = prefabRTForIcon.sizeDelta;
                }

                Vector2 desiredIconSize = overrideSize ? emoteIconSize : baseSize;

                // 親レイアウトが子サイズを強制している場合への対応
                GridLayoutGroup grid = _iconContainer.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    grid.cellSize = desiredIconSize;
                }
                HorizontalLayoutGroup hGroup = _iconContainer.GetComponent<HorizontalLayoutGroup>();
                if (hGroup != null)
                {
                    hGroup.childControlWidth = false;
                    hGroup.childControlHeight = false;
                }
                VerticalLayoutGroup vGroup = _iconContainer.GetComponent<VerticalLayoutGroup>();
                if (vGroup != null)
                {
                    vGroup.childControlWidth = false;
                    vGroup.childControlHeight = false;
                }

                // 個数分のアイコンを生成
                for (int i = 0; i < count; i++)
                {
                    GameObject iconObj = Instantiate(_iconPrefab, _iconContainer);
                    Image iconImage = iconObj.GetComponent<Image>();

                    if (iconImage != null)
                    {
                        iconImage.sprite = emoteSprite;

                        RectTransform rt = iconImage.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            rt.sizeDelta = desiredIconSize;
                        }
                    }
                }
            }
        }
    }
}