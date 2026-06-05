using UnityEngine;

[CreateAssetMenu(fileName = "NotificationData", menuName = "JustFishing/Notification Data")]
public class NotificationData : ScriptableObject
{
    [SerializeField, TextArea] private string _message;
    [SerializeField]           private float  _displayDuration = 2f;
    [SerializeField]           private float  _fadeDuration    = 0.5f;

    public string Message         => _message;
    public float  DisplayDuration => _displayDuration;
    public float  FadeDuration    => _fadeDuration;
}