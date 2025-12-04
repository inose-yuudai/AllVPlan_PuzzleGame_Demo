using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EmoteOrchestra.Puzzle;

namespace EmoteOrchestra.Core
{
    public sealed class SceneFlowController : MonoBehaviour
    {
        [SerializeField] private string _selectSceneName = "SelectScene";
        [SerializeField] private ScreenFader _fader;
        [SerializeField] private bool _disableInputsOnTransition = true;
        [SerializeField] private SwayAndLipSync _vtuber; // 演出対象（未設定なら自動検出）
        [SerializeField] private float _postOutroDelay = 0.6f; // 沈んだ後の待機時間（シーン遷移前）
        [SerializeField] private GridController _gridController; // コンボ進行確認用

        [Header("Hold Image After Sink (Optional)")]
        [SerializeField] private bool _showHoldImageAfterSink = false;
        [SerializeField] private Image _holdImage; // 画面に被せる1枚画像
        [SerializeField] private float _holdFadeInDuration = 0.8f;
        [SerializeField] private float _holdDisplayDuration = 10f;

        private ISceneLoader _sceneLoader;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            _sceneLoader = FindObjectOfType<SceneLoader>();
            if (_fader == null)
            {
                _fader = FindObjectOfType<ScreenFader>();
            }

            if (_vtuber == null)
            {
                _vtuber = FindObjectOfType<SwayAndLipSync>();
            }

            if (_gridController == null)
            {
                _gridController = FindObjectOfType<GridController>();
            }

            var tm = FindObjectOfType<TimeManager>();
            tm?.SetOnTimeUpListener(OnTimeUp);
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnTimeUp()
        {
            _ = NavigateToSelectAsync();
        }

        public async Task NavigateToSelectAsync()
        {
            if (_cts != null) return;
            _cts = new CancellationTokenSource();
            try
            {
                // コンボ実行中は終了処理を保留
                if (_gridController != null)
                {
                    while (_gridController.CurrentState == PuzzleState.Execution)
                    {
                        await Task.Yield();
                    }
                }

                // タイムアップ演出（あれば先に待つ）
                if (_vtuber != null)
                {
                    try { await _vtuber.PlayStreamEndOutroAsync(); }
                    catch { /* 念のため握りつぶし */ }
                }

                // 沈んだ後の余韻時間
                if (_postOutroDelay > 0f)
                {
                    float t = 0f;
                    while (t < _postOutroDelay)
                    {
                        t += Time.unscaledDeltaTime;
                        await Task.Yield();
                    }
                }

                // 完全に沈んだ後、任意の画像をフェードインして表示保持
                if (_showHoldImageAfterSink && _holdImage != null)
                {
                    _holdImage.gameObject.SetActive(true);
                    Color c = _holdImage.color;
                    float startA = 0f;
                    float endA = 1f;
                    c.a = startA;
                    _holdImage.color = c;

                    // フェードイン
                    float tIn = 0f;
                    float durIn = Mathf.Max(0.01f, _holdFadeInDuration);
                    while (tIn < durIn)
                    {
                        tIn += Time.unscaledDeltaTime;
                        float r = Mathf.Clamp01(tIn / durIn);
                        c.a = Mathf.Lerp(startA, endA, r);
                        _holdImage.color = c;
                        await Task.Yield();
                    }
                    c.a = endA;
                    _holdImage.color = c;

                    // 表示保持
                    float tHold = 0f;
                    float durHold = Mathf.Max(0f, _holdDisplayDuration);
                    while (tHold < durHold)
                    {
                        tHold += Time.unscaledDeltaTime;
                        await Task.Yield();
                    }
                }

                if (_disableInputsOnTransition)
                {
                    // 必要であればここで入力やBGMを制御
                    // 例: AudioManager.Instance?.FadeOutBgm(0.2f);
                }

                if (_fader != null) await _fader.FadeOut();

                if (_sceneLoader == null)
                {
                    _sceneLoader = FindObjectOfType<SceneLoader>();
                }

                if (_sceneLoader != null)
                {
                    await _sceneLoader.LoadSingleAsync(_selectSceneName, _cts.Token);
                }
                else
                {
                    // フォールバック: 直接SceneManagerでロード
                    AsyncOperation op = SceneManager.LoadSceneAsync(_selectSceneName, LoadSceneMode.Single);
                    op.allowSceneActivation = true;
                    while (!op.isDone)
                    {
                        if (_cts.IsCancellationRequested) break;
                        await Task.Yield();
                    }
                }

                if (_fader != null) await _fader.FadeIn();
            }
            finally
            {
                _cts.Dispose();
                _cts = null;
            }
        }
    }
}


