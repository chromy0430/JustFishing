using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "AudioData", menuName = "JustFishing/Audio Data")]
public class AudioData : ScriptableObject
{
    [Header("Audio Mixer")]
    public AudioMixer     audioMixer;         // GameAudioMixer 연결
    public AudioMixerGroup bgmMixerGroup;     // BGM 그룹
    public AudioMixerGroup sfxMixerGroup;     // SFX 그룹
    
    [Header("BGM")]
    public AudioClip bgmLobby;
    public AudioClip bgmOcean01;
    public AudioClip bgmOcean02;
    public AudioClip bgmMainMenu01; // 추가
    public AudioClip bgmMainMenu02; // 추가 (없으면 null)

    [Header("SFX")]
    public AudioClip sfxEnhance;
    public AudioClip sfxPurchase;
    public AudioClip sfxShipMoving;
    public AudioClip sfxSplash;
    public AudioClip sfxUIClick;

    [Header("미니게임 판정 SFX")]
    public AudioClip sfxPerfect;  // 임팩트 있는 소리
    public AudioClip sfxGood;     // 평범한 타악기
    public AudioClip sfxMiss;     // 삐삑
    
    [Header("볼륨 설정")]
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;
}