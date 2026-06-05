using System.Collections.Generic;
using UnityEngine;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private List<UpgradeRow> rows;
    [SerializeField] private InventoryTooltip tooltip;

    private void Start()
    {
        if (UpgradeSystem.Instance != null)
        {
            // 각 Row 초기화 - upgradeDataList와 rows 순서 맞춰야 함
            for (int i = 0; i < rows.Count && i < UpgradeSystem.Instance.UpgradeDataList.Count; i++)
                rows[i].Init(UpgradeSystem.Instance.UpgradeDataList[i], tooltip);
            UpgradeSystem.Instance.OnUpgradeChanged += RefreshAll;
        }
        
        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.OnGoldChanged += _ => RefreshAll();
    }

    private void OnEnable()
    {
        // OnEnable에서는 RefreshAll만 (Instance 체크 포함)
        RefreshAll();
    }

    private void OnDisable()
    {
        if (UpgradeSystem.Instance != null)
            UpgradeSystem.Instance.OnUpgradeChanged -= RefreshAll;
    }

    private void RefreshAll()
    {
        if (UpgradeSystem.Instance == null) return;

        foreach (UpgradeRow row in rows)
            row.Refresh();
    }
}