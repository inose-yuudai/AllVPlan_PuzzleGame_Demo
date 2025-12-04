using UnityEngine;

public class OpenURL : MonoBehaviour
{
    // 引数にURLを受け取り、それを開く
    public void OpenBrowser(string url)
    {
        Application.OpenURL(url);
    }
}
