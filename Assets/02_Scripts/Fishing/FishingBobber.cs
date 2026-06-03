using System;
using System.Collections;
using UnityEngine;
using StylizedWater3;

[RequireComponent(typeof(Collider))]
public class FishingBobber : MonoBehaviour
{
    [SerializeField] private ParticleSystem _splashVFX;
    [SerializeField] private ParticleSystem _impactVFX;
    [SerializeField] private ParticleSystem splashParticle;
    
    private Vector3 _surfacePosition;
    private Coroutine _currentRoutine;
    private AlignToWater _alignToWater;
    private Action _onLandedCallback;
    private bool _hasLanded = false;

    private void Awake()
    {
        _alignToWater = GetComponent<AlignToWater>();

        if (_alignToWater != null) _alignToWater.enabled = false;
        if (splashParticle != null) splashParticle.gameObject.SetActive(false);
        
        Hide();
    }

    public void Cast(Vector3 from, Vector3 to, FishingData data, Action onLanded)
    {
        _hasLanded = false;
        _onLandedCallback = onLanded;

        if (_alignToWater != null) _alignToWater.enabled = false;

        gameObject.SetActive(true);
        if (_currentRoutine != null) StopCoroutine(_currentRoutine);
        _currentRoutine = StartCoroutine(CastRoutine(from, to, data, onLanded));
    }

    public void StartWaiting(FishingData data, Action onBite)
    {
        if (_currentRoutine != null) StopCoroutine(_currentRoutine);
        _currentRoutine = StartCoroutine(WaitRoutine(data, onBite));
    }

    public void Hide()
    {
        if (_currentRoutine != null) StopCoroutine(_currentRoutine);
        _hasLanded = false;

        if (_alignToWater != null)
            _alignToWater.enabled = false;

        gameObject.SetActive(false);
    }

    private IEnumerator CastRoutine(Vector3 from, Vector3 to, FishingData data, Action onLanded)
    {
        float elapsed = 0f;
        while (elapsed < data.castDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / data.castDuration;

            Vector3 pos = Vector3.Lerp(from, to, t);
            pos.y += data.arcHeight * Mathf.Sin(Mathf.PI * t);
            transform.position = pos;

            yield return null;
        }
        
        AudioManager.Instance?.PlaySplash();

        transform.position = to;
        
        if (_alignToWater != null) _alignToWater.enabled = true;
        Instantiate(_splashVFX, this.transform.position, Quaternion.identity);
        Instantiate(_impactVFX, this.transform.position, Quaternion.identity);

        onLanded?.Invoke();
    }

    private IEnumerator WaitRoutine(FishingData data, Action onBite)
    {
        float waitTime = UnityEngine.Random.Range(data.waitMinTime, data.waitMaxTime);
        yield return new WaitForSeconds(waitTime);

        // 콜백 먼저 호출 (물고기 접근 연출 시작)
        onBite?.Invoke();

        // BiteRoutine은 FishingController에서 직접 호출하도록 변경
    }
    
    public void PlayBiteAnimation(FishingData data, Action onComplete)
    {
        _surfacePosition = transform.position;
        StartCoroutine(BiteRoutine(data, onComplete));
    }

    private IEnumerator BiteRoutine(FishingData data, Action onComplete = null)
    {
        if (_alignToWater != null) _alignToWater.enabled = false;

        Vector3 startPos = transform.position;
        Vector3 endPos   = startPos - new Vector3(0f, data.biteDepth, 0f);

        float elapsed = 0f;
        float duration = data.biteDuration * 0.3f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // EaseInQuart: 처음엔 느리다가 확 내려감
            float t     = elapsed / duration;
            float eased = t * t * t * t;
            transform.position = Vector3.Lerp(startPos, endPos, eased);
            yield return null;
        }

        transform.position = endPos;
        onComplete?.Invoke();
    }
    
    public void PlaySplash()
    {
        if (splashParticle == null) return;
        
        
        splashParticle.transform.SetParent(null);
        splashParticle.transform.position = _surfacePosition;
        splashParticle.transform.localScale = Vector3.one * 2f;
        
        splashParticle.gameObject.SetActive(true);
        splashParticle.Stop();
        splashParticle.Play();
    }

    public void StopSplash()
    {
        if (splashParticle == null) return;
        splashParticle.Stop();
        splashParticle.gameObject.SetActive(false);
        
        splashParticle.transform.SetParent(transform);
        splashParticle.transform.localPosition = Vector3.zero;
    }

}