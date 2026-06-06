using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class FishingController : MonoBehaviour
{
    public enum FishingState { Idle, Aiming, Casting, Waiting, Biting, Minigame, Result }

    [Header("Data")]
    [SerializeField] private FishingData     fishingData;
    [SerializeField] private PlayerInputData inputData;

    [Header("References")]
    [SerializeField] private FishingMinigame minigame;
    [SerializeField] private PlayerAnimator  playerAnimator; // YellowHuman_01에 있는 컴포넌트
    [SerializeField] private Transform       playerTransform;
    [SerializeField] private GameObject      exclamationMark; // 느낌표 파티클 Transform

    [Header("Prefabs")]
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private GameObject bobberPrefab;

    [Header("물고기 접근 설정")]
    [SerializeField] private float fishApproachSpeed = 2f; // 물고기가 찌로 다가오는 속도

    private Camera           _mainCamera;
    private FishingIndicator _indicator;
    private FishingBobber    _bobber;
    private FishData         _currentFishData;
    private FishAgent        _approachingFish; // 찌로 다가오는 물고기
    private GameObject mark;

    public FishingState CurrentState { get; private set; } = FishingState.Idle;

    public void SetCamera(Camera cam)
    {
        if (cam != null) _mainCamera = cam;
    }

    private void Start()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        if (exclamationMark != null)
        {
            mark = Instantiate(
                exclamationMark,
                playerTransform
            );

            mark.transform.localPosition = Vector3.up * 2.5f;
            mark.SetActive(false);
        }
            
        
    }

    private void Update()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;

        switch (CurrentState)
        {
            case FishingState.Aiming:   HandleAiming();   break;
            case FishingState.Waiting:  HandleWaiting();  break;
            case FishingState.Biting:   HandleBiting();   break;
        }

        // 낚시 모드일 때 인디케이터 방향으로 캐릭터 회전
        if (CurrentState == FishingState.Aiming && _indicator != null
            && _indicator.gameObject.activeSelf)
        {
            RotatePlayerToIndicator();
        }
    }

    public void EnterFishingMode()
    {
        inputData.ConsumeJump();

        GameObject indicatorObj = Instantiate(indicatorPrefab);
        GameObject bobberObj    = Instantiate(bobberPrefab);

        _indicator = indicatorObj.GetComponent<FishingIndicator>();
        _bobber    = bobberObj.GetComponent<FishingBobber>();

        _indicator.Hide();
        _bobber.Hide();

        // 애니메이션은 여기서 실행 안 함
        playerAnimator?.EnterFishingMode();
        SetState(FishingState.Aiming);
    }

    public void ExitFishingMode()
    {
        if (_indicator != null) Destroy(_indicator.gameObject);
        if (_bobber    != null) Destroy(_bobber.gameObject);

        ReleaseApproachingFish();

        if (mark != null)
            mark.SetActive(false);

        // 낚시 모드 종료 → Idle로 복귀
        playerAnimator?.ExitFishingMode();
        SetState(FishingState.Idle);
    }

    private void HandleAiming()
    {
        if (_mainCamera == null || playerTransform == null) return;
        if (Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Ray     ray            = _mainCamera.ScreenPointToRay(mouseScreenPos);

        Plane waterPlane = new Plane(Vector3.up, Vector3.zero);
        if (waterPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 dir      = hitPoint - playerTransform.position;
            dir.y = 0f;

            float   clampedDist = Mathf.Clamp(dir.magnitude,
                fishingData.minCastRadius, fishingData.maxCastRadius);
            Vector3 clampedDir  = dir.normalized * clampedDist;

            Vector3 targetPos = playerTransform.position + clampedDir;
            targetPos.y       = 0f;

            _indicator.Show(targetPos);

            if (inputData.JumpPressed)
            {
                inputData.ConsumeJump();
                _indicator.Hide();

                // 찌 던지는 순간 낚시 애니메이션 시작
                playerAnimator?.OnCast();

                SetState(FishingState.Casting);
                _bobber.Cast(playerTransform.position, targetPos,
                    fishingData, OnBobberLanded);
            }
        }
    }

    private void HandleWaiting()
    {
        // Waiting 상태는 FishingBobber의 WaitRoutine이 처리
        // 물고기 접근 연출은 OnBobberLanded 후 코루틴으로 처리
    }

    private void HandleBiting()
    {
        if (inputData.JumpPressed)
        {
            inputData.ConsumeJump();
            CancelInvoke(nameof(OnBiteTimeout));

            if (mark != null)
                mark.SetActive(false);

            // null이면 재시도
            if (_currentFishData == null)
                _currentFishData = GetRandomFishData();

            if (_currentFishData == null)
            {
                Debug.LogError("FishData null - 미니게임 진입 불가");
                ReturnToAiming();
                return;
            }

            playerAnimator?.SetBiting(true);
            SetState(FishingState.Minigame);
            _bobber.PlaySplash();
            minigame.StartMinigame(fishingData, _currentFishData, OnMinigameResult);
        }
    }


    private void OnBobberLanded()
    {
        // 물고기 랜덤 결정
        _currentFishData = GetRandomFishData();

        SetState(FishingState.Waiting);
        _bobber.StartWaiting(fishingData, OnPreBite);
    }

    // 찌 물리기 전 물고기 접근 연출
    private void OnPreBite()
    {
        StartCoroutine(FishApproachRoutine());
    }

    private System.Collections.IEnumerator FishApproachRoutine()
    {
        _approachingFish = FishManager.Instance?.GetNearestFish(_bobber.transform.position);

        if (_approachingFish != null)
        {
            Vector3 startPos  = _approachingFish.transform.position;
            Vector3 targetPos = _bobber.transform.position;

            float distance = Vector3.Distance(startPos, targetPos);
            float duration = distance / fishApproachSpeed;

            FishManager.Instance?.SetFishOverride(_approachingFish, true);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (_approachingFish == null) break;

                float t = elapsed / duration;

                // EaseInCubic: 처음엔 느리다가 끝에 가속
                float eased = t * t * t;

                _approachingFish.transform.position = Vector3.Lerp(startPos, targetPos, eased);

                // 찌 방향으로 회전
                Vector3 dir = targetPos - _approachingFish.transform.position;
                dir.y = 0f;
                if (dir.magnitude > 0.1f)
                    _approachingFish.transform.rotation = Quaternion.LookRotation(dir);

                yield return null;
            }
            
            if (_approachingFish != null)
                _approachingFish.gameObject.SetActive(false);
        }

        // 물고기 접근 완료 후 찌 내려가는 연출
        _bobber.PlayBiteAnimation(fishingData, () =>
        {
            // 찌 내려간 후 실제 Bite 상태로 전환
            OnBite();
        });
    }

    private void OnBite()
    {
        SetState(FishingState.Biting);

        // 느낌표 파티클 활성화
        if (mark != null)
            mark.SetActive(true);

        Invoke(nameof(OnBiteTimeout), fishingData.biteTimeLimit);
    }

    private void OnBiteTimeout()
    {
        if (CurrentState != FishingState.Biting) return;

        if (mark != null)
            mark.SetActive(false);

        ReleaseApproachingFish();
        _bobber.Hide();
        inputData.ConsumeJump();
        SetState(FishingState.Aiming);
        playerAnimator?.ExitFishingMode();
        _indicator.gameObject.SetActive(true);
        _bobber.StopSplash();

    }

    private void OnMinigameResult(bool success)
    {
        playerAnimator?.SetBiting(false);
        _bobber.StopSplash();
        
        if (success && _approachingFish != null)
        {
            // 물고기 인스턴스 생성
            FishInstance newFish = new FishInstance(_currentFishData);

            // 인벤토리 추가 시도
            InventorySystem.AddResult result =
                InventorySystem.Instance.TryAddFish(newFish);

            if (result == InventorySystem.AddResult.Success)
            {
                // 현재 Zone 정보와 함께 퀘스트 진행
                int currentZone = FishManager.Instance?.GetCurrentZone() ?? 1;
                QuestSystem.Instance?.OnFishCaught(newFish, currentZone);
                
                Debug.Log($"물고기 획득: {newFish.fishData.fishName} " +
                          $"({newFish.length:F1}cm / {newFish.weight:F1}kg / {newFish.price}G)");
            }
            else
            {
                // 인벤토리 가득 참 → 교체 UI 표시
                InventoryReplaceUI.Instance?.Show(newFish, result);
            }

            FishManager.Instance?.UnregisterFish(_approachingFish);
            Destroy(_approachingFish.gameObject);
            _approachingFish = null;
        }
        else
        {
            if (_approachingFish != null)
            {
                _approachingFish.gameObject.SetActive(true);
                ReleaseApproachingFish();
            }
        }


        Debug.Log(success ? "낚시 성공!" : "낚시 실패!");
        
        // 낚시 모드 유지 → 다시 조준 상태로
        Invoke(nameof(ReturnToAiming), 1.5f);
    }

    private void ReturnToAiming()
    {
        _bobber.Hide();
        inputData.ConsumeJump();

        // 낚시 모드 유지 상태로 다시 조준
        SetState(FishingState.Aiming);
        _indicator.gameObject.SetActive(true);
    }

    private void RotatePlayerToIndicator()
    {
        if (playerTransform == null) return;

        Vector3 dir = _indicator.transform.position - playerTransform.position;
        dir.y = 0f;
        if (dir.magnitude < 0.1f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        playerTransform.rotation = Quaternion.Slerp(
            playerTransform.rotation,
            targetRot,
            10f * Time.deltaTime
        );
    }

    // 물고기 제어권 복귀
    private void ReleaseApproachingFish()
    {
        if (_approachingFish != null)
        {
            FishManager.Instance?.SetFishOverride(_approachingFish, false);
            _approachingFish = null;
        }
    }

    private FishData GetRandomFishData()
    {
        FishManager fishManager = FishManager.Instance;
        if (fishManager == null)
        {
            Debug.LogError("FishManager가 없습니다.");
            return null;
        }

        int      zone     = fishManager.GetCurrentZone();
        FishData fishData = fishManager.GetZoneData().GetRandomFishData(zone);

        if (fishData == null)
            Debug.LogError($"Zone {zone}의 FishData가 null입니다. FishZoneData를 확인해주세요.");

        return fishData;
    }

    private void SetState(FishingState newState)
    {
        CurrentState = newState;
    }
    
    public void SetPlayerAnimator(PlayerAnimator animator)
    {
        playerAnimator = animator;
    }
}