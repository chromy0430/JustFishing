using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "JustFishing/Audio Data")]
public class AudioData : ScriptableObject
{
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