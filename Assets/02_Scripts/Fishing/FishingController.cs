using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class FishingController : MonoBehaviour
{
    public enum FishingState { Idle, Aiming, Casting, Waiting, Biting, Minigame, Result }

    [Header("Data")]
    [SerializeField] private FishingData fishingData;
    [SerializeField] private PlayerInputData inputData;

    [Header("Prefabs")]
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private GameObject bobberPrefab;


    [Header("References")]
    [SerializeField] private FishingMinigame minigame;

    private Camera _mainCamera;
    private FishingIndicator _indicator;
    private FishingBobber _bobber;
    private Transform _playerTransform;

    [Header("Fish Data (임시 - 나중에 물고기 시스템으로 교체)")]
    [SerializeField] private FishData[] fishDataList; // 테스트용 물고기 목록

    private FishData _currentFishData;

    public FishingState CurrentState { get; private set; } = FishingState.Idle;

    private void Start()
    {
        // Player를 태그로 찾아서 참조
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            _playerTransform = player.transform;
        else
            Debug.LogError("Player를 찾을 수 없습니다");
    }

    // BoatSpawner에서 호출
    public void SetCamera(Camera cam)
    {
        if (cam != null) _mainCamera = cam;
    }

    private void Update()
    {
        // 카메라 null 방어
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        switch (CurrentState)
        {
            case FishingState.Aiming: HandleAiming(); break;
            case FishingState.Biting: HandleBiting(); break;
        }
    }

    // PlayerModeController에서 R키로 낚시 모드 전환 시 호출
    public void EnterFishingMode()
    {
        inputData.ConsumeJump();

        // 프리팹 생성
        GameObject indicatorObj = Instantiate(indicatorPrefab);
        GameObject bobberObj = Instantiate(bobberPrefab);

        // 컴포넌트 가져오기
        _indicator = indicatorObj.GetComponent<FishingIndicator>();
        _bobber = bobberObj.GetComponent<FishingBobber>();

        // 시작 시 비활성화
        _indicator.Hide();
        _bobber.Hide();

        SetState(FishingState.Aiming);
    }

    public void ExitFishingMode()
    {
        if (_indicator != null) Destroy(_indicator.gameObject);
        if (_bobber != null) Destroy(_bobber.gameObject);

        SetState(FishingState.Idle);
    }

    private void HandleAiming()
    {
        if (CurrentState == FishingState.Minigame) return;
        if (_mainCamera == null || _playerTransform == null) return;

        if (Mouse.current == null) return;
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mouseScreenPos);

        float waterHeight = 0f; // 수면 높이 (필요시 조절)
        Plane waterPlane = new Plane(Vector3.up, new Vector3(0f, waterHeight, 0f));

        if (waterPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);

            Vector3 dir = hitPoint - _playerTransform.position;
            dir.y = 0f;

            float clampedDist = Mathf.Clamp(dir.magnitude, fishingData.minCastRadius, fishingData.maxCastRadius);
            Vector3 clampedDir = dir.normalized * clampedDist;

            Vector3 targetPos = _playerTransform.position + clampedDir;
            targetPos.y = waterHeight;

            _indicator.Show(targetPos);

            if (inputData.JumpPressed)
            {
                inputData.ConsumeJump();
                _indicator.Hide();
                SetState(FishingState.Casting);
                _bobber.Cast(_playerTransform.position, targetPos, fishingData, OnBobberLanded);
            }
        }
    }

    private void HandleBiting()
    {
        if (inputData.JumpPressed)
        {
            inputData.ConsumeJump();
            CancelInvoke(nameof(OnBiteTimeout));
            SetState(FishingState.Minigame);
            inputData.ConsumeJump();
            minigame.StartMinigame(fishingData, _currentFishData, OnMinigameResult);
        }
    }

    private void OnBobberLanded()
    {
        // 물고기 랜덤 결정 (나중에 물고기 시스템으로 교체)
        _currentFishData = fishDataList[Random.Range(0, fishDataList.Length)];

        SetState(FishingState.Waiting);
        _bobber.StartWaiting(fishingData, OnBite);
    }

    private void OnBite()
    {
        SetState(FishingState.Biting);
        Invoke(nameof(OnBiteTimeout), fishingData.biteTimeLimit);
    }

    private void OnBiteTimeout()
    {
        if (CurrentState != FishingState.Biting) return;
        Debug.Log("입질 시간 초과");
        _bobber.Hide();
        SetState(FishingState.Aiming);

        // 다시 인디케이터 표시
        _indicator.gameObject.SetActive(true);
    }

    private void OnMinigameResult(bool success)
    {
        Debug.Log(success ? "낚시 성공!" : "낚시 실패!");
        Invoke(nameof(ReturnToAiming), 1.5f);
    }

    private void ReturnToAiming()
    {
        _bobber.Hide();
        inputData.ConsumeJump();
        SetState(FishingState.Aiming);
    }

    private void SetState(FishingState newState)
    {
        CurrentState = newState;
        Debug.Log($"FishingState: {newState}");
    }
}