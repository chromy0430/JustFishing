using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioData audioData;

    [Header("BGM")]
    [SerializeField] private float crossFadeDuration = 1.5f; // 크로스페이드 시간

    [Header("SFX Pool")]
    [SerializeField] private int sfxPoolSize = 8; // 동시 재생 가능한 SFX 수

    // BGM용 AudioSource 2개 (크로스페이드)
    private AudioSource _bgmSourceA;
    private AudioSource _bgmSourceB;
    private bool        _isAActive = true;

    // SFX 풀
    private List<AudioSource> _sfxPool = new List<AudioSource>();

    // 현재 재생 중인 BGM 코루틴
    private Coroutine _bgmRoutine;
    private Coroutine _crossFadeRoutine;

    // ShipMoving SFX는 루프 재생이므로 별도 관리
    private AudioSource _shipMovingSource;
    private bool        _isShipMoving = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitAudioSources();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitAudioSources()
    {
        // BGM AudioSource 2개 생성
        _bgmSourceA = CreateAudioSource("BGM_A", true, audioData.bgmVolume);
        _bgmSourceB = CreateAudioSource("BGM_B", true, audioData.bgmVolume);
        _bgmSourceB.volume = 0f;

        // ShipMoving 전용 AudioSource
        _shipMovingSource = CreateAudioSource("SFX_ShipMoving", true, 0f);
        _shipMovingSource.clip = audioData.sfxShipMoving;

        // SFX 풀 생성
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource src = CreateAudioSource($"SFX_Pool_{i}", false, audioData.sfxVolume);
            _sfxPool.Add(src);
        }
    }

    private AudioSource CreateAudioSource(string name, bool loop, float volume)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(transform);

        AudioSource src = obj.AddComponent<AudioSource>();
        src.loop        = loop;
        src.volume      = volume;
        src.playOnAwake = false;

        return src;
    }

    // 씬 로드 시 BGM 자동 전환
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Island":
                PlayBGM_Lobby();
                StopShipMoving();
                break;
            case "Ocean":
                PlayBGM_Ocean();
                break;
        }
    }

    // ==================== BGM ====================

    public void PlayBGM_Lobby()
    {
        if (_bgmRoutine != null) StopCoroutine(_bgmRoutine);
        CrossFadeBGM(audioData.bgmLobby);
    }

    public void PlayBGM_Ocean()
    {
        if (_bgmRoutine != null) StopCoroutine(_bgmRoutine);
        _bgmRoutine = StartCoroutine(OceanBGMRoutine());
    }

    // Ocean BGM 01, 02 번갈아 재생
    private IEnumerator OceanBGMRoutine()
    {
        bool playFirst = true;
        while (true)
        {
            AudioClip clip = playFirst ? audioData.bgmOcean01 : audioData.bgmOcean02;
            CrossFadeBGM(clip);

            // 현재 클립 재생 시간만큼 대기 후 다음 클립으로
            yield return new WaitForSeconds(clip.length - crossFadeDuration);
            playFirst = !playFirst;
        }
    }

    private void CrossFadeBGM(AudioClip newClip)
    {
        if (_crossFadeRoutine != null) StopCoroutine(_crossFadeRoutine);
        _crossFadeRoutine = StartCoroutine(CrossFadeRoutine(newClip));
    }

    private IEnumerator CrossFadeRoutine(AudioClip newClip)
    {
        AudioSource fadeOut = _isAActive ? _bgmSourceA : _bgmSourceB;
        AudioSource fadeIn  = _isAActive ? _bgmSourceB : _bgmSourceA;

        fadeIn.clip   = newClip;
        fadeIn.volume = 0f;
        fadeIn.Play();

        float elapsed     = 0f;
        float startVolume = fadeOut.volume;

        while (elapsed < crossFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / crossFadeDuration;

            fadeOut.volume = Mathf.Lerp(startVolume, 0f, t);
            fadeIn.volume  = Mathf.Lerp(0f, audioData.bgmVolume, t);

            yield return null;
        }

        fadeOut.Stop();
        fadeOut.volume = 0f;
        fadeIn.volume  = audioData.bgmVolume;

        _isAActive = !_isAActive;
    }

    // ==================== SFX ====================

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        // 같은 클립이 이미 재생 중이면 중복 재생 방지
        foreach (AudioSource src in _sfxPool)
        {
            if (src.isPlaying && src.clip == clip) return;
        }

        // 풀에서 비어있는 AudioSource 찾기
        AudioSource available = GetAvailableSFXSource();
        if (available == null) return;

        available.clip   = clip;
        available.volume = audioData.sfxVolume;
        available.Play();
    }

    private AudioSource GetAvailableSFXSource()
    {
        // 재생 중이지 않은 Source 찾기
        foreach (AudioSource src in _sfxPool)
            if (!src.isPlaying) return src;

        // 모두 재생 중이면 null 반환 (무시)
        return null;
    }

    // ==================== 개별 SFX 메서드 ====================

    public void PlayEnhance()  => PlaySFX(audioData.sfxEnhance);
    public void PlayPurchase() => PlaySFX(audioData.sfxPurchase);
    public void PlaySplash()   => PlaySFX(audioData.sfxSplash);

    // ShipMoving은 루프 재생 + 페이드 처리
    public void StartShipMoving()
    {
        if (_isShipMoving) return;
        _isShipMoving = true;
        StartCoroutine(FadeShipMoving(0f, audioData.sfxVolume, 0.5f));
    }

    public void StopShipMoving()
    {
        if (!_isShipMoving) return;
        _isShipMoving = false;
        StartCoroutine(FadeShipMoving(_shipMovingSource.volume, 0f, 0.5f, stopOnComplete: true));
    }

    private IEnumerator FadeShipMoving(float from, float to, float duration, bool stopOnComplete = false)
    {
        if (!_shipMovingSource.isPlaying && to > 0f)
            _shipMovingSource.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _shipMovingSource.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        _shipMovingSource.volume = to;

        if (stopOnComplete)
            _shipMovingSource.Stop();
    }

    // ==================== 볼륨 조절 ====================

    public void SetBGMVolume(float volume)
    {
        audioData.bgmVolume = Mathf.Clamp01(volume);
        AudioSource active  = _isAActive ? _bgmSourceA : _bgmSourceB;
        active.volume       = audioData.bgmVolume;
    }

    public void SetSFXVolume(float volume)
    {
        audioData.sfxVolume = Mathf.Clamp01(volume);
        foreach (AudioSource src in _sfxPool)
            src.volume = audioData.sfxVolume;
    }
}