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

    [Header("SFX")]
    public AudioClip sfxEnhance;
    public AudioClip sfxPurchase;
    public AudioClip sfxShipMoving;
    public AudioClip sfxSplash;

    [Header("볼륨 설정")]
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;
}