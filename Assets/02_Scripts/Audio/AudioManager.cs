using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioData audioData;

    [Header("BGM")]
    [SerializeField] private float crossFadeDuration = 1.5f;

    [Header("SFX Pool")]
    [SerializeField] private int sfxPoolSize = 8;

    private AudioSource _bgmSourceA;
    private AudioSource _bgmSourceB;
    private bool        _isAActive = true;

    private List<AudioSource> _sfxPool       = new List<AudioSource>();
    private AudioSource       _shipMovingSource;
    private bool              _isShipMoving  = false;

    private Coroutine _bgmRoutine;
    private Coroutine _crossFadeRoutine;
    private Coroutine _shipFadeRoutine;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitAudioSources();

        // 볼륨 초기값 적용
        SetBGMVolume(audioData.bgmVolume);
        SetSFXVolume(audioData.sfxVolume);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // 첫 씬 BGM 시작 (Awake 이후 확실히 실행)
        PlayBGMForCurrentScene();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitAudioSources()
    {
        // BGM AudioSource 2개
        _bgmSourceA = CreateAudioSource("BGM_A", true,  audioData.bgmMixerGroup);
        _bgmSourceB = CreateAudioSource("BGM_B", true,  audioData.bgmMixerGroup);
        _bgmSourceB.volume = 0f;

        // ShipMoving 전용
        _shipMovingSource      = CreateAudioSource("SFX_ShipMoving", true, audioData.sfxMixerGroup);
        _shipMovingSource.clip = audioData.sfxShipMoving;
        _shipMovingSource.volume = 0f;

        // SFX 풀
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource src = CreateAudioSource($"SFX_Pool_{i}", false, audioData.sfxMixerGroup);
            _sfxPool.Add(src);
        }
    }

    private AudioSource CreateAudioSource(string name, bool loop, AudioMixerGroup mixerGroup)
    {
        GameObject obj        = new GameObject(name);
        obj.transform.SetParent(transform);

        AudioSource src       = obj.AddComponent<AudioSource>();
        src.loop              = loop;
        src.playOnAwake       = false;
        src.outputAudioMixerGroup = mixerGroup;

        return src;
    }

    // ==================== 씬 전환 ====================

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForCurrentScene();
        ForceStopShipMoving();
    }

    private void PlayBGMForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        switch (sceneName)
        {
            case "Start":
                PlayBGM_MainMenu();
                break;
            case "Island":
                PlayBGM_Lobby();
                break;
            case "Ocean":
                PlayBGM_Ocean();
                break;
        }
    }

    // ==================== BGM ====================
    
    public void PlayBGM_MainMenu()
    {
        if (_bgmRoutine != null) StopCoroutine(_bgmRoutine);

        // BGM 2개면 번갈아, 1개면 루프
        if (audioData.bgmMainMenu02 != null)
            _bgmRoutine = StartCoroutine(MainMenuBGMRoutine());
        else
            CrossFadeBGM(audioData.bgmMainMenu01);
    }

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

    private IEnumerator OceanBGMRoutine()
    {
        bool playFirst = true;
        while (true)
        {
            AudioClip clip = playFirst ? audioData.bgmOcean01 : audioData.bgmOcean02;
            CrossFadeBGM(clip);
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
            elapsed       += Time.deltaTime;
            float t        = elapsed / crossFadeDuration;
            fadeOut.volume = Mathf.Lerp(startVolume, 0f, t);
            fadeIn.volume  = Mathf.Lerp(0f, 1f, t); // Mixer가 볼륨 담당
            yield return null;
        }

        fadeOut.Stop();
        fadeOut.volume = 0f;
        fadeIn.volume  = 1f;
        _isAActive     = !_isAActive;
    }
    
    private IEnumerator MainMenuBGMRoutine()
    {
        bool playFirst = true;
        while (true)
        {
            AudioClip clip = playFirst
                ? audioData.bgmMainMenu01
                : audioData.bgmMainMenu02;
            CrossFadeBGM(clip);
            yield return new WaitForSeconds(clip.length - crossFadeDuration);
            playFirst = !playFirst;
        }
    }

    // ==================== SFX ====================

    public void PlaySFX(AudioClip clip, bool allowOverlap = false)
    {
        if (clip == null) return;

        // 중복 재생 방지 (allowOverlap = false일 때)
        if (!allowOverlap)
        {
            foreach (AudioSource src in _sfxPool)
                if (src.isPlaying && src.clip == clip) return;
        }

        AudioSource available = GetAvailableSFXSource();
        if (available == null) return;

        available.clip   = clip;
        available.volume = 1f; // Mixer가 볼륨 담당
        available.Play();
    }

    private AudioSource GetAvailableSFXSource()
    {
        foreach (AudioSource src in _sfxPool)
            if (!src.isPlaying) return src;
        return null;
    }
    
    public void PlayUIClick()
    {
        PlaySFX(audioData.sfxUIClick, allowOverlap: false);
    }

    // ==================== 개별 SFX ====================

    public void PlayEnhance()  => PlaySFX(audioData.sfxEnhance);
    public void PlayPurchase() => PlaySFX(audioData.sfxPurchase);

    // 찌 착수 - 딜레이 없이 즉시 재생
    public void PlaySplash()   => PlaySFX(audioData.sfxSplash, allowOverlap: true);
    
    public void PlayJudgement(NoteJudgement judgement)
    {
        AudioClip clip = judgement switch
        {
            NoteJudgement.Perfect => audioData.sfxPerfect,
            NoteJudgement.Good    => audioData.sfxGood,
            NoteJudgement.Bad    => audioData.sfxMiss,
            NoteJudgement.Miss   => audioData.sfxMiss,
            _                     => audioData.sfxMiss
        };
        PlaySFX(clip, allowOverlap: false);
    }

    // ==================== ShipMoving ====================

    public void StartShipMoving()
    {
        if (_isShipMoving) return;
        _isShipMoving = true;

        if (_shipFadeRoutine != null) StopCoroutine(_shipFadeRoutine);

        if (!_shipMovingSource.isPlaying)
            _shipMovingSource.Play();

        _shipFadeRoutine = StartCoroutine(FadeShipMoving(
            _shipMovingSource.volume, 1f, 0.3f)); // 0.5 → 0.3으로 단축
    }

    public void StopShipMoving()
    {
        if (!_isShipMoving) return;
        _isShipMoving = false;

        if (_shipFadeRoutine != null) StopCoroutine(_shipFadeRoutine);
        _shipFadeRoutine = StartCoroutine(FadeShipMoving(
            _shipMovingSource.volume, 0f, 0.3f, stopOnComplete: true));
    }

    // 씬 전환 또는 모드 전환 시 즉시 정지
    public void ForceStopShipMoving()
    {
        _isShipMoving = false;
        if (_shipFadeRoutine != null) StopCoroutine(_shipFadeRoutine);
        _shipMovingSource.volume = 0f;
        _shipMovingSource.Stop();
    }

    private IEnumerator FadeShipMoving(float from, float to,
        float duration, bool stopOnComplete = false)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _shipMovingSource.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        _shipMovingSource.volume = to;
        if (stopOnComplete) _shipMovingSource.Stop();
    }

    // ==================== 볼륨 조절 (UI 연동용) ====================

    // AudioMixer는 dB 단위 사용 (-80 ~ 0)
    // 0~1 float을 dB로 변환해서 Mixer에 적용
    public void SetBGMVolume(float volume)
    {
        audioData.bgmVolume = Mathf.Clamp01(volume);
        float db = volume > 0f ? Mathf.Log10(volume) * 20f : -80f;
        audioData.audioMixer.SetFloat("BGMVolume", db);
    }

    public void SetSFXVolume(float volume)
    {
        audioData.sfxVolume = Mathf.Clamp01(volume);
        float db = volume > 0f ? Mathf.Log10(volume) * 20f : -80f;
        audioData.audioMixer.SetFloat("SFXVolume", db);
    }

    // 현재 볼륨 반환 (UI Slider 초기값 설정용)
    public float GetBGMVolume() => audioData.bgmVolume;
    public float GetSFXVolume() => audioData.sfxVolume;
}