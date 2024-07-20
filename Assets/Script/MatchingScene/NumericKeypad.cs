using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.InputSystem;
using System;
using UnityEngine.EventSystems;

public class NumericKeypad : MonoBehaviour
{
    public Action<string> onReturnString;
    [System.NonSerialized] public UnityEvent onCloseKeyboard = new UnityEvent();

    GameObject keyboardPanel;
    [SerializeField] Text fieldText;
    [SerializeField] GameObject enterButton;
    [SerializeField] GameObject backButton;
    [SerializeField] GameObject escapeButton;
    [SerializeField] GameObject backPointer;//どこをさしているかの表示
    [SerializeField] List<GameObject> keys;

    [SerializeField] InputAction tapAction;//ボタンをタップ
    [SerializeField] InputAction enterAction;//入力を終了
    [SerializeField] InputAction backAction;//一つ消す
    [SerializeField] InputAction escapeAction;//戻る
    [SerializeField] InputAction leftAction;//左
    [SerializeField] InputAction rightAction;//右
    [SerializeField] InputAction upAction;//上
    [SerializeField] InputAction downAction;//下

    bool isInput = false;
    float panelMoveRange = 20f;
    float panelMoveTime = 0.2f;

    int maxStringCount = 0;//最大文字数
    int minStringCount = 0;//最小文字数
    int nowSelectNumber = 0;
    int nowSelectX = 0;
    int nowSelectY = 0;
    bool nowSelectXOption = false;
    Keyboard keyboard;

    void Start()
    {
        keyboardPanel = this.gameObject;
        keyboardPanel.GetComponent<CanvasGroup>().alpha = 0;

        tapAction?.Enable();
        enterAction?.Enable();
        backAction?.Enable();
        escapeAction?.Enable();
        leftAction?.Enable();
        rightAction?.Enable();
        upAction?.Enable();
        downAction?.Enable();


        KeySetting();
        this.gameObject.SetActive(false);
    }
    
    void Update()
    {
        ShareInputUpdate();
    }

    public void KeyboardOpen(int minStringCount , int maxStringCount)
    {
        keyboard = Keyboard.current;
        keyboard.onTextInput += ch => KeyboardInput(ch.ToString());

        this.minStringCount = minStringCount;
        this.maxStringCount = maxStringCount;
        nowSelectNumber = 0;
        nowSelectX = 0;
        nowSelectY = 0;
        nowSelectXOption = false;
        KeyMove(keys[nowSelectNumber]);//初期位置
        nowSelectNumber++;
        fieldText.text = "";
        backPointer.SetActive(true);
        backPointer.transform.DOScale(Vector3.one * 1.03f, panelMoveTime * 5).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
        
        keyboardPanel.SetActive(true);
        keyboardPanel.GetComponent<RectTransform>().DOAnchorPosY(-panelMoveRange, 0);
        keyboardPanel.GetComponent<CanvasGroup>().DOFade(1, panelMoveTime);
        keyboardPanel.GetComponent<RectTransform>().DOAnchorPosY(0, panelMoveTime).OnComplete(() => isInput = true);
    }

    public void KeyboardClose(bool flag = false)
    {
        SoundList.Instance.SoundEffectPlay(1);
        keyboard.onTextInput -= ch => KeyboardInput(ch.ToString());
        isInput = false;
        backPointer.transform.DOKill();
        backPointer.transform.localScale = Vector3.one;
        if (!flag)
        {
            keyboardPanel.GetComponent<CanvasGroup>().DOFade(0, panelMoveTime).OnComplete(() => onCloseKeyboard.Invoke());
        }
        else
        {
            keyboardPanel.GetComponent<CanvasGroup>().DOFade(0, panelMoveTime);
        }
        keyboardPanel.GetComponent<RectTransform>().DOAnchorPosY(-panelMoveRange, panelMoveTime).OnComplete(() => keyboardPanel.SetActive(false));
    }

    void KeySetting()
    {
        int count = 0;
        foreach(GameObject tempKey in keys)
        {
            count++;
            if(count >= 10)
            {
                count = 0;
            }
            tempKey.name = count.ToString();
            EventTrigger.Entry entry1 = new EventTrigger.Entry();
            entry1.eventID = EventTriggerType.PointerClick; ;
            entry1.callback.AddListener((eventDate) => { InputText(tempKey.name); });
            tempKey.GetComponent<EventTrigger>().triggers.Add(entry1);

        }
    }

    void ShareInputUpdate()
    {
        if (isInput && GameVariable.nowApplicationFocus)
        {
            if (tapAction.WasPressedThisFrame())
            {
                TapActionSetting();
            }
            if (enterAction.WasPressedThisFrame())
            {
                EndInput();
            }
            if (backAction.WasPressedThisFrame())
            {
                BackText(true);
            }
            if (escapeAction.WasPressedThisFrame())
            {
                SoundList.Instance.SoundEffectPlay(1);
                KeyboardClose();
            }

            if (leftAction.WasPressedThisFrame())
            {
                GamepadInput(false,-1);
            }
            if(rightAction.WasPressedThisFrame())
            {
                GamepadInput(false, 1);
            }
            if (upAction.WasPressedThisFrame())
            {
                GamepadInput(true, -1);
            }
            if (downAction.WasPressedThisFrame())
            {
                GamepadInput(true, 1);
            }
        }
    }

    void GamepadInput(bool upDown, int changeNumber)
    {
        if (isInput)
        {
            if (!backPointer.activeSelf)
            {
                backPointer.SetActive(true);
            }
            if (upDown)
            {
                nowSelectY += changeNumber;
                if (nowSelectY > 2)
                {
                    if (nowSelectX == 1)
                    {
                        if (nowSelectY > 3)
                        {
                            nowSelectY = 0;
                        }
                        else
                        {
                            nowSelectY = 3;
                        }
                    }
                    else
                    {
                        nowSelectY = 0;
                    }

                }
                else if (nowSelectY < 0)
                {
                    if (nowSelectX == 1)
                    {
                        nowSelectY = 3;
                    }
                    else
                    {
                        nowSelectY = 2;
                    }
                }
            }
            else
            {
                if (nowSelectY != 3)//選択した文字が0以外なら
                {
                    nowSelectX += changeNumber;
                }

                if (nowSelectX > 2)
                {
                    if (nowSelectX > 3)
                    {
                        nowSelectX = 0;
                    }
                    else
                    {
                        nowSelectX = 3;
                    }

                    //nowSelectX = 0;
                }
                else if (nowSelectX < 0)
                {
                    nowSelectX = 3;
                }
            }
            nowSelectXOption = nowSelectX >= 3 ? true : false;//識別
            Debug.Log(nowSelectX + " " + nowSelectY);
            if (!nowSelectXOption)
            {
                nowSelectNumber = (nowSelectY * 3) + nowSelectX;
                if (nowSelectNumber >= 9)
                {
                    nowSelectNumber = 9;
                }
                KeyMove(keys[nowSelectNumber]);
                nowSelectNumber++;
                if (nowSelectNumber >= 10)
                {
                    nowSelectNumber = 0;
                }
            }
            else
            {
                switch (nowSelectY)
                {
                    case 0://戻る
                        KeyMove(escapeButton);
                        break;
                    case 1:
                        KeyMove(backButton);
                        break;
                    case 2:
                        KeyMove(enterButton);
                        break;
                }
            }
        }
    }

    void KeyMove(GameObject targetObject)
    {
        SoundList.Instance.SoundEffectPlay(0);
        backPointer.GetComponent<RectTransform>().sizeDelta = new Vector2(
                targetObject.GetComponent<RectTransform>().sizeDelta.x + 20,
                targetObject.GetComponent<RectTransform>().sizeDelta.y + 20);
        backPointer.transform.localPosition = new Vector3(
            targetObject.transform.localPosition.x,
            targetObject.transform.localPosition.y);
    }

    void KeyboardInput(string text)
    {
        if (isInput)
        {
            string inputName = text;
            int inputNumber;
            if(int.TryParse(inputName, out inputNumber))
            {
                if (inputNumber >= 0 && inputNumber <= 9)
                {
                    InputText(inputNumber.ToString());
                }
            }
            
        }
    }

    void TapActionSetting()
    {
        if (nowSelectXOption)
        {
            switch (nowSelectY)
            {
                case 0://閉じる
                    KeyboardClose();
                    break;
                case 1:
                    BackText(true);
                    break;
                case 2:
                    EndInput(); 
                    break;
            }
        }
        else
        {
            InputText(nowSelectNumber.ToString(),true);
        }
        
    }

    public void BackText(bool buttonInput = false)
    {
        if (!buttonInput && backPointer.activeSelf)
        {
            backPointer.SetActive(false);
        }
        if (fieldText.text.Length <= 0)
        {
            //KeyboardClose();
        }
        else
        {
            SoundList.Instance.SoundEffectPlay(1);
            fieldText.text = fieldText.text.Remove(fieldText.text.Length - 1);
        }
    }

    public void InputText(string text,bool buttonInput = false)
    {
        if (!buttonInput && backPointer.activeSelf)
        {
            backPointer.SetActive(false);
        }
        if (text != null)
        {
            if(fieldText.text.Length < maxStringCount)
            {
                SoundList.Instance.SoundEffectPlay(1);
                fieldText.text += text;
            }
        }
    }

    public void EndInput()
    {
        if(fieldText.text.Length >= minStringCount)
        {
            onReturnString.Invoke(fieldText.text);
            KeyboardClose(true);
        }
    }
}
