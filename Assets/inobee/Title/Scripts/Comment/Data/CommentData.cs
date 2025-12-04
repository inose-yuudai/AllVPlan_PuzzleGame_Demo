// CommentData.cs
namespace Title.Comment.Data
{
    [System.Serializable]
    public class CommentData
    {
        public string userName;
        public string commentText;
        public string timestamp;
        public string userIconPath;

        public CommentData(string userName, string commentText, string userIconPath = "")
        {
            this.userName = userName;
            this.commentText = commentText;
            this.timestamp = System.DateTime.Now.ToString("MM/dd HH:mm");
            this.userIconPath = userIconPath;
        }
    }
}