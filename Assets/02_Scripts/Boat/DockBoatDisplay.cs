using UnityEngine;

// Island 씬 선착장 빈 오브젝트에 부착
public class DockBoatDisplay : MonoBehaviour
{
    public static DockBoatDisplay Instance { get; private set; }

    [SerializeField] private BoatData  boatData;
    [SerializeField] private Transform spawnPoint; // 현재 보트 위치

    private GameObject _currentBoat;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        UpgradeSystem.Instance.OnUpgradeChanged += RefreshBoat;
        RefreshBoat();
    }

    private void OnDestroy()
    {
        if (UpgradeSystem.Instance != null)
            UpgradeSystem.Instance.OnUpgradeChanged -= RefreshBoat;
    }

    private void RefreshBoat()
    {
        int level = UpgradeSystem.Instance.GetCurrentLevel("보트");
        level = Mathf.Clamp(level, 0, boatData.levels.Length - 1);

        GameObject prefab = boatData.levels[level].islandPrefab;
        if (prefab == null) return;

        if (_currentBoat != null) Destroy(_currentBoat);
        _currentBoat = Instantiate(prefab, spawnPoint.position, prefab.transform.rotation, spawnPoint);
        
    }
}