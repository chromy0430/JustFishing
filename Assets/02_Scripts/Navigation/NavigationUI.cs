using UnityEngine;
using UnityEngine.UI;

public class NavigationUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    // 배 연출용 Transform (항구에 있는 배 오브젝트)
    [SerializeField] private Transform dockBoat;
    [SerializeField] private float boatSailDuration = 2f;  // 화면 밖으로 나가는 시간
    [SerializeField] private Vector3 boatSailDirection = new Vector3(-1, 0, 0); // 나가는 방향

    private void Awake()
    {
        panel.SetActive(false);
        yesButton.onClick.AddListener(OnYes);
        noButton.onClick.AddListener(OnNo);
    }

    public void Show() => panel.SetActive(true);
    public void Hide() => panel.SetActive(false);

    private void OnNo()
    {
        Hide();
    }

    private void OnYes()
    {
        Hide();
        StartCoroutine(SailAndTransition());
    }

    private System.Collections.IEnumerator SailAndTransition()
    {
        // 배 연출: dockBoat를 화면 밖으로 이동
        float elapsed = 0f;
        Vector3 startPos = dockBoat.position;
        Vector3 endPos = startPos + boatSailDirection * 30f; // 30유닛 이동

        while (elapsed < boatSailDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / boatSailDuration;
            dockBoat.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // 페이드 + 씬 전환
        SceneTransition.Instance.TransitionToScene("Ocean");
    }
}