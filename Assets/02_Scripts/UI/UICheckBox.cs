using System;
using UnityEngine;
using UnityEngine.UI;


public class UICheckBox : MonoBehaviour
{
    public Button checkBtn;
    public GameObject onCheckBtn;
    public bool isOn;

    private void Awake()
    {
        isOn = false;
        Check();
    }

    void Start()
    {
        checkBtn.onClick.AddListener(() =>
        {
            if(isOn == false)
            {
                OnCheckBox();             
            }
            else
            {
                OffCheckBox();               
            }
        });
    }

    private void Check()
    {
        if (isOn)
        {
            OnCheckBox();  
        }
        else
        {
            OffCheckBox();
        }
    }

    public void OnCheckBox()
    {
        onCheckBtn.SetActive(true);
        isOn = true;
    }

    public void OffCheckBox()
    {
        onCheckBtn.SetActive(false);
        isOn = false;
    }
}