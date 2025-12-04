using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// Controlling parameters of material for transition image
/// </summary>


[RequireComponent(typeof(RawImage))]
[ExecuteAlways]
public class TransitionProgressController : MonoBehaviour
{
    [Range(0f, 1f)]
    public float progress = 0f;

    private RawImage rawImage;
    private Material material;

    void OnEnable()
    {
        rawImage = GetComponent<RawImage>();
        material = rawImage.material;
        SetProgressProperty();
    }

    void Update()
    {
        SetProgressProperty();
    }

    public void SetProgressProperty()
    {
        if (material.HasProperty("_Progress"))
        {
            material.SetFloat("_Progress", progress);
        }
        else
        {
            Debug.LogWarning("The material does not have a 'Progress' property.");
        }
    }

    /// <summary> For Preventing memory leak </summary>  
    private void OnDestroy()
    {
        if (!material)
        {
            Destroy(material);
            material = null;
        }
    }
}
