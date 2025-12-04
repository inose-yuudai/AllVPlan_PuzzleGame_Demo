using System.Collections.Generic;
using UnityEngine;

namespace EmoteOrchestra.UI
{
	[CreateAssetMenu(menuName = "EmoteOrchestra/Comments/EmoteIconLibrary", fileName = "EmoteIconLibrary")]
	public class EmoteIconLibrary : ScriptableObject
	{
		[Header("エモートアイコン（ランダム選択）")]
		public List<Sprite> emoteSprites = new List<Sprite>();

		[Header("3個以上の時に使う特別アイコン（任意）")]
		public Sprite specialIcon;
	}
}


