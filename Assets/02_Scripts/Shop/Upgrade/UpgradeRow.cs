using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradeRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI  toolNameTxt;
    [SerializeField] private List<UpgradeSlot> slots; // 3개

    private UpgradeData _data;

    public void Init(UpgradeData data, InventoryTooltip tooltip)
    {
        _data = data;
        if (toolNameTxt != null)
            toolNameTxt.text = data.toolName;

        for (int i = 0; i < slots.Count && i < data.levels.Length; i++)
            slots[i].Init(data, i, tooltip);
    }

    public void Refresh()
    {
        foreach (UpgradeSlot slot in slots)
            slot.Refresh();
    }
}