using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using EmoteOrchestra.Core;

public class SubscribeManager : MonoBehaviour
{
    public static int mainSubscriberCount = 1000;
    public static int mainAddSubscriberCount;

    [SerializeField]  Text mainSubscriberCountText,mainAddSubscriberText,huetaText;

    // アニメーション中フラグ
    private bool isAnimating = false;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        IncreaseSubscriber();
    }

    void Update()
    {

    }

    // mainAddSubscriberCount の分だけ mainSubscriberCount を 2秒かけて徐々に増加表示する
    public void IncreaseSubscriber()
    {
        if (mainAddSubscriberCount <= 0) return;
        if (isAnimating) return;
        // huetaText に増えた人数を表示
        if (huetaText != null) huetaText.text = mainAddSubscriberCount + "人増えた";
        StartCoroutine(AnimateSubscriberIncrease());
    }

    private IEnumerator AnimateSubscriberIncrease()
    {
        isAnimating = true;

        int startCount = mainSubscriberCount;
        int targetCount = mainSubscriberCount + mainAddSubscriberCount;
        float duration = 2f;
        float elapsed = 0f;

        // prepare add text
        Text addText = mainAddSubscriberText;
        Vector3 startPos = Vector3.zero;
        Color startColor = Color.white;
        if (addText != null)
        {
            addText.text = "+" + mainAddSubscriberCount.ToString();
            startPos = addText.transform.localPosition;
            startColor = addText.color;
            startColor.a = 1f;
            addText.color = startColor;
        }

        // animate over duration
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // smooth interpolation (can change easing if needed)
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            // update main subscriber displayed value (integer)
            int display = Mathf.FloorToInt(Mathf.Lerp(startCount, targetCount, smooth));
            if (mainSubscriberCountText != null) mainSubscriberCountText.text = display.ToString();

            // animate addText: move up a bit and fade out
            if (addText != null)
            {
                float moveY = 30f; // 上に移動する量
                addText.transform.localPosition = Vector3.Lerp(startPos, startPos + Vector3.up * moveY, smooth);
                Color c = startColor;
                c.a = 1f - smooth;
                addText.color = c;
            }

            yield return null;
        }

        // 完了: 確定表示、リセット
        mainSubscriberCount = targetCount;
        if (mainSubscriberCountText != null) mainSubscriberCountText.text = mainSubscriberCount.ToString();
        if (mainAddSubscriberText != null) mainAddSubscriberText.text = "";

        // リセット追加分
        mainAddSubscriberCount = 0;
        isAnimating = false;
    }
    
    public void BackSelectStage()
    {
        SceneManager.LoadScene("Music");
    }
      
}
