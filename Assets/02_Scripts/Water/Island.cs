using UnityEngine;

[ExecuteAlways]
public class WaterZoneController : MonoBehaviour
{
    [Header("References")]
    public Transform island;
    public Renderer waterRenderer; // Center 오브젝트

    // 없으면 자동으로 MainCamera 사용
    public Transform followTarget;

    [Header("Zone Distances")]
    public float zone1Distance = 20f;
    public float zone2Distance = 40f;
    public float zone3Distance = 60f;

    [Header("Base Colors (Deep)")]
    public Color zone1BaseColor = new Color(0.30f, 0.60f, 1.0f, 1f);
    public Color zone2BaseColor = new Color(0.10f, 0.30f, 0.8f, 1f);
    public Color zone3BaseColor = new Color(0.05f, 0.10f, 0.4f, 1f);

    [Header("Shallow Colors")]
    public Color zone1ShallowColor = new Color(0.40f, 0.80f, 1.0f, 1f);
    public Color zone2ShallowColor = new Color(0.20f, 0.50f, 0.9f, 1f);
    public Color zone3ShallowColor = new Color(0.10f, 0.20f, 0.6f, 1f);

    // StylizedWater3_Standard.watershader3 셰이더 파일에서 변경
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ShallowColorID = Shader.PropertyToID("_ShallowColor");

    Transform GetFollowTarget()
    {
        if (followTarget != null) return followTarget;
        var cam = Camera.main;
        return cam != null ? cam.transform : null;
    }

    void Update()
    {
        if (island == null || waterRenderer == null) return;

        Transform target = GetFollowTarget();
        if (target == null) return;

        // ✅ 카메라(또는 플레이어) ↔ 섬 사이의 XZ 거리
        float dist = Vector2.Distance(
            new Vector2(island.position.x, island.position.z),
            new Vector2(target.position.x, target.position.z)
        );

        float t1 = Mathf.InverseLerp(zone1Distance, zone2Distance, dist);
        float t2 = Mathf.InverseLerp(zone2Distance, zone3Distance, dist);

        Color baseCol = Color.Lerp(
            Color.Lerp(zone1BaseColor, zone2BaseColor, t1),
            zone3BaseColor, t2);

        Color shallowCol = Color.Lerp(
            Color.Lerp(zone1ShallowColor, zone2ShallowColor, t1),
            zone3ShallowColor, t2);

        waterRenderer.sharedMaterial.SetColor(BaseColorID, baseCol);
        waterRenderer.sharedMaterial.SetColor(ShallowColorID, shallowCol);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (island == null) return;
        Vector3 c = island.position;

        Gizmos.color = new Color(0.3f, 0.9f, 0.3f, 0.5f);
        DrawCircle(c, zone1Distance);

        Gizmos.color = new Color(0.1f, 0.3f, 0.9f, 0.5f);
        DrawCircle(c, zone2Distance);

        Gizmos.color = new Color(0.05f, 0.1f, 0.5f, 0.5f);
        DrawCircle(c, zone3Distance);
    }

    void DrawCircle(Vector3 center, float radius)
    {
        int seg = 64;
        Vector3 prev = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= seg; i++)
        {
            float a = i * (360f / seg) * Mathf.Deg2Rad;
            Vector3 next = center + new Vector3(
                Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
#endif
}