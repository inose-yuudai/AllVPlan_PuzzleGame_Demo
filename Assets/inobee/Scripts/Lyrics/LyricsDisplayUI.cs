using UnityEngine;
using TMPro;
using DG.Tweening;
using EmoteOrchestra.Core;
using EmoteOrchestra.Data;

namespace EmoteOrchestra.UI
{
    /// <summary>
    /// カラオケ風歌詞表示（CSV対応版）
    /// </summary>
    public class LyricsDisplayUI : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private TextMeshProUGUI _currentLineText;
        [SerializeField] private TextMeshProUGUI _nextLineText;
        [SerializeField] private TextMeshProUGUI _currentLineRuby;
        [SerializeField] private TextMeshProUGUI _currentLineHighlight;
        
        [Header("背景")]
        [SerializeField] private CanvasGroup _lyricsBackground;
        
        [Header("表示設定")]
        [SerializeField] private Color _normalColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        [SerializeField] private Color _highlightColor = Color.white;
        [SerializeField] private float _fadeInDuration = 0.3f;
        [SerializeField] private float _fadeOutDuration = 0.3f;
        
       [SerializeField] private LyricsData _currentLyrics;
        private int _currentLineIndex = -1;
        private float _currentSongTime = 0f;
        private string _currentSongId = "";
        private Sequence _fadeSequence;

      private void Update()
{
    UpdateLyrics();
}

private void UpdateLyrics()
{
    MusicGameManager gameManager = ServiceLocator.Get<MusicGameManager>();
    if (gameManager == null || gameManager.CurrentSong == null)
    {
        // 曲が再生されていない場合は非表示にし、IDをリセット
        if (_lyricsBackground != null && _lyricsBackground.alpha > 0)
        {
            HideLyrics();
            _currentSongId = "";
            _currentLyrics = null;
        }
        return;
    }

    string newSongId = gameManager.CurrentSong.lyricsId;
    
    // 毎フレームではなく、1秒ごとにログ出力
    if (Time.frameCount % 60 == 0)
    {
       // Debug.Log($"現在: songId='{newSongId}', 曲時間={gameManager.CurrentTime}秒, _currentLyrics={((_currentLyrics != null) ? "あり" : "なし")}");
    }
    
    // --- 修正点 ---
    
    // 1. 曲が変更された場合
    if (newSongId != _currentSongId)
    {
        //Debug.Log($"[UpdateLyrics] 曲が変更されました: {_currentSongId} -> {newSongId}");
        _currentSongId = newSongId;
        _currentLyrics = null; // 歌詞データをリセット
        LoadLyrics(_currentSongId); // 読み込みを試行
    }
    // 2. (追加) 曲は同じだが、歌詞データがまだ無い場合
    // (LyricsManagerの読み込み待ちや、何らかの理由で初回LoadLyricsが失敗した場合)
    else if (_currentLyrics == null && !string.IsNullOrEmpty(_currentSongId))
    {
        // 毎フレーム実行すると重いので、1秒に1回程度リトライ
        if (Time.frameCount % 60 == 0)
        {
            Debug.LogWarning($"[UpdateLyrics] 歌詞データがnullのため再取得試行: {_currentSongId}");
            LoadLyrics(_currentSongId);
        }
    }
    
    // --- 修正ここまで ---

    if (_currentLyrics == null || _currentLyrics.lines.Count == 0)
    {
        // 読み込み中、または歌詞データが存在しない場合は、
        // LoadLyrics()内でHideLyrics()が呼ばれるため、ここでは何もしない
        return;
    }

    _currentSongTime = gameManager.CurrentTime;
    
    int newLineIndex = FindCurrentLineIndex(_currentSongTime);
    
    if (Time.frameCount % 60 == 0)
    {
       // Debug.Log($"FindCurrentLineIndex: 時間={_currentSongTime}秒 → lineIndex={newLineIndex}");
    }
    
    if (newLineIndex != _currentLineIndex)
    {
        _currentLineIndex = newLineIndex;
        DisplayLine(_currentLineIndex);
    }
    
    UpdateHighlight(_currentSongTime);
}

       private void LoadLyrics(string songId)
{
   // Debug.Log($"[LoadLyrics] 開始: songId='{songId}'"); // 追加
    
    if (string.IsNullOrEmpty(songId))
    {
        Debug.LogError("[LoadLyrics] songId が空です！"); // 追加
        _currentLyrics = null;
        HideLyrics();
        return;
    }

    LyricsManager lyricsManager = ServiceLocator.Get<LyricsManager>();
    if (lyricsManager == null)
    {
        Debug.LogError("[LoadLyrics] LyricsManager が見つかりません"); // 追加
        _currentLyrics = null;
        HideLyrics();
        return;
    }

   // Debug.Log($"[LoadLyrics] LyricsManager.IsLoaded = {lyricsManager.IsLoaded}"); // 追加
    
    if (!lyricsManager.IsLoaded)
    {
        Debug.LogWarning("[LoadLyrics] LyricsManager がまだ読み込み完了していません"); // 追加
        _currentLyrics = null;
        HideLyrics();
        return;
    }

   // Debug.Log($"[LoadLyrics] GetLyrics 呼び出し: songId='{songId}'"); // 追加
    _currentLyrics = lyricsManager.GetLyrics(songId);
    _currentLineIndex = -1;
    
    if (_currentLyrics != null)
    {
        ShowLyrics();
     //   Debug.Log($"[LoadLyrics] 成功: {songId} ({_currentLyrics.lines.Count}行)"); // 追加
    }
    else
    {
      //  Debug.LogError($"[LoadLyrics] 失敗: GetLyrics が null を返しました。songId='{songId}'"); // 追加
        HideLyrics();
    }
}

        private int FindCurrentLineIndex(float time)
        {
            if (_currentLyrics == null)
                return -1;

            for (int i = _currentLyrics.lines.Count - 1; i >= 0; i--)
            {
                if (time >= _currentLyrics.lines[i].startTime)
                {
                    return i;
                }
            }
            
            return -1;
        }

        private void DisplayLine(int lineIndex)
        {
            if (_currentLyrics == null || lineIndex < 0 || lineIndex >= _currentLyrics.lines.Count)
            {
                ClearDisplay();
                return;
            }

            LyricLine currentLine = _currentLyrics.lines[lineIndex];
            
            if (_currentLineText != null)
            {
                _currentLineText.text = currentLine.text;
                _currentLineText.color = _normalColor;
            }

            if (_currentLineHighlight != null)
            {
                _currentLineHighlight.text = currentLine.text;
                _currentLineHighlight.color = _highlightColor;
                _currentLineHighlight.maxVisibleCharacters = 0;
            }

            if (_currentLineRuby != null && _currentLyrics.showRuby && !string.IsNullOrEmpty(currentLine.ruby))
            {
                _currentLineRuby.text = currentLine.ruby;
                _currentLineRuby.gameObject.SetActive(true);
            }
            else if (_currentLineRuby != null)
            {
                _currentLineRuby.gameObject.SetActive(false);
            }

            if (_nextLineText != null)
            {
                int nextIndex = lineIndex + 1;
                if (nextIndex < _currentLyrics.lines.Count)
                {
                    _nextLineText.text = _currentLyrics.lines[nextIndex].text;
                    _nextLineText.color = new Color(_normalColor.r, _normalColor.g, _normalColor.b, 0.5f);
                }
                else
                {
                    _nextLineText.text = "";
                }
            }

            AnimateFadeIn();
        }

        private void UpdateHighlight(float time)
        {
            if (_currentLyrics == null || _currentLineIndex < 0 || 
                _currentLineIndex >= _currentLyrics.lines.Count)
                return;

            if (_currentLineHighlight == null)
                return;

            LyricLine currentLine = _currentLyrics.lines[_currentLineIndex];
            
            float lineProgress = 0f;
            float lineDuration = currentLine.endTime - currentLine.startTime;
            
            if (lineDuration > 0)
            {
                lineProgress = (time - currentLine.startTime) / lineDuration;
                lineProgress = Mathf.Clamp01(lineProgress);
            }
            else if (time >= currentLine.startTime)
            {
                lineProgress = 1f;
            }

            int totalChars = _currentLineHighlight.textInfo.characterCount;
            int visibleChars = Mathf.FloorToInt(totalChars * lineProgress);
            _currentLineHighlight.maxVisibleCharacters = visibleChars;
        }

        private void ClearDisplay()
        {
            if (_currentLineText != null)
                _currentLineText.text = "";
            
            if (_currentLineHighlight != null)
                _currentLineHighlight.text = "";
            
            if (_nextLineText != null)
                _nextLineText.text = "";
            
            if (_currentLineRuby != null)
                _currentLineRuby.gameObject.SetActive(false);
        }

        private void ShowLyrics()
        {
            if (_lyricsBackground != null)
            {
                _lyricsBackground.alpha = 0f;
                _lyricsBackground.DOFade(1f, _fadeInDuration);
            }
        }

        private void HideLyrics()
        {
            if (_lyricsBackground != null && _lyricsBackground.alpha > 0)
            {
                _lyricsBackground.DOFade(0f, _fadeOutDuration);
            }
            
            ClearDisplay();
        }

        private void AnimateFadeIn()
        {
            _fadeSequence?.Kill();
            
            if (_currentLineText != null)
            {
                _currentLineText.alpha = 0f;
                _currentLineText.DOFade(1f, _fadeInDuration);
            }

            if (_nextLineText != null)
            {
                _nextLineText.alpha = 0f;
                _nextLineText.DOFade(0.5f, _fadeInDuration);
            }
        }

        private void OnDestroy()
        {
            _fadeSequence?.Kill();
        }
    }
}