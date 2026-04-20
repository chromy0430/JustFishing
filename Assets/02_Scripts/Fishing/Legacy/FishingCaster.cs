using UnityEngine;
using UnityEngine.InputSystem;

public class FishingCaster : MonoBehaviour
{
    [Header("References")]
    public Transform rod;                       // Rod 오브젝트
    public Transform rodTip;                    // Rod 끝 빈 오브젝트
    public Transform endLine;                   // EndLine (찌)
    public LineRenderer lineRenderer;           // StartLine의 LineRenderer
    public RopeControllerSimple ropeController; // StartLine의 RopeControllerSimple

    [Header("Rod Swing")]
    public float swingBackAngle = 60f;
    public float swingFrontAngle = -45f;
    public float swingSpeed = 150f;

    [Header("Cast")]
    public float castForce = 12f;
    public float castAngleUp = 30f;

    [Header("Line")]
    public int linePoints = 20;
    public float lineSagAmount = 0.5f;

    public enum State { Idle, SwingBack, SwingForward, Flying, Landed }
    public State state = State.Idle;

    Rigidbody endLineRb;
    float currentAngle = 0f;
    bool launched = false;
    Quaternion defaultRodRotation;

    void Start()
    {
        endLineRb = endLine.GetComponent<Rigidbody>();
        defaultRodRotation = rod.localRotation;

        endLineRb.isKinematic = true;
        ropeController.enabled = false;
        lineRenderer.positionCount = linePoints;
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.isPressed && state == State.Idle)
            StartCast();

        HandleSwing();

        // Flying 중에만 FishingCaster가 LineRenderer 담당
        if (state == State.SwingBack ||
            state == State.SwingForward ||
            state == State.Flying)
        {
            DrawLine();
        }
    }

    void StartCast()
    {
        state = State.SwingBack;
        currentAngle = 0f;
        launched = false;

        endLineRb.isKinematic = true;
        ropeController.enabled = false;
        endLine.position = rodTip.position;

        rod.localRotation = defaultRodRotation;
    }

    void HandleSwing()
    {
        if (state == State.SwingBack)
        {
            currentAngle += swingSpeed * Time.deltaTime;
            rod.localRotation = defaultRodRotation *
                Quaternion.Euler(currentAngle, 0f, 0f);

            if (currentAngle >= swingBackAngle)
            {
                currentAngle = swingBackAngle;
                state = State.SwingForward;
            }
        }
        else if (state == State.SwingForward)
        {
            currentAngle -= swingSpeed * Time.deltaTime;
            rod.localRotation = defaultRodRotation *
                Quaternion.Euler(currentAngle, 0f, 0f);

            // 0도 통과 순간 = 낚싯대가 앞을 향하는 순간 발사
            if (currentAngle <= 0f && !launched)
            {
                LaunchBobber();
                launched = true;
            }

            if (currentAngle <= swingFrontAngle)
            {
                currentAngle = swingFrontAngle;
                state = State.Flying;
            }
        }
    }

    void LaunchBobber()
    {
        endLineRb.isKinematic = false;
        endLineRb.linearVelocity = Vector3.zero;

        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 castDir = (forward +
            Vector3.up * Mathf.Tan(castAngleUp * Mathf.Deg2Rad)).normalized;

        endLineRb.AddForce(castDir * castForce, ForceMode.Impulse);
    }

    // BobberLanding 스크립트가 호출
    public void OnBobberLanded()
    {
        state = State.Landed;

        // 착수 후 RopeControllerSimple에게 LineRenderer 제어권 반환
        ropeController.enabled = true;
    }

    void DrawLine()
    {
        Vector3 start = rodTip.position;
        Vector3 end = endLine.position;
        float dist = Vector3.Distance(start, end);

        Vector3 mid = (start + end) * 0.5f;
        mid.y -= lineSagAmount * (dist * 0.15f + 0.5f);

        for (int i = 0; i < linePoints; i++)
        {
            float t = i / (float)(linePoints - 1);
            lineRenderer.SetPosition(i, Bezier(t, start, mid, end));
        }
    }

    Vector3 Bezier(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
    }
}