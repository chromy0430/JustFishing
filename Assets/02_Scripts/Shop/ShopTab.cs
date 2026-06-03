using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopTab : MonoBehaviour
{
    [Header("Tab Buttons")]
    [SerializeField] private List<Button> _buttons = new List<Button>();

    [Header("Tab UI")]
    [SerializeField] private List<GameObject> _uiList = new List<GameObject>();
    
    private void Start()
    {
        foreach (GameObject ui in _uiList)
            ui.SetActive(false);

        // 버튼마다 람다로 인덱스 캡처
        for (int i = 0; i < _buttons.Count; i++)
        {
            int index = i; // 클로저 캡처용 (i를 직접 쓰면 항상 마지막 값)
            _buttons[index].onClick.AddListener(() => OpenUI(index));
        }
        
        OpenUI(0);
    }

    private void OpenUI(int index)
    {
        for (int i = 0; i < _uiList.Count; i++)
            _uiList[i].SetActive(i == index);
    }
}
