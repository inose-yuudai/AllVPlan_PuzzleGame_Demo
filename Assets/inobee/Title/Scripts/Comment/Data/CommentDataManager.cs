// CommentDataManager.cs
using UnityEngine;
using System.IO;

namespace Title.Comment.Data
{
    public static class CommentDataManager
    {
        private const string k_SaveFileName = "comments.json";
        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, k_SaveFileName);

        public static void SaveComments(CommentDataCollection collection)
        {
            try
            {
                string json = JsonUtility.ToJson(collection, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"Comments saved to: {SaveFilePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save comments: {e.Message}");
            }
        }

        public static CommentDataCollection LoadComments()
        {
            if (!File.Exists(SaveFilePath))
            {
                Debug.Log("No save file found. Creating new collection.");
                return new CommentDataCollection();
            }

            try
            {
                string json = File.ReadAllText(SaveFilePath);
                CommentDataCollection collection = JsonUtility.FromJson<CommentDataCollection>(json);
                Debug.Log($"Loaded {collection.comments.Count} comments");
                return collection;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load comments: {e.Message}");
                return new CommentDataCollection();
            }
        }

        public static string GetSaveFilePath()
        {
            return SaveFilePath;
        }

        public static bool SaveFileExists()
        {
            return File.Exists(SaveFilePath);
        }
    }
}