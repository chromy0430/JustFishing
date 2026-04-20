using System;
using System.Collections;
using UnityEngine;
using StylizedWater3;

[RequireComponent(typeof(Collider))]
public class FishingBobber : MonoBehaviour
{
    [SerializeField] private LayerMask waterLayer;

    private Coroutine _currentRoutine;
    private AlignToWater _alignToWater;
    private Action _onLandedCallback;
    private bool _hasLanded = false;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _alignToWater = GetComponent<AlignToWater>();

        if (_alignToWater != null) _alignToWater.enabled = false;
        Hide();
    }

    public void Cast(Vector3 from, Vector3 to, FishingData data, Action onLanded)
    {
        _hasLanded = false;
        _onLandedCallback = onLanded;

        if (_rb != null) _rb.isKinematic = true;
        if (_alignToWater != null) _alignToWater.enabled = false;

        gameObject.SetActive(true);
        if (_currentRoutine != null) StopCoroutine(_currentRoutine);
        _currentRoutine = StartCoroutine(CastRoutine(from, to, data, onLanded));
    }

    // onLanded는 OnTriggerEnter에서 처리하므로 콜백 제거
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

        transform.position = to;

        if (_rb != null) _rb.isKinematic = false;
        if (_alignToWater != null) _alignToWater.enabled = true;

        onLanded?.Invoke();
    }

    private IEnumerator WaitRoutine(FishingData data, Action onBite)
    {
        float waitTime = UnityEngine.Random.Range(data.waitMinTime, data.waitMaxTime);
        yield return new WaitForSeconds(waitTime);

        // 입질 시 AlignToWater 비활성 후 찌 내려가는 연출
        if (_alignToWater != null)
            _alignToWater.enabled = false;

        yield return StartCoroutine(BiteRoutine(data));
        onBite?.Invoke();
    }

    private IEnumerator BiteRoutine(FishingData data)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - new Vector3(0f, data.biteDepth, 0f);

        float elapsed = 0f;
        while (elapsed < data.biteDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / data.biteDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
    }

}