using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[DisallowMultipleComponent]
public class EmoteSelectScaler : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Target")]
    [Tooltip("未設定なら自身のTransform")]
    [SerializeField] private Transform _target;

    [Header("Scale Settings")]
    [SerializeField] private float _hoverScaleMultiplier = 1.10f;
    [SerializeField] private float _selectedScaleMultiplier = 1.12f;
    [SerializeField] private float _scaleDuration = 0.12f;
    [SerializeField] private Ease _ease = Ease.OutSine;

    [Header("Triggers")]
    [Tooltip("UI ナビゲーションの選択(ISelectHandler)で拡大")]
    [SerializeField] private bool _useSelectEvents = true;
    [Tooltip("マウスホバー(IPointerEnter/Exit)でも拡大")]
    [SerializeField] private bool _useHoverEvents = true;

    [Header("Options")]
    [Tooltip("有効化時にスケールを初期化")]
    [SerializeField] private bool _resetOnEnable = true;

    private Transform _t;
    private Vector3 _originalScale;
    private bool _isHovered;
    private bool _isSelected;

    private void Awake()
    {
        _t = transform;
        if (_target == null) _target = _t;
        _originalScale = _target.localScale;
    }

    private void OnEnable()
    {
        if (_resetOnEnable)
        {
            _target.DOKill();
            _target.localScale = _originalScale;
            _isHovered = false;
            _isSelected = false;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!_useSelectEvents) return;
        SetSelected(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (!_useSelectEvents) return;
        SetSelected(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_useHoverEvents) return;
        SetHovered(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_useHoverEvents) return;
        SetHovered(false);
    }

    /// <summary>
    /// 外部から手動でハイライト切替（hover/selected両方をまとめて指定）。
    /// </summary>
    public void SetHighlighted(bool highlighted)
    {
        _isHovered = highlighted;
        _isSelected = highlighted;
        UpdateScale();
    }

    /// <summary>外部からホバー状態を設定</summary>
    public void SetHovered(bool hovered)
    {
        _isHovered = hovered;
        UpdateScale();
    }

    /// <summary>外部から選択状態を設定</summary>
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateScale();
    }

    private void AnimateTo(Vector3 toScale)
    {
        _target.DOKill();
        _target.DOScale(toScale, _scaleDuration).SetEase(_ease);
    }

    private void UpdateScale()
    {
        float mul = 1f;
        if (_isHovered) mul = Mathf.Max(mul, _hoverScaleMultiplier);
        if (_isSelected) mul = Mathf.Max(mul, _selectedScaleMultiplier);
        AnimateTo(_originalScale * mul);
    }
}


