using System.Collections.Generic;
using UnityEngine;

namespace EmoteOrchestra.Data
{
    /// <summary>
    /// 歌詞の1行分のデータ
    /// </summary>
    [System.Serializable]
    public class LyricLine
    {
        public float startTime;
        public float endTime;
        public string text;
        public string ruby;

        public LyricLine(float start, float end, string txt, string rb)
        {
            startTime = start;
            endTime = end;
            text = txt;
            ruby = rb;
        }
    }

    /// <summary>
    /// 歌詞データ（CSV から動的生成）
    /// </summary>
    public class LyricsData
    {
        public string songId;
        public List<LyricLine> lines = new List<LyricLine>();
        public bool showRuby = true;

        public LyricsData(string id)
        {
            songId = id;
        }

        public void AddLine(float startTime, float endTime, string text, string ruby)
        {
            lines.Add(new LyricLine(startTime, endTime, text, ruby));
        }

        public void SortLines()
        {
            lines.Sort((a, b) => a.startTime.CompareTo(b.startTime));
        }
    }
}