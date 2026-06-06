using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UpgradeSlot : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image iconImage;

    private UpgradeData  _data;
    private int          _slotIndex;
    private InventoryTooltip _tooltip;
    
    private void OnEnable()
    {
        if (UpgradeSystem.Instance != null)
            UpgradeSystem.Instance.OnUpgradeChanged += Refresh;
        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.OnGoldChanged += _ => Refresh();
    }
    
    private void OnDisable()
    {
        if (UpgradeSystem.Instance != null)
            UpgradeSystem.Instance.OnUpgradeChanged -= Refresh;
        if (PlayerWallet.Instance != null)
            PlayerWallet.Instance.OnGoldChanged -= _ => Refresh();
    }


    public void Init(UpgradeData data, int index, InventoryTooltip tooltip)
    {
        _data      = data;
        _slotIndex = index;
        _tooltip   = tooltip;

        iconImage.sprite = data.levels[index].icon;
        Refresh();
    }

    public void Refresh()
    {
        if (_data == null) return;
        if (UpgradeSystem.Instance == null) return;

        int currentLevel = UpgradeSystem.Instance.GetCurrentLevel(_data.toolName);

        if (_slotIndex <= currentLevel)
        {
            // 보유 중 → 흰색 (최우선)
            iconImage.color = Color.white;
        }
        else if (_slotIndex == currentLevel + 1 && CanAfford())
        {
            // 다음 단계 + 구매 가능 → 형광색
            iconImage.color = new Color(0.6f, 1f, 0.2f, 1f);
        }
        else
        {
            // 미보유 + 구매 불가 → 회색
            iconImage.color = new Color(0.4f, 0.4f, 0.4f, 1f);
        }
    }

    private bool CanAfford()
    {
        if (_slotIndex == 0) return true;

        UpgradeLevel next = _data.levels[_slotIndex];

        if (PlayerWallet.Instance == null) return false;
        if (PlayerWallet.Instance.Gold < next.goldCost) return false;

        if (next.requiredFish != null)
        {
            if (InventorySystem.Instance == null) return false;
            int count = 0;
            foreach (FishInstance fish in InventorySystem.Instance.Items)
                if (fish.fishData == next.requiredFish) count++;
            if (count < next.requiredFishCount) return false;
        }

        return true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_tooltip == null) return;

        UpgradeLevel level       = _data.levels[_slotIndex];
        int          currentLevel = UpgradeSystem.Instance.GetCurrentLevel(_data.toolName);

        // 비용 텍스트
        string costText;
        if (_slotIndex == 0)
            costText = "기본 보유";
        else if (level.requiredFish == null)
            costText = $"{level.goldCost} G";
        else
            costText = $"{level.goldCost} G  +  " +
                       $"{level.requiredFish.fishName} x{level.requiredFishCount}";

        // 보유 상태
        string statusText = _slotIndex <= currentLevel ? "보유 중" : "미보유";

        _tooltip.ShowUpgrade(
            level.toolTipTitle,
            level.toolTipDesc,
            level.toolTipStat,
            costText,
            statusText
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltip?.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int currentLevel = UpgradeSystem.Instance.GetCurrentLevel(_data.toolName);

        // 이미 보유한 단계거나 순서 건너뛰기 방지
        if (_slotIndex == 0 || _slotIndex <= currentLevel) return;
        if (_slotIndex != currentLevel + 1) return;

        // 즉시 강화 시도
        UpgradeSystem.UpgradeResult result = UpgradeSystem.Instance.TryUpgrade(_data);
        

        string message = result switch
        {
            UpgradeSystem.UpgradeResult.Success =>
                $"{_data.toolName}이(가) 강화되었습니다.",
            UpgradeSystem.UpgradeResult.NotEnoughGold =>
                "골드가 부족합니다.",
            UpgradeSystem.UpgradeResult.NotEnoughFish =>
                $"{_data.levels[_slotIndex].requiredFish?.fishName}이(가) 부족합니다.",
            UpgradeSystem.UpgradeResult.AlreadyMaxLevel =>
                "이미 최고 레벨입니다.",
            _ => ""
        };

        // Notification으로 결과 표시
        if (!string.IsNullOrEmpty(message))
        {
            // NotificationData를 런타임에 생성
            NotificationManager.Instance?.ShowMessage(message);
        }
    }
}