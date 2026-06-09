using UnityEngine;
using DG.Tweening;

public class UIPanelAnim : MonoBehaviour
{
    [SerializeField] private float   openDuration  = 0.25f;
    [SerializeField] private float   closeDuration = 0.18f;
    [SerializeField] private Vector3 closedScale   = new Vector3(0.85f, 0.85f, 1f);

    private CanvasGroup _cg;
    private Sequence    _seq;

    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        _seq?.Kill();
        transform.localScale = closedScale;
        _cg.alpha            = 0f;

        _seq = DOTween.Sequence();
        _seq.Append(transform.DOScale(Vector3.one, openDuration).SetEase(Ease.OutBack))
            .Join(_cg.DOFade(1f, openDuration).SetEase(Ease.OutQuad));
    }

    public void Close(System.Action onComplete = null)
    {
        _seq?.Kill();
        _seq = DOTween.Sequence();
        _seq.Append(transform.DOScale(closedScale, closeDuration).SetEase(Ease.InQuad))
            .Join(_cg.DOFade(0f, closeDuration).SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
    }

    private void OnDestroy() => _seq?.Kill();
}