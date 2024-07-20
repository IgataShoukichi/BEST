using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using UnityEngine.EventSystems;

public class ButtonManager : MonoBehaviour
{
    //操作方法別表示
    [SerializeField][Header("コントローラー")] GameObject controllerButtonLayout;
    [SerializeField] GameObject controllerEnterButton;
    [SerializeField] GameObject controllerAction1Button;
    [SerializeField] GameObject controllerAction2Button;
    [SerializeField] GameObject controllerHelpButton;

    [SerializeField][Header("タッチ")] GameObject touchButtonLayout;
    [SerializeField] GameObject touchEnterButton;
    [SerializeField] GameObject touchAction1Button;
    [SerializeField] GameObject touchAction2Button;
    [SerializeField] GameObject touchHelpButton;
    //色
    //Color32 buttonEnableOn = new Color32(240, 240, 240, 255);
    //Color32 buttonEnableOff = new Color32(150, 150, 150, 255);
    Color32 buttonEnableOn = new Color32(0, 0, 0, 230);
    Color32 buttonEnableOff = new Color32(0, 0, 0, 180);

    [System.NonSerialized] public UnityEvent onActionButtonUpdate = new UnityEvent();

    [System.NonSerialized] public Button enterButton = null;
    [System.NonSerialized] public Button action1Button = null;
    [System.NonSerialized] public Button action2Button = null;
    [System.NonSerialized] public Button helpButton = null;
    [System.NonSerialized] public GameObject enterObject = null;
    [System.NonSerialized] public GameObject action1Object = null;
    [System.NonSerialized] public GameObject action2Object = null;
    [System.NonSerialized] public GameObject helpObject = null;

    bool touchInput;
    


    void Awake()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                touchInput = false;
                ButtonSetting(touchInput);
                break;
            case RuntimePlatform.Android:
                touchInput = true;
                ButtonSetting(touchInput);
                break;
        }
    }

    void ButtonSetting(bool tInput)
    {
        controllerButtonLayout.SetActive(!tInput);
        touchButtonLayout.SetActive(tInput);
        if (tInput)//タッチの場合
        {
            enterObject = touchEnterButton;
            action1Object = touchAction1Button;
            action2Object = touchAction2Button;
            helpObject = touchHelpButton;
            enterButton = touchEnterButton.GetComponent<Button>();
            action1Button = touchAction1Button.GetComponent<Button>();
            action2Button = touchAction2Button.GetComponent<Button>();
            helpButton = touchHelpButton.GetComponent<Button>();
        }
        else//それ以外
        {
            enterObject = controllerEnterButton;
            action1Object = controllerAction1Button;
            action2Object = controllerAction2Button;
            helpObject = controllerHelpButton;
            enterButton = null;
            action1Button = null;
            action2Button = null;
            helpButton = null;
        }
        action1Object.GetComponent<ButtonCoolTime>().onFinishCoolTime.AddListener(() => onActionButtonUpdate.Invoke());
        action2Object.GetComponent<ButtonCoolTime>().onFinishCoolTime.AddListener(() => onActionButtonUpdate.Invoke());
    }


    ///////////////////////////////////////
    //Enter

    public void EnterButtonReset()
    {
        enterButton?.onClick.RemoveAllListeners();
        enterObject.GetComponent<EventTrigger>()?.triggers.RemoveRange(0, enterButton.GetComponent<EventTrigger>().triggers.Count);
    }

    public void EnterButtonOn()
    {
        if (touchInput)
        {
            enterButton.enabled = true;
            enterObject.GetComponent<Image>().color = buttonEnableOn;
        }
        else
        {
            //enterObject.GetComponent<Image>().enabled = true;
            //enterObject.transform.GetChild(0).GetComponent<Text>().enabled = true;
        }
    }

    public void EnterButtonOff()
    {
        if (touchInput)
        {
            enterButton.enabled = false ;
            enterObject.GetComponent<Image>().color = buttonEnableOff;
        }
        else
        {
            //enterObject.GetComponent<Image>().enabled = false;
            //enterObject.transform.GetChild(0).GetComponent<Text>().enabled = false;

        }
    }

    public void EnterButtonTextChange(string text)
    {
        enterObject.transform.GetChild(0).GetComponent<Text>().text = text;
    }

    ///////////////////////////////////////
    //Action1
    public void Action1ButtonReset()
    {
        action1Button?.onClick.RemoveAllListeners();
    }

    public void Action1ButtonOn()
    {
        if (touchInput)
        {
            action1Button.enabled = true;
            action1Object.GetComponent<Image>().color = buttonEnableOn;
        }
    }

    public void Action1ButtonOff()
    {
        if (touchInput)
        {
            action1Button.enabled = false;
            action1Object.GetComponent<Image>().color = buttonEnableOff;
        }
    }

    public void Action1ButtonTextChange(string text)
    {
        action1Object.transform.GetChild(0).GetComponent<Text>().text = text;
    }

    public void Action1ButtonCoolTime(float time)
    {
        action1Object.GetComponent<ButtonCoolTime>().CoolTimeSetting(time, buttonEnableOn, buttonEnableOff);
    }

    //Action2

    public void Action2ButtonReset()
    {
        action2Button?.onClick.RemoveAllListeners();
    }

    public void Action2ButtonOn()
    {
        if (touchInput)
        {
            action2Button.enabled = true;
            action2Object.GetComponent<Image>().color = buttonEnableOn;
        }
    }

    public void Action2ButtonOff()
    {
        if (touchInput)
        {
            action2Button.enabled = false;
            action2Object.GetComponent<Image>().color = buttonEnableOff;
        }
    }

    public void Action2ButtonTextChange(string text)
    {
        action2Object.transform.GetChild(0).GetComponent<Text>().text = text;
    }

    public void Action2ButtonCoolTime(float time)
    {
        action2Object.GetComponent<ButtonCoolTime>().CoolTimeSetting(time, buttonEnableOn, buttonEnableOff);
    }

    //////////////////////////////////////
    //Help
    public void HelpButtonOn()
    {
        if (touchInput)
        {
            helpButton.enabled = true;
            helpObject.GetComponent<Image>().color = buttonEnableOn;
        }
    }

    public void HelpButtonOff()
    {
        if (touchInput)
        {
            helpButton.enabled = false;
            helpObject.GetComponent<Image>().color = buttonEnableOff;
        }
    }

    //////////////////////////////////////
}
