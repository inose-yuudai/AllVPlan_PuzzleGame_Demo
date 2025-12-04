using System.Collections.Generic;
using UnityEngine;

namespace EmoteOrchestra.UI
{
	[CreateAssetMenu(menuName = "EmoteOrchestra/Comments/CommentLibrary", fileName = "CommentLibrary")]
	public class CommentLibrary : ScriptableObject
	{
		[Header("コメント候補（ランダム選択）")]
		public List<string> comments = new List<string>();
	}
}


