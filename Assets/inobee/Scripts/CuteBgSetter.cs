using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CuteBgSetter : MonoBehaviour
{
    private static readonly int k_Color1 = Shader.PropertyToID("_Color1");
    private static readonly int k_Color2 = Shader.PropertyToID("_Color2");
    private static readonly int k_StripeColor = Shader.PropertyToID("_StripeColor");
    private static readonly int k_StripeSpeed = Shader.PropertyToID("_StripeSpeed");
    private static readonly int k_StripeAngle = Shader.PropertyToID("_StripeAngle");
    private static readonly int k_StripeWidth = Shader.PropertyToID("_StripeWidth");

    [SerializeField] private Color _color1 = new Color32(54, 208, 208, 255);
    [SerializeField] private Color _color2 = new Color32(140, 83, 215, 255);
    [SerializeField] private Color _stripeColor = new Color(1f, 1f, 1f, 0.12f);
    [SerializeField] private float _stripeSpeed = 0.3f;
    [SerializeField] private float _stripeAngle = 45f;
    [SerializeField] private float _stripeWidth = 0.15f;

    private Material _material;

    private void Awake()
    {
        var renderer = GetComponent<SpriteRenderer>();
        _material = renderer.material;

        _material.SetColor(k_Color1, _color1);
        _material.SetColor(k_Color2, _color2);
        _material.SetColor(k_StripeColor, _stripeColor);
        _material.SetFloat(k_StripeSpeed, _stripeSpeed);
        _material.SetFloat(k_StripeAngle, _stripeAngle);
        _material.SetFloat(k_StripeWidth, _stripeWidth);
    }

    private void OnDestroy()
    {
        if (_material != null)
        {
            Destroy(_material);
            _material = null;
        }
    }
}
