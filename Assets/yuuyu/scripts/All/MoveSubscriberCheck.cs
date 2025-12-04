using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveSubscriberCheck : MonoBehaviour
{
    [SerializeField] private string sceneName = "SubscriberCheckScene";
    [SerializeField] private float delaySeconds = 2f;

    // Start is called before the first frame update
    void OnEnable()
    {
        StartCoroutine(LoadSceneAfterDelay());
    }



    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(delaySeconds);
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
