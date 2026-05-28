using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

public class CenterZonePulse : MonoBehaviour
{
    [Header("Pulse 설정")]
    [SerializeField] private float pulseDuration = 0.15f;
    [SerializeField] private float pulseScale    = 1.3f;

    [Header("임팩트 이펙트 설정")]
    [SerializeField] private GameObject impactPrefab;   // ImpactEffect 프리팹
    [SerializeField] private Transform  impactParent;   // NoteSpawnArea
    [SerializeField] private int        impactCount  = 3;    // 퍼져나가는 원 개수
    [SerializeField] private float      impactDelay  = 0.05f; // 각 원 사이 딜레이
    [SerializeField] private float      impactScale  = 3f;    // 최대 크기
    [SerializeField] private float      impactDuration = 0.4f; // 퍼져나가는 시간

    private List<GameObject> _spawnedImpacts = new List<GameObject>();
    private List<TweenerCore<float, float, FloatOptions>> _delayedCalls = new List<TweenerCore<float, float, FloatOptions>>();
    
    private Vector3  _originScale;
    private Color    _originColor;
    private Image    _image;
    private Sequence _currentSequence;

    private void Awake()
    {
        _image       = GetComponent<Image>();
        _originScale = transform.localScale;
        _originColor = _image.color;
    }

    public void Pulse(NoteJudgement judgement)
    {
        _currentSequence?.Kill();
        transform.localScale = _originScale;
        _image.color         = _originColor;

        Color targetColor = judgement switch
        {
            NoteJudgement.Perfect => Color.yellow,
            NoteJudgement.Good    => Color.green,
            NoteJudgement.Bad     => Color.white,
            _                     => _originColor
        };

        // 심장박동
        _currentSequence = DOTween.Sequence();
        _currentSequence
            .Append(transform.DOScale(_originScale * pulseScale, pulseDuration)
                .SetEase(Ease.OutQuad))
            .Append(transform.DOScale(_originScale, pulseDuration)
                .SetEase(Ease.InQuad))
            .Append(transform.DOScale(_originScale * (pulseScale * 0.7f), pulseDuration * 0.7f)
                .SetEase(Ease.OutQuad))
            .Append(transform.DOScale(_originScale, pulseDuration * 0.7f)
                .SetEase(Ease.InQuad))
            .Join(_image.DOColor(targetColor, pulseDuration)
                .SetEase(Ease.OutQuad))
            .Append(_image.DOColor(_originColor, pulseDuration * 0.5f));

        // 임팩트 이펙트
        SpawnImpacts(targetColor);
    }

    private void SpawnImpacts(Color color)
    {
        if (impactPrefab == null) return;

        for (int i = 0; i < impactCount; i++)
        {
            int   index = i;
            float delay = impactDelay * i;

            DOVirtual.DelayedCall(delay, () =>
            {
                if (impactParent == null) return;

                GameObject    obj    = Instantiate(impactPrefab, impactParent);
                RectTransform rt     = obj.GetComponent<RectTransform>();
                ImpactEffect  effect = obj.GetComponent<ImpactEffect>();

                rt.anchoredPosition = Vector2.zero;

                Color impactColor = color;
                impactColor.a     = 1f - (index * 0.25f);

                effect.Play(impactColor, impactScale, impactDuration);

                // 생성된 오브젝트 추적
                _spawnedImpacts.Add(obj);

            }).SetId("ImpactEffect"); // ID 부여
        }
    }

    public void ClearImpacts()
    {
        // 예약된 DelayedCall 취소
        DOTween.Kill("ImpactEffect");

        // Kill() 호출 후 Destroy (DOTween 먼저 정리)
        foreach (GameObject obj in _spawnedImpacts)
        {
            if (obj == null) continue;
            ImpactEffect effect = obj.GetComponent<ImpactEffect>();
            if (effect != null)
                effect.Kill(); // DOTween Kill 후 Destroy
            else
                Destroy(obj);
        }
        _spawnedImpacts.Clear();

        _currentSequence?.Kill();
        if (this != null)
        {
            transform.localScale = _originScale;
            _image.color         = _originColor;
        }
    }

    private void OnDestroy()
    {
        _currentSequence?.Kill();
    }
}