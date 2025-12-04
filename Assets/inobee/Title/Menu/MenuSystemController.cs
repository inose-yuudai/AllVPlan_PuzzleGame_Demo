// MenuSystemController.cs
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using Title.Animation;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

namespace Title.Menu
{
    public class MenuSystemController : MonoBehaviour
    {
        [Header("Menu Buttons")]
        [SerializeField] private List<MenuButton> _menuButtons = new List<MenuButton>();
        
        [Header("Play Button")]
        [SerializeField] private Button _playButton;
        [SerializeField] private CanvasGroup _playButtonGroup;
        [SerializeField] private Image _playButtonGlow;
        
        [Header("Comment Section")]
        [SerializeField] private Button _commentToggleButton;
        [SerializeField] private RectTransform _commentSection;
        [SerializeField] private RectTransform _menuButtonsContainer;
        [SerializeField] private float _commentSectionHeight = 400f;
        
        [Header("Animation Settings")]
        [SerializeField] private float _glowDuration = 1f;
        [SerializeField] private float _glowIntensity = 1.5f;
        [SerializeField] private float _slideAnimationDuration = 0.5f;

        [Header("References")]
        [SerializeField] private AnimatedImageController _animatedImageController;

        [Header("New Game Video")]
        [SerializeField] private VideoPlayer _newGameVideoPlayer;
        
        private MenuAction? _selectedMenuAction;
        private bool _isCommentSectionOpen;
        private Sequence _glowSequence;

        public GameObject maskPanel;
        public GameObject popUpPanel;

        private void Awake()
        {
            // メニューボタンの初期化
            foreach (var button in _menuButtons)
            {
                button.Initialize(this);
            }
            
            // 再生ボタンの初期化
            if (_playButton != null)
            {
                _playButton.onClick.AddListener(OnPlayButtonClicked);
                _playButtonGroup.alpha = 0f;
                _playButton.interactable = false;
            }
            
            // コメントトグルボタンの初期化
            if (_commentToggleButton != null)
            {
                _commentToggleButton.onClick.AddListener(ToggleCommentSection);
            }

            // コメントセクションの初期状態
            if (_commentSection != null)
            {
                _commentSection.gameObject.SetActive(false);
                _isCommentSectionOpen = false;
            }
            if (_newGameVideoPlayer != null)
            {
                _newGameVideoPlayer.gameObject.SetActive(false);
            }
        }

        public void SelectMenu(MenuAction action)
        {
            // 既に選択されている場合は何もしない
            if (_selectedMenuAction == action)
            {
                return;
            }
            
            
            switch (action)
            {
                case MenuAction.NewGame:
                    // ★ NewGame のとき：動画再生
                    PlayNewGameVideo();
                    break;

                case MenuAction.LoadGame:
                    // 既存のロード処理があるならここに
                    // LoadGame();
                    break;

                case MenuAction.Option:
                    maskPanel.SetActive(true);
                    popUpPanel.SetActive(true);
                    return;

                case MenuAction.Exit:
                    // 既存の終了処理など
                    // QuitGame();
                    break;

                default:
                    break;
            }
            
            Debug.Log($"Menu selected: {action}");
        }

        // private void DeselectCurrentMenu()
        // {
        //     if (!_selectedMenuAction.HasValue) return;
            
        //     MenuButton currentButton = _menuButtons.Find(b => b.MenuAction == _selectedMenuAction.Value);
        //     if (currentButton != null)
        //     {
        //         currentButton.Deselect();
        //     }
        // }

        private void ActivatePlayButton()
        {
            if (_playButton == null) return;
            
            // 再生ボタンを表示
            _playButton.interactable = true;
            _playButtonGroup.DOFade(1f, 0.3f);
            
            // グロー効果を開始
            StartGlowEffect();
        }

        private void StartGlowEffect()
        {
            if (_playButtonGlow == null) return;
            
            // 既存のグロー効果を停止
            if (_glowSequence != null && _glowSequence.IsActive())
            {
                _glowSequence.Kill();
            }
            
            // 新しいグロー効果を開始
            _glowSequence = DOTween.Sequence();
            _glowSequence.Append(_playButtonGlow.DOFade(_glowIntensity, _glowDuration).SetEase(Ease.InOutSine));
            _glowSequence.Append(_playButtonGlow.DOFade(0.3f, _glowDuration).SetEase(Ease.InOutSine));
            _glowSequence.SetLoops(-1, LoopType.Yoyo);
        }

        private void StopGlowEffect()
        {
            if (_glowSequence != null && _glowSequence.IsActive())
            {
                _glowSequence.Kill();
            }
            
            if (_playButtonGlow != null)
            {
                _playButtonGlow.DOFade(0f, 0.3f);
            }
        }

        private void OnPlayButtonClicked()
        {
            if (!_selectedMenuAction.HasValue)
            {
                Debug.LogWarning("No menu action selected!");
                return;
            }
            
            // グロー効果を停止
            StopGlowEffect();
            
            // 選択されたアクションを実行
            ExecuteMenuAction(_selectedMenuAction.Value);
        }

        private void ExecuteMenuAction(MenuAction action)
        {
            Debug.Log($"Executing menu action: {action}");
            
            switch (action)
            {
                case MenuAction.NewGame:
                    break;
                case MenuAction.LoadGame:
                    LoadGame();
                    break;
                case MenuAction.Option:
                    OpenOption();
                    break;
                case MenuAction.Exit:
                    ExitGame();
                    break;
            }
        }



        private void LoadGame()
        {
            Debug.Log("Loading game...");
            // ロード処理を実装
        }

        private void OpenOption()
        {
            Debug.Log("Opening options...");
            // オプション画面を開く処理を実装
        }

        private void ExitGame()
        {
            Debug.Log("Exiting game...");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        private void ToggleCommentSection()
        {
            _isCommentSectionOpen = !_isCommentSectionOpen;
            
            if (_isCommentSectionOpen)
            {
                OpenCommentSection();
            }
            else
            {
                CloseCommentSection();
            }
        }

        private void OpenCommentSection()
        {
            if (_commentSection == null) return;
            
            // コメントセクションを表示
            _commentSection.gameObject.SetActive(true);
            
            // メニューボタンを下に移動
            if (_menuButtonsContainer != null)
            {
                _menuButtonsContainer.DOAnchorPosY(-_commentSectionHeight, _slideAnimationDuration)
                    .SetEase(Ease.OutCubic);
            }
            
            // コメントセクションをスライドイン
            _commentSection.sizeDelta = new Vector2(_commentSection.sizeDelta.x, 0);
            _commentSection.DOSizeDelta(new Vector2(_commentSection.sizeDelta.x, _commentSectionHeight), _slideAnimationDuration)
                .SetEase(Ease.OutCubic);
            
            Debug.Log("Comment section opened");
        }

        private void CloseCommentSection()
        {
            if (_commentSection == null) return;
            
            // メニューボタンを元の位置に戻す
            if (_menuButtonsContainer != null)
            {
                _menuButtonsContainer.DOAnchorPosY(0, _slideAnimationDuration)
                    .SetEase(Ease.OutCubic);
            }
            
            // コメントセクションをスライドアウト
            _commentSection.DOSizeDelta(new Vector2(_commentSection.sizeDelta.x, 0), _slideAnimationDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    _commentSection.gameObject.SetActive(false);
                });
            
            Debug.Log("Comment section closed");
        }

        private void OnDestroy()
        {
            if (_playButton != null)
            {
                _playButton.onClick.RemoveListener(OnPlayButtonClicked);
            }

            if (_commentToggleButton != null)
            {
                _commentToggleButton.onClick.RemoveListener(ToggleCommentSection);
            }

            StopGlowEffect();
            DOTween.Kill(_playButtonGroup);
            DOTween.Kill(_menuButtonsContainer);
            DOTween.Kill(_commentSection);
        }

        private void PlayNewGameVideo()
        {
            if (_newGameVideoPlayer == null)
            {
                Debug.LogWarning("NewGame 用の VideoPlayer が設定されていません。");
                return;
            }

            // 必要なら一度停止してから
            _newGameVideoPlayer.Stop();

            // オブジェクトを表示して再生
            _newGameVideoPlayer.gameObject.SetActive(true);
            _newGameVideoPlayer.Play();

            _newGameVideoPlayer.loopPointReached += OnNewGameVideoFinished;

        }
        private void OnNewGameVideoFinished(VideoPlayer source)
        {
            print("New Game video finished.");
            // イベント解除（多重登録防止）
            source.loopPointReached -= OnNewGameVideoFinished;

            // 必要なら動画を非表示にする
            source.gameObject.SetActive(false);

            SceneManager.LoadScene("Story");

        }
    }
}