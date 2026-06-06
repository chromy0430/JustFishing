using UnityEngine;

// Ocean 씬의 빈 오브젝트에 부착
public class BoatSpawner : MonoBehaviour
{
    [SerializeField] private WaterZoneController waterZone;
    [SerializeField] private BoatData boatData;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform playerSpawnPos;
    // BoatSpawner.cs
    private void Start()
    {
        int level = PlayerPrefs.GetInt("Upgrade_보트", 0);
        level = Mathf.Clamp(level, 0, boatData.levels.Length - 1);
        BoatLevelData levelData = boatData.levels[level];

        GameObject boat = Instantiate(
            levelData.oceanPrefab,
            spawnPoint.position,
            spawnPoint.rotation);

        // BoatController에 레벨 데이터 주입
        BoatController boatCtrl = boat.GetComponent<BoatController>();
        boatCtrl?.SetLevelData(levelData);

        // BoatDurability에 레벨 데이터 주입
        BoatDurability durability = boat.GetComponent<BoatDurability>();
        durability?.SetLevelData(levelData);
        durability?.GetWaterZone(waterZone);

        // 이하 기존 코드
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) { Debug.LogError("Player 없음"); return; }

        player.transform.SetParent(boat.transform);
        player.transform.localPosition = new Vector3(0f, 1f, 0f);

        if (player.TryGetComponent<CharacterController>(out var cc)) cc.enabled = false;
        if (player.TryGetComponent<PlayerMove>(out var pm)) pm.enabled = false;

        boatCtrl?.Init(player.transform);

        CameraStabilizer stabilizer = FindFirstObjectByType<CameraStabilizer>();
        if (stabilizer != null) stabilizer.SetTarget(boat.transform);

        var vcam = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (vcam != null && stabilizer != null)
            vcam.Target.TrackingTarget = stabilizer.transform;

        FishingController fishingCtrl = FindFirstObjectByType<FishingController>();
        fishingCtrl?.SetCamera(Camera.main);

        PlayerAnimator playerAnim = player.GetComponentInChildren<PlayerAnimator>();
        playerAnim?.SetOnBoat(true);
        fishingCtrl?.SetPlayerAnimator(playerAnim);

        if (player.TryGetComponent<PlayerModeController>(out var modeCtrl))
            modeCtrl.OnBoardBoat(boatCtrl, fishingCtrl);
    }
}