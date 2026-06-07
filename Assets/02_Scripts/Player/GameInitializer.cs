using System.Collections.Generic;
using UnityEngine;

// Island 씬 시작점 오브젝트에 부착
public class GameInitializer : MonoBehaviour
{
    [SerializeField] private List<FishData> allFishData;

    private void Start()
    {
        if (SaveSystem.Instance == null) return;
        if (SaveSystem.Instance.CurrentSlot < 0) return;

        // 로드된 슬롯의 인벤토리 적용
        SaveSystem.Instance.ApplyInventoryData(
            SaveSystem.Instance.CurrentSlot,
            allFishData);
    }
}
