using System.Collections;
using UnityEngine;
using UnityEngine.UI; // RawImage のために必要
using UnityEngine.Video; // VideoPlayer のために必要
using TMPro;

namespace EmoteOrchestra.UI
{
    public class CutInController : MonoBehaviour
    {
        [Header("参照（Inspectorで設定）")]
        [SerializeField] private RawImage _characterRawImage; // Image から RawImage に変更
        [SerializeField] private VideoPlayer _videoPlayer; // 動画再生用に参照を追加
        [SerializeField] private TextMeshProUGUI _comboText;

        // private float _displayDuration = 2.0f; // 動画の再生時間に依存するため不要に

        private Coroutine _showCoroutine;
        private RenderTexture _renderTexture; // 動画をUIに描画するための中間テクスチャ

        private void Awake()
        {
            // 起動時は非表示にする
            gameObject.SetActive(false);

            // VideoPlayerの初期設定
            if (_videoPlayer != null)
            {
                _videoPlayer.playOnAwake = false; // 自動再生はオフ
                _videoPlayer.source = VideoSource.VideoClip; // ソースはClip
                _videoPlayer.renderMode = VideoRenderMode.RenderTexture; // RenderTextureに出力
                _videoPlayer.isLooping = false; // カットインはループしない
            }
        }

        private void OnDestroy()
        {
            // オブジェクト破棄時にRenderTextureが残っていれば解放
            ReleaseRenderTexture();
        }

        /// <summary>
        /// カットインを表示（動画版）
        /// </summary>
        /// <param name="videoClip">再生する動画クリップ</param>
        /// <param name="comboCount">表示するコンボ数</param>
        public void Show(VideoClip videoClip, int comboCount)
        {
            if (_characterRawImage == null || _videoPlayer == null || _comboText == null)
            {
                Debug.LogError("CutInControllerの参照が設定されていません。");
                return;
            }
            if (videoClip == null)
            {
                Debug.LogError("再生するVideoClipがnullです。");
                return;
            }

            // 既に表示中なら停止（古いRenderTextureの解放も含む）
            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
                _videoPlayer.Stop();
                ReleaseRenderTexture();
            }

            // 1. データを先にセットする
            _videoPlayer.clip = videoClip;
            _comboText.text = $"{comboCount} COMBO!!";

            // 2. 自分自身をアクティブにする
            gameObject.SetActive(true);

            // 3. コルーチンを開始
            _showCoroutine = StartCoroutine(ShowCoroutine());
        }

        private IEnumerator ShowCoroutine()
        {
            // --- ▼▼▼ 動画対応ロジック ▼▼▼ ---

            // 1. 動画の解像度に合わせたRenderTextureを動的に生成
            //    (uint)にキャストして解像度を取得
            _renderTexture = new RenderTexture(
                (int)_videoPlayer.clip.width,
                (int)_videoPlayer.clip.height,
                24 // 24ビット深度バッファ (0でも良い場合もある)
            );
            
            // 2. VideoPlayerの出力先とRawImageの表示テクスチャを紐付け
            _videoPlayer.targetTexture = _renderTexture;
            _characterRawImage.texture = _renderTexture;

            // 3. 動画の準備（非同期）
            _videoPlayer.Prepare();

            // 4. 準備完了を待つ
            while (!_videoPlayer.isPrepared)
            {
                yield return null;
            }

            // 5. 準備ができたので再生開始
            _videoPlayer.Play();

            // 6. 動画の再生時間分だけ待機
            //    (float)にキャストして秒数を取得
            yield return new WaitForSeconds((float)_videoPlayer.clip.length);

            // 7. 再生終了後の後処理
            _videoPlayer.Stop();
            gameObject.SetActive(false);

            // 8. 動的に作成したRenderTextureを解放
            ReleaseRenderTexture();
            
            _showCoroutine = null;
            
            // --- ▲▲▲ 動画対応ロジック ▲▲▲ ---
        }

        /// <summary>
        /// RenderTextureを安全に解放する
        /// </summary>
        private void ReleaseRenderTexture()
        {
            if (_renderTexture != null)
            {
                // RawImageとVideoPlayerの参照を切る
                _characterRawImage.texture = null;
                _videoPlayer.targetTexture = null;

                // 解放
                _renderTexture.Release();
                _renderTexture = null;
            }
        }
    }
}