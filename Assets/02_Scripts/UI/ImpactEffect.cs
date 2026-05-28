using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ImpactEffect : MonoBehaviour
{
    private Image         _image;
    private RectTransform _rt;
    private Sequence      _sequence;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _rt    = GetComponent<RectTransform>();
    }

    public void Play(Color color, float endScale, float duration)
    {
        _image.color   = color;
        _rt.localScale = Vector3.one;

        _sequence = DOTween.Sequence();
        _sequence
            .Append(_rt.DOScale(endScale, duration).SetEase(Ease.OutQuad))
            .Join(_image.DOFade(0f, duration).SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                if (this != null && gameObject != null)
                    Destroy(gameObject);
            });
    }

    // 외부에서 강제 종료 시 호출
    public void Kill()
    {
        _sequence?.Kill();
        if (gameObject != null)
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        _sequence?.Kill();
    }
}