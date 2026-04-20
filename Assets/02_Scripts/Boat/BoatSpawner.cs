using UnityEngine;
using Unity.Cinemachine;

// Ocean 씬의 빈 오브젝트에 부착
public class BoatSpawner : MonoBehaviour
{
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

        if (player.TryGetComponent<CharacterController>(out var cc)) cc.enabled = false;
        if (player.TryGetComponent<PlayerMove>(out var pm)) pm.enabled = false;

        BoatController boatCtrl = boat.GetComponent<BoatController>();
        boatCtrl?.Init(player.transform);

        // CameraStabilizer + Cinemachine 연결
        CameraStabilizer stabilizer = FindFirstObjectByType<CameraStabilizer>();
        if (stabilizer != null) stabilizer.SetTarget(boat.transform);

        var vcam = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (vcam != null && stabilizer != null)
            vcam.Target.TrackingTarget = stabilizer.transform;

        // FishingController 참조
        FishingController fishingCtrl = FindFirstObjectByType<FishingController>();

        // FishingController에 카메라 주입
        fishingCtrl?.SetCamera(Camera.main);

        // PlayerModeController에 보트 + 낚시 컨트롤러 주입
        // Start가 안 불리므로 BoatSpawner에서 직접 전달
        if (player.TryGetComponent<PlayerModeController>(out var modeCtrl))
            modeCtrl.OnBoardBoat(boatCtrl, fishingCtrl);
    }
}