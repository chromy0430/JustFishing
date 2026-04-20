using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// DontDestroyOnLoad Canvas에 부착
public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] private Image fadeImage; // 검정 Image (전체화면)
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 시작 시 투명하게
        fadeImage.color = new Color(0, 0, 0, 0);
    }

    // 연출 후 씬 전환 (배 연출 끝난 뒤 호출)
    public void TransitionToScene(string sceneName, float delayBeforeFade = 0f)
    {
        StartCoroutine(TransitionRoutine(sceneName, delayBeforeFade));
    }

    private IEnumerator TransitionRoutine(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 페이드 아웃
        yield return StartCoroutine(Fade(0f, 1f));

        yield return SceneManager.LoadSceneAsync(sceneName);

        // 페이드 인
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, to);
    }
}