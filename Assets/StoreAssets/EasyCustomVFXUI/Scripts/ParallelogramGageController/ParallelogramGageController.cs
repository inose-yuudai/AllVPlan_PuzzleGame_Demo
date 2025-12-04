using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[ExecuteAlways]

public class ParallelogramGageController : MonoBehaviour
{
    [Range(0f, 1f)]
    public float fillAmount = 0.5f;

    [Range(30f, 75f)]
    public float cutAngle = 45.0f;

    private Material controlledMaterial;

    void OnEnable()
    {
        Image image = GetComponent<Image>();
        if (image != null)
        {
            controlledMaterial = image.material;
        }
    }

    void Update()
    {
        if (controlledMaterial != null)
        {
            controlledMaterial.SetFloat("_FillAmount", Mathf.Clamp(fillAmount, 0.0f, 1.0f));

            controlledMaterial.SetFloat("_CutAngle", Mathf.Clamp(cutAngle, 30.0f, 75.0f));
        }
    }
}
