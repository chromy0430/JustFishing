using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

// 호버 효과가 필요한 버튼에 부착
public class UIButtonAnim : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerClickHandler
{
    [SerializeField] private float hoverScale  = 1.08f;
    [SerializeField] private float clickScale  = 0.93f;
    [SerializeField] private float duration    = 0.12f;

    private Vector3 _originalScale;
    private Tween   _currentTween;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        _currentTween?.Kill();
        _currentTween = transform
            .DOScale(_originalScale * hoverScale, duration)
            .SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData e)
    {
        _currentTween?.Kill();
        _currentTween = transform
            .DOScale(_originalScale, duration)
            .SetEase(Ease.OutQuad);
    }

    public void OnPointerClick(PointerEventData e)
    {
        _currentTween?.Kill();
        _currentTween = transform
            .DOScale(_originalScale * clickScale, duration * 0.5f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
                transform.DOScale(_originalScale, duration * 0.5f)
                    .SetEase(Ease.OutBack));
    }

    private void OnDestroy()
    {
        _currentTween?.Kill();
    }
}