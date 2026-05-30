using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishManager : MonoBehaviour
{
    public static FishManager Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private FishZoneData          zoneData;
    [SerializeField] private WaterZoneController   waterZone;

    [Header("스폰 설정")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float     spawnRadius   = 15f;
    [SerializeField] private float     despawnRadius = 20f;
    [SerializeField] private float     despawnDelay  = 5f;
    [SerializeField] private int       maxFishCount  = 30;
    [SerializeField] private float     spawnInterval = 2f;
    [SerializeField] private float     spawnHeight   = -0.3f;

    [Header("Boids 설정")]
    [SerializeField] private float perceptionRadius  = 4f;    // 주변 인식 반경
    [SerializeField] private float separationWeight  = 1.5f;  // 분리 강도
    [SerializeField] private float alignmentWeight   = 1.0f;  // 정렬 강도
    [SerializeField] private float cohesionWeight    = 1.0f;  // 응집 강도
    [SerializeField] private float wanderWeight      = 0.8f;  // 배회 강도
    [SerializeField] private float boundaryWeight    = 2.0f;  // 경계 복귀 강도
    [SerializeField] private float maxSpeed          = 2.5f;  // 최대 속도
    [SerializeField] private float maxForce          = 3f;    // 최대 가속도
    [SerializeField] private float smoothRotation    = 5f;    // 회전 부드러움

    private List<FishAgent>                    _agents      = new List<FishAgent>();
    private Dictionary<FishAgent, float>       _outsideTimer = new Dictionary<FishAgent, float>();
    private Dictionary<FishAgent, Vector3>     _wanderTarget = new Dictionary<FishAgent, Vector3>();
    private float                              _wanderChangeTimer = 0f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (playerTransform == null)
            playerTransform = GameObject.FindWithTag("Player").transform;

        StartCoroutine(SpawnRoutine());
        StartCoroutine(DespawnRoutine());
    }

    private void Update()
    {
        _wanderChangeTimer -= Time.deltaTime;
        bool changeWander = _wanderChangeTimer <= 0f;
        if (changeWander) _wanderChangeTimer = Random.Range(2f, 4f);

        // 중앙 매니저에서 모든 물고기 Update 처리
        for (int i = 0; i < _agents.Count; i++)
        {
            FishAgent agent = _agents[i];
            if (agent == null) continue;

            Vector3 force = CalculateBoids(agent, changeWander);

            // 가속도 적용
            agent.acceleration = Vector3.ClampMagnitude(force, maxForce);
            agent.velocity    += agent.acceleration * Time.deltaTime;

            // Y축 속도 제거 (수평 이동만)
            agent.velocity.y   = 0f;
            agent.velocity     = Vector3.ClampMagnitude(agent.velocity, maxSpeed);

            // 최소 속도 유지 (멈추지 않도록)
            if (agent.velocity.magnitude < 0.5f)
            {
                Vector2 rand   = Random.insideUnitCircle.normalized;
                agent.velocity = new Vector3(rand.x, 0f, rand.y) * 0.5f;
            }

            // 위치 이동
            agent.transform.position += agent.velocity * Time.deltaTime;

            // 수면 높이 고정
            Vector3 pos = agent.transform.position;
            pos.y = spawnHeight;
            agent.transform.position = pos;

            // 부드러운 회전
            if (agent.velocity.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(agent.velocity.normalized);
                agent.transform.rotation = Quaternion.Slerp(
                    agent.transform.rotation,
                    targetRot,
                    smoothRotation * Time.deltaTime
                );
            }
        }
    }

    private Vector3 CalculateBoids(FishAgent agent, bool changeWander)
    {
        Vector3 separation  = Vector3.zero;
        Vector3 alignment   = Vector3.zero;
        Vector3 cohesion    = Vector3.zero;
        int     neighborCount = 0;

        for (int i = 0; i < _agents.Count; i++)
        {
            FishAgent other = _agents[i];
            if (other == null || other == agent) continue;

            float dist = Vector3.Distance(agent.transform.position, other.transform.position);
            if (dist > perceptionRadius) continue;

            // 분리: 가까울수록 강하게 밀어냄
            Vector3 diff = agent.transform.position - other.transform.position;
            separation  += diff.normalized / Mathf.Max(dist, 0.01f);

            // 정렬: 주변 속도 방향 평균
            alignment   += other.velocity;

            // 응집: 주변 위치 평균
            cohesion    += other.transform.position;

            neighborCount++;
        }

        Vector3 force = Vector3.zero;

        if (neighborCount > 0)
        {
            // 분리
            separation /= neighborCount;
            force      += separation.normalized * separationWeight;

            // 정렬
            alignment  /= neighborCount;
            force      += (alignment.normalized * maxSpeed - agent.velocity) * alignmentWeight;

            // 응집
            cohesion   /= neighborCount;
            Vector3 cohesionDir = (cohesion - agent.transform.position).normalized;
            force      += cohesionDir * cohesionWeight;
        }

        // 배회: 랜덤 목표 지점으로 이동
        force += CalculateWander(agent, changeWander) * wanderWeight;

        // 경계 복귀: 스폰 중심 밖으로 너무 나가면 돌아옴
        force += CalculateBoundary(agent) * boundaryWeight;

        return force;
    }

    private Vector3 CalculateWander(FishAgent agent, bool changeTarget)
    {
        if (!_wanderTarget.ContainsKey(agent) || changeTarget)
        {
            // 스폰 중심 기준 랜덤 목표
            Vector2 rand          = Random.insideUnitCircle * agent.wanderRadius;
            _wanderTarget[agent]  = agent.spawnCenter + new Vector3(rand.x, 0f, rand.y);
        }

        Vector3 dir = (_wanderTarget[agent] - agent.transform.position);
        dir.y = 0f;
        return dir.magnitude > 0.1f ? dir.normalized : Vector3.zero;
    }

    private Vector3 CalculateBoundary(FishAgent agent)
    {
        Vector3 offset = agent.transform.position - agent.spawnCenter;
        offset.y = 0f;

        // 배회 반경 밖으로 나가면 중심으로 복귀
        if (offset.magnitude > agent.wanderRadius)
            return -offset.normalized * (offset.magnitude - agent.wanderRadius);

        return Vector3.zero;
    }

    // 외부에서 물고기 등록 (FishSpawner에서 호출)
    public void RegisterFish(FishAgent agent)
    {
        if (!_agents.Contains(agent))
            _agents.Add(agent);
    }

    public void UnregisterFish(FishAgent agent)
    {
        _agents.Remove(agent);
        _wanderTarget.Remove(agent);
        _outsideTimer.Remove(agent);
    }

    // Zone 판별
    private int GetCurrentZone()
    {
        float dist = Vector2.Distance(
            new Vector2(waterZone.island.position.x, waterZone.island.position.z),
            new Vector2(playerTransform.position.x,  playerTransform.position.z)
        );

        if (dist < waterZone.zone1Distance) return 1;
        if (dist < waterZone.zone2Distance) return 2;
        return 3;
    }

    private GameObject[] GetZonePrefabs(int zone)
    {
        return zone switch
        {
            1 => zoneData.zone1FishPrefabs,
            2 => zoneData.zone2FishPrefabs,
            _ => zoneData.zone3FishPrefabs,
        };
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (_agents.Count >= maxFishCount) continue;

            int          zone    = GetCurrentZone();
            GameObject[] prefabs = GetZonePrefabs(zone);
            if (prefabs == null || prefabs.Length == 0) continue;

            Vector2    rand     = Random.insideUnitCircle * spawnRadius;
            Vector3    spawnPos = playerTransform.position
                                + new Vector3(rand.x, spawnHeight, rand.y);

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            GameObject fish   = Instantiate(prefab, spawnPos, Quaternion.identity);

            FishAgent agent = fish.GetComponent<FishAgent>();
            if (agent == null) agent = fish.AddComponent<FishAgent>();

            agent.Init(spawnPos, zone);
            RegisterFish(agent);
        }
    }

    private IEnumerator DespawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            List<FishAgent> toRemove = new List<FishAgent>();

            foreach (FishAgent agent in _agents)
            {
                if (agent == null) { toRemove.Add(agent); continue; }

                float dist = Vector3.Distance(
                    new Vector3(agent.transform.position.x, 0f, agent.transform.position.z),
                    new Vector3(playerTransform.position.x, 0f, playerTransform.position.z)
                );

                if (dist > despawnRadius)
                {
                    if (!_outsideTimer.ContainsKey(agent))
                        _outsideTimer[agent] = 0f;

                    _outsideTimer[agent] += 1f;

                    if (_outsideTimer[agent] >= despawnDelay)
                    {
                        toRemove.Add(agent);
                        Destroy(agent.gameObject);
                    }
                }
                else
                {
                    if (_outsideTimer.ContainsKey(agent))
                        _outsideTimer.Remove(agent);
                }
            }

            foreach (FishAgent fa in toRemove)
                UnregisterFish(fa);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;

        Gizmos.color = Color.cyan;
        DrawCircle(playerTransform.position, spawnRadius);

        Gizmos.color = Color.red;
        DrawCircle(playerTransform.position, despawnRadius);

        Gizmos.color = Color.yellow;
        DrawCircle(playerTransform.position, perceptionRadius);
    }

    private void DrawCircle(Vector3 center, float radius)
    {
        int     seg  = 32;
        Vector3 prev = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= seg; i++)
        {
            float   a    = i * (360f / seg) * Mathf.Deg2Rad;
            Vector3 next = center + new Vector3(
                Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}