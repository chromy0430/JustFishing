using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIClickSound : MonoBehaviour
{
    public static UIClickSound Instance { get; private set; }

    // 클릭음 무시할 오브젝트 태그 (선택사항)
    [SerializeField] private List<string> ignoreTags = new List<string>();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        // UI 위에서 클릭했을 때만 재생
        if (EventSystem.current == null) return;
        if (!EventSystem.current.IsPointerOverGameObject()) return;

        AudioManager.Instance?.PlayUIClick();
    }
}