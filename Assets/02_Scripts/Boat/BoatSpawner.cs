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
        GameObject boat = Instantiate(boatData.boatPrefab, spawnPoint.position, spawnPoint.rotation);

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) { Debug.LogError("Player 없음"); return; }

        player.transform.SetParent(boat.transform);
        player.transform.localPosition = new Vector3(0f, 1f, 0f);
        player.transform.localRotation = Quaternion.identity;

        if (player.TryGetComponent<CharacterController>(out var cc)) cc.enabled = false;
        if (player.TryGetComponent<PlayerMove>(out var pm)) pm.enabled = false;

        BoatController boatCtrl = boat.GetComponent<BoatController>();
        boatCtrl?.Init(player.transform);

        // PlayerAnimator 찾기 (자식 오브젝트인 YellowHuman_01에 있음)
        PlayerAnimator playerAnim = player.GetComponentInChildren<PlayerAnimator>();
        if (playerAnim != null)
            playerAnim.SetOnBoat(true);

        // CameraStabilizer + Cinemachine 연결
        CameraStabilizer stabilizer = FindFirstObjectByType<CameraStabilizer>();
        if (stabilizer != null) stabilizer.SetTarget(boat.transform);

        var vcam = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (vcam != null && stabilizer != null)
            vcam.Target.TrackingTarget = stabilizer.transform;

        // FishingController 참조 + 주입
        FishingController fishingCtrl = FindFirstObjectByType<FishingController>();
        if (fishingCtrl != null)
        {
            fishingCtrl.SetCamera(Camera.main);
            fishingCtrl.SetPlayerAnimator(playerAnim); // PlayerAnimator 주입
        }

        // PlayerModeController에 보트 + 낚시 컨트롤러 주입
        if (player.TryGetComponent<PlayerModeController>(out var modeCtrl))
            modeCtrl.OnBoardBoat(boatCtrl, fishingCtrl);
        
        BoatDurability durability = boat.GetComponent<BoatDurability>();
        if (durability == null)
            durability = boat.AddComponent<BoatDurability>();
        durability.GetWaterZone(waterZone);
        
    }
}