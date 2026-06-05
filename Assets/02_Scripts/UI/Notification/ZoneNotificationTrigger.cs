using UnityEngine;

public class ZoneNotificationTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WaterZoneController waterZone;

    [Header("알람 데이터")]
    [SerializeField] private NotificationData zone1Notification; // 연안 지역
    [SerializeField] private NotificationData zone2Notification; // 심해 지역
    [SerializeField] private NotificationData zone3Notification; // 마력해역

    [Header("Ocean 씬 진입 알람")]
    [SerializeField] private NotificationData oceanEnterNotification;

    private int _lastZone = -1; // 이전 지역 (-1 = 초기값)

    private void Start()
    {
        // Ocean 씬 진입 알람
        if (oceanEnterNotification != null)
            NotificationManager.Instance?.Show(oceanEnterNotification);
    }

    private void Update()
    {
        if (waterZone == null) return;

        int currentZone = GetCurrentZone();
        if (currentZone == _lastZone) return;

        // 초기 진입 시엔 알람 생략 (Start에서 처리)
        if (_lastZone != -1)
            ShowZoneNotification(currentZone);

        _lastZone = currentZone;
    }

    private int GetCurrentZone()
    {
        Transform target = waterZone.followTarget != null
            ? waterZone.followTarget
            : (Camera.main != null ? Camera.main.transform : null);

        if (target == null || waterZone.island == null) return 1;

        float dist = Vector2.Distance(
            new Vector2(waterZone.island.position.x, waterZone.island.position.z),
            new Vector2(target.position.x, target.position.z)
        );

        if (dist < waterZone.zone1Distance) return 1;
        if (dist < waterZone.zone2Distance) return 2;
        return 3;
    }

    private void ShowZoneNotification(int zone)
    {
        NotificationData data = zone switch
        {
            1 => zone1Notification,
            2 => zone2Notification,
            _ => zone3Notification,
        };

        if (data != null)
            NotificationManager.Instance?.Show(data);
    }
}