using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace Title.Menu
{
    public enum MenuAction
    {
        NewGame,
        LoadGame,
        Option,
        Exit
    }

    public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] private Image _thumbnailImage;
        [SerializeField] private TextMeshProUGUI _labelText;
        [SerializeField] private Button _button;
        // ★ _selectionHighlight を削除

        [Header("Settings")]
        [SerializeField] private MenuAction _menuAction;
        [SerializeField] private Sprite _thumbnailSprite;
        [SerializeField] private Color _normalColor  = new Color(1f, 1f, 1f, 0.7f);
        [SerializeField] private Color _selectedColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private float _highlightDuration = 0.3f;

        private MenuSystemController _menuController;

        public MenuAction MenuAction => _menuAction;

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }

            _button.onClick.AddListener(OnButtonClicked);

            SetupVisuals();
        }

        public void Initialize(MenuSystemController controller)
        {
            _menuController = controller;
        }

        private void SetupVisuals()
        {
            if (_thumbnailImage != null && _thumbnailSprite != null)
            {
                _thumbnailImage.sprite = _thumbnailSprite;
            }

            if (_labelText != null)
            {
                _labelText.text = GetMenuActionLabel();
            }

            // 初期状態はホバーしていない
            SetColor(_normalColor);
            // 通常時のスケールを 0.73 にしたい場合
            transform.localScale = Vector3.one * 0.73f;
        }

        private string GetMenuActionLabel()
        {
            switch (_menuAction)
            {
                case MenuAction.NewGame:
                    return "NEW GAME";
                case MenuAction.LoadGame:
                    return "LOAD GAME";
                case MenuAction.Option:
                    return "OPTION";
                case MenuAction.Exit:
                    return "EXIT";
                default:
                    return "";
            }
        }

        // ボタンクリック時
        private void OnButtonClicked()
        {
            if (_menuController != null)
            {
                _menuController.SelectMenu(_menuAction);
            }

            OnClicked();
        }

        /// <summary>
        /// クリック時の追加処理用。ここに好きな処理を書いてください。
        /// </summary>
        protected virtual void OnClicked()
        {
            if (_menuController != null)
            {
                _menuController.SelectMenu(_menuAction);
            }
        }

        // マウスカーソルがボタン上に乗ったとき
        public void OnPointerEnter(PointerEventData eventData)
        {
            ApplyHighlight();
        }

        // マウスカーソルがボタンから外れたとき
        public void OnPointerExit(PointerEventData eventData)
        {
            RemoveHighlight();
        }

        // ホバー中の見た目（「今までの選択状態」に相当）
        private void ApplyHighlight()
        {
            //ゲムダン10用に、LOADGAME、EXITをハイライトできないようにする
            if(_menuAction == MenuAction.LoadGame || _menuAction == MenuAction.Exit)
            {
                return;
            }
            // 色変更（くっきり）
            SetColor(_selectedColor);

            // スケールアップ（1.0 に）
            transform.DOScale(1f, _highlightDuration).SetEase(Ease.OutBack);
        }

        // ホバーが外れたときの見た目
        private void RemoveHighlight()
        {
            // 色を通常に戻す
            SetColor(_normalColor);

            // スケールを 0.73 に戻す
            transform.DOScale(0.73f, _highlightDuration).SetEase(Ease.OutQuad);
        }

        private void SetColor(Color color)
        {
            if (_thumbnailImage != null)
            {
                _thumbnailImage.DOColor(color, _highlightDuration);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }

            DOTween.Kill(transform);
            if (_thumbnailImage != null) DOTween.Kill(_thumbnailImage);
        }
    }
}
