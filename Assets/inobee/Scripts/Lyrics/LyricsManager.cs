using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using EmoteOrchestra.Data;

namespace EmoteOrchestra.Core
{
    /// <summary>
    /// 歌詞データの読み込み・管理（オンライン対応版）
    /// </summary>
    public class LyricsManager : MonoBehaviour
    {
        [Header("Google Spreadsheet設定")]
        [SerializeField] private string _spreadsheetId = "";
        [SerializeField] private string _sheetGid = "0"; // シートのGID（通常は0）
        
        [Header("ローカルCSVファイル（オフライン用）")]
        [SerializeField] private TextAsset _fallbackCsvFile;
        
        [Header("読み込み設定")]
        [SerializeField] private bool _loadOnStart = true;
        [SerializeField] private bool _useOnlineFirst = true; // オンライン優先
        
        private Dictionary<string, LyricsData> _lyricsDatabase = new Dictionary<string, LyricsData>();
        private bool _isLoaded = false;
        private bool _isLoading = false;

       private void Awake()
{
    ServiceLocator.Register<LyricsManager>(this);
    
    // Awake で読み込み開始
    if (_loadOnStart)
    {
        if (_useOnlineFirst)
        {
            StartCoroutine(LoadLyricsFromOnline());
        }
        else
        {
            LoadLyricsFromLocal();
        }
    }
}

        private void Start()
        {
           
        }

        /// <summary>
        /// オンラインからスプレッドシートを読み込む
        /// </summary>
       public IEnumerator LoadLyricsFromOnline()
{
    if (_isLoading)
    {
        Debug.LogWarning("LyricsManager: すでに読み込み中です");
        yield break;
    }

    if (string.IsNullOrEmpty(_spreadsheetId))
    {
        Debug.LogError("LyricsManager: Spreadsheet ID が設定されていません");
        LoadLyricsFromLocal();
        yield break;
    }

    _isLoading = true;
    _isLoaded = false;

    string csvUrl = $"https://docs.google.com/spreadsheets/d/{_spreadsheetId}/export?format=csv&gid={_sheetGid}";
    
    Debug.Log($"LyricsManager: スプレッドシートから読み込み中... URL={csvUrl}"); // 修正

    using (UnityWebRequest request = UnityWebRequest.Get(csvUrl))
    {
        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.Success)
        {
            string csvText = request.downloadHandler.text;

            
            ParseCSV(csvText);
  
        }
        else
        {

            LoadLyricsFromLocal();
        }
    }

    _isLoading = false;
}

        /// <summary>
        /// ローカルCSVファイルから読み込む（フォールバック）
        /// </summary>
        private void LoadLyricsFromLocal()
        {
            if (_fallbackCsvFile == null)
            {
                Debug.LogWarning("LyricsManager: ローカルCSVファイルが設定されていません");
                return;
            }

            ParseCSV(_fallbackCsvFile.text);

        }

        /// <summary>
        /// CSVテキストをパース
        /// </summary>
        private void ParseCSV(string csvText)
        {
            _lyricsDatabase.Clear();

            string[] lines = csvText.Split('\n');

            // ヘッダー行をスキップ
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                try
                {
                    string[] values = ParseCSVLine(line);
                    
                    if (values.Length < 5)
                        continue;

                    string songId = values[0].Trim();
                    int lineIndex = int.Parse(values[1].Trim());
                    float startTime = float.Parse(values[2].Trim());
                    float endTime = float.Parse(values[3].Trim());
                    string text = values[4].Trim();
                    string ruby = values.Length > 5 ? values[5].Trim() : "";

                    // 曲IDでグループ化
                    if (!_lyricsDatabase.ContainsKey(songId))
                    {
                        _lyricsDatabase[songId] = new LyricsData(songId);
                    }

                    _lyricsDatabase[songId].AddLine(startTime, endTime, text, ruby);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"LyricsManager: 行{i}のパースエラー - {e.Message}");
                }
            }

            // 各曲の歌詞を時系列順にソート
            foreach (var lyrics in _lyricsDatabase.Values)
            {
                lyrics.SortLines();
            }

            _isLoaded = true;
        }

        /// <summary>
        /// CSV行をパース（カンマ区切り、ダブルクォート対応）
        /// </summary>
        private string[] ParseCSVLine(string line)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            string current = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }

            result.Add(current);
            return result.ToArray();
        }

        /// <summary>
        /// 指定した曲IDの歌詞を取得
        /// </summary>
       public LyricsData GetLyrics(string songId)
{
    
    if (!_isLoaded)
    {
        Debug.LogWarning("LyricsManager: まだ歌詞が読み込まれていません");
        return null;
    }

    if (_lyricsDatabase.TryGetValue(songId, out LyricsData lyrics))
    {

        return lyrics;
    }

    Debug.LogWarning($"LyricsManager: 曲ID '{songId}' の歌詞が見つかりません");
    Debug.LogWarning($"データベースの内容: {string.Join(", ", _lyricsDatabase.Keys)}"); // 追加
    return null;
}

        /// <summary>
        /// スプレッドシートを再読み込み
        /// </summary>
        public void ReloadFromOnline()
        {
            StartCoroutine(LoadLyricsFromOnline());
        }

        /// <summary>
        /// 読み込まれている曲IDのリストを取得
        /// </summary>
        public List<string> GetAvailableSongIds()
        {
            return new List<string>(_lyricsDatabase.Keys);
        }

        /// <summary>
        /// 歌詞が読み込まれているか
        /// </summary>
        public bool IsLoaded => _isLoaded;

        /// <summary>
        /// 読み込み中かどうか
        /// </summary>
        public bool IsLoading => _isLoading;
    }
}