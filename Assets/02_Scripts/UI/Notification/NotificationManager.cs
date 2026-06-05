using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [SerializeField] private GameObject      notificationParent;
    [SerializeField] private TextMeshProUGUI notificationTextUI;

    private CanvasGroup              _canvasGroup;
    private Queue<NotificationData>  _queue       = new Queue<NotificationData>();
    private bool                     _isDisplaying = false;
    private Coroutine                _displayRoutine;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _canvasGroup       = notificationParent.GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        notificationParent.SetActive(false);
    }

    // 외부에서 호출
    public void Show(NotificationData data)
    {
        if (data == null) return;

        _queue.Enqueue(data);

        if (!_isDisplaying)
            _displayRoutine = StartCoroutine(DisplayRoutine());
    }

    // 여러 개 한번에 큐에 쌓기
    public void ShowMultiple(IEnumerable<NotificationData> dataList)
    {
        foreach (NotificationData data in dataList)
            if (data != null) _queue.Enqueue(data);

        if (!_isDisplaying)
            _displayRoutine = StartCoroutine(DisplayRoutine());
    }

    // 큐 전체 즉시 취소
    public void ClearQueue()
    {
        _queue.Clear();
        if (_displayRoutine != null)
        {
            StopCoroutine(_displayRoutine);
            _displayRoutine = null;
        }
        _isDisplaying      = false;
        _canvasGroup.alpha = 0f;
        notificationParent.SetActive(false);
    }

    private IEnumerator DisplayRoutine()
    {
        _isDisplaying = true;

        while (_queue.Count > 0)
        {
            NotificationData data = _queue.Dequeue();

            notificationTextUI.text = data.Message;
            notificationParent.SetActive(true);

            // 페이드 인
            yield return StartCoroutine(
                FadeCanvasGroup(_canvasGroup, fadeIn: true, data.FadeDuration));

            // 표시 유지
            yield return new WaitForSeconds(data.DisplayDuration);

            // 페이드 아웃
            yield return StartCoroutine(
                FadeCanvasGroup(_canvasGroup, fadeIn: false, data.FadeDuration));

            // 다음 알람 전 짧은 간격
            yield return new WaitForSeconds(0.2f);
        }

        notificationParent.SetActive(false);
        _isDisplaying = false;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, bool fadeIn, float duration)
    {
        float startAlpha  = cg.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;
        float elapsed     = 0f;

        while (elapsed < duration)
        {
            elapsed      += Time.deltaTime;
            cg.alpha      = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        // while 종료 후 정확한 값으로 보정
        cg.alpha = targetAlpha;
    }
    
    public void ShowMessage(string message, float displayDuration = 2f, float fadeDuration = 0.5f)
    {
        // 런타임 임시 데이터로 처리
        StartCoroutine(ShowMessageRoutine(message, displayDuration, fadeDuration));
    }
    
    private IEnumerator ShowMessageRoutine(string message, float displayDuration, float fadeDuration)
    {
        // 현재 표시 중이면 대기
        while (_isDisplaying)
            yield return null;

        _isDisplaying           = true;
        notificationTextUI.text = message;
        notificationParent.SetActive(true);

        yield return StartCoroutine(FadeCanvasGroup(_canvasGroup, true,  fadeDuration));
        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(FadeCanvasGroup(_canvasGroup, false, fadeDuration));

        notificationParent.SetActive(false);
        _isDisplaying = false;

        // 큐에 남은 것 처리
        if (_queue.Count > 0)
            _displayRoutine = StartCoroutine(DisplayRoutine());
    }
}