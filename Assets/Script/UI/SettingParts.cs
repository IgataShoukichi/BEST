using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class SettingParts : MonoBehaviour
{
    //[System.NonSerialized] public Action<int> onUpdateSelect = null;
    [System.NonSerialized] public UnityEvent onUpdate = new UnityEvent();
    [System.NonSerialized] public UnityEvent onInputTextStart = new UnityEvent();//文字入力を始めた時
    [System.NonSerialized] public UnityEvent onInputTextEnd = new UnityEvent();//文字入力を終えた時

    [SerializeField] ModeSelect modeSelect;


    [SerializeField] EventTrigger backButton;
    [SerializeField] EventTrigger nextButton;

    [SerializeField] Text selectText;
    [SerializeField] Scrollbar scrollbar;
    [SerializeField] Button button;
    
    enum ModeSelect
    {
        Swipe,
        Scroll,
        NameChange,
        Button
    }

    //スワイプ
    int nowNumber = 0;
    //スクロール
    float nowScrollVolume = 0;
    float oneStepValue = 0;
    //テキスト
    List<string> modeNames = new List<string>();
    Coroutine textMoveCoroutine = null;
    float textMoveRange = 10;
    float textMoveSpeed = 0.1f;
    //名前
    InputField inputField = null;
    string oldName = "";
    //ボタン
    UnityAction uAction = null;

    void Awake()
    {
        //if(modeSelect == ModeSelect.Swipe)
        //{
        //    EventTrigger.Entry entry1 = new EventTrigger.Entry();
        //    EventTrigger.Entry entry2 = new EventTrigger.Entry();
        //    entry1.eventID = EventTriggerType.PointerClick;
        //    entry2.eventID = EventTriggerType.PointerClick;
        //    entry1.callback.AddListener((eventDate) => { BackNext(true); });
        //    entry2.callback.AddListener((eventDate) => { BackNext(false); });
        //    nextButton.GetComponent<EventTrigger>().triggers.Add(entry1);
        //    backButton.GetComponent<EventTrigger>().triggers.Add(entry2);
        //}
       
        switch (modeSelect)
        {
            case ModeSelect.Swipe:
                scrollbar.gameObject.SetActive(false);
                selectText.gameObject.SetActive(true);
                button.gameObject.SetActive(false);
                EventTrigger.Entry entry1 = new EventTrigger.Entry();
                EventTrigger.Entry entry2 = new EventTrigger.Entry();
                entry1.eventID = EventTriggerType.PointerClick;
                entry2.eventID = EventTriggerType.PointerClick;
                entry1.callback.AddListener((eventDate) => { BackNext(true); });
                entry2.callback.AddListener((eventDate) => { BackNext(false); });
                nextButton.GetComponent<EventTrigger>().triggers.Add(entry1);
                backButton.GetComponent<EventTrigger>().triggers.Add(entry2);
                break;
            case ModeSelect.Scroll:
                scrollbar.gameObject.SetActive(true);
                selectText.gameObject.SetActive(false);
                button.gameObject.SetActive(false);
                EventTrigger.Entry entry3 = new EventTrigger.Entry();
                EventTrigger.Entry entry4 = new EventTrigger.Entry();
                entry3.eventID = EventTriggerType.PointerClick;
                entry4.eventID = EventTriggerType.PointerClick;
                entry3.callback.AddListener((eventDate) => { BackNext(true); });
                entry4.callback.AddListener((eventDate) => { BackNext(false); });
                nextButton.GetComponent<EventTrigger>().triggers.Add(entry3);
                backButton.GetComponent<EventTrigger>().triggers.Add(entry4);
                scrollbar.onValueChanged.AddListener((value) => nowScrollVolume = value);
                break;
            case ModeSelect.NameChange:
                scrollbar.gameObject.SetActive(false);
                selectText.gameObject.SetActive(false);
                button.gameObject.SetActive(true);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => Enter());
                backButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(false);
                break;
            case ModeSelect.Button:
                scrollbar.gameObject.SetActive(false);
                selectText.gameObject.SetActive(false);
                button.gameObject.SetActive(true);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => Enter());
                backButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(false);
                break;
        }
        //this.gameObject.SetActive(false);
    }

    //初期設定
    public int SwipeSetting(string[] modeNames,int nowSelectNumber)
    {
        //scrollbar.gameObject.SetActive(false);
        //selectText.gameObject.SetActive(true);
        //button.gameObject.SetActive(false);

        this.modeNames.Clear();
        this.modeNames.AddRange(modeNames);
        nowNumber = nowSelectNumber;
        selectText.text = modeNames[nowNumber];
        this.gameObject.SetActive(true);

        return nowNumber;
    }

    public float ScrollSetting(float settingNumber,int stepCount = -1)
    {
        //scrollbar.gameObject.SetActive(true);
        //selectText.gameObject.SetActive(false);
        //button.gameObject.SetActive(false);

        scrollbar.value = settingNumber;
        nowScrollVolume = scrollbar.value;
        if (stepCount > 0)
        {
            scrollbar.numberOfSteps = stepCount;
        }
        oneStepValue = 1f / scrollbar.numberOfSteps;
        return scrollbar.value;
    }

    public string NameChangeSetting(string name)
    {
        //scrollbar.gameObject.SetActive(false);
        //selectText.gameObject.SetActive(false);
        //button.gameObject.SetActive(true);

        inputField = button.transform.GetChild(2).GetComponent<InputField>();
        inputField.text = name;
        inputField.onEndEdit.RemoveAllListeners();
        inputField.onEndEdit.AddListener((text) => EndEdit());
        return name;
    }

    public void ButtonSetting(string name,UnityAction action)
    {
        inputField = button.transform.GetChild(2).GetComponent<InputField>();
        inputField.text = name;
        inputField.enabled = false;
        uAction = action;
    }
    public int ReturnNumber()
    {
        return nowNumber;
    }
    public float ReturnVolume()
    {
        return nowScrollVolume;
    }
    public string ReturnString()
    {
        return inputField.text;
    }

    public void BackNext(bool next)
    {
        if (modeSelect == ModeSelect.Scroll)
        {
            if (next)
            {
                scrollbar.value += oneStepValue;
                scrollbar.value = scrollbar.value >= 1.0f ? 1f : scrollbar.value;
                nowScrollVolume = scrollbar.value;
                SoundList.Instance.SoundEffectPlay(2, 0.3f);
                StartCoroutine(ButtonDown(nextButton.gameObject));
            }
            else
            {
                scrollbar.value -= oneStepValue;
                scrollbar.value = scrollbar.value <= 0.0f ? 0.0f : scrollbar.value;
                nowScrollVolume = scrollbar.value;
                SoundList.Instance.SoundEffectPlay(2, 0.3f);
                StartCoroutine(ButtonDown(backButton.gameObject));
            }
        }
        if (modeSelect == ModeSelect.Swipe)
        {
            if (next)
            {
                nowNumber++;
                if (nowNumber > this.modeNames.Count - 1)
                {
                    nowNumber = 0;
                }
                SoundList.Instance.SoundEffectPlay(2, 0.3f);
                StartCoroutine(ButtonDown(nextButton.gameObject));
            }
            else
            {
                nowNumber--;
                if (nowNumber < 0)
                {
                    nowNumber = this.modeNames.Count - 1;
                }
                SoundList.Instance.SoundEffectPlay(2, 0.3f);
                StartCoroutine(ButtonDown(backButton.gameObject));
            }
            if (textMoveCoroutine != null)
            {
                StopCoroutine(textMoveCoroutine);
            }
            textMoveCoroutine = StartCoroutine(TextChange(next));
            //onUpdateSelect?.Invoke(nowNumber);
            onUpdate.Invoke();
        }
    }

    public void Enter()
    {
        if(modeSelect == ModeSelect.NameChange)
        {
            SoundList.Instance.SoundEffectPlay(2, 0.3f);
            oldName = inputField.text;
            inputField.Select();
            onInputTextStart.Invoke();
        }
        else if(modeSelect == ModeSelect.Button)
        {
            if(uAction != null)
            {
                SoundList.Instance.SoundEffectPlay(2, 0.3f);
                uAction.Invoke();
            }
        }

    }

    void EndEdit()
    {
        SoundList.Instance.SoundEffectPlay(2, 0.3f);
        if(string.IsNullOrEmpty(inputField.text))//入力チェック
        {
            inputField.text = oldName;
        }
        Debug.Log(inputField.text);
        onUpdate.Invoke();
        onInputTextEnd.Invoke();
        
        
    }

    IEnumerator ButtonDown(GameObject button)
    {
        button.transform.DOScale(Vector2.one * 0.8f, textMoveSpeed).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(textMoveSpeed);
        button.transform.DOScale(Vector2.one, textMoveSpeed).SetEase(Ease.OutCubic);
        yield return null;
    }

    IEnumerator TextChange(bool next)
    {
        RectTransform rectTransform = selectText.GetComponent<RectTransform>();
        float plusMinus = 1;
        if(!next)
        {
            plusMinus = -1;
        }

        rectTransform.DOAnchorPosX(textMoveRange * plusMinus, textMoveSpeed).SetEase(Ease.OutCubic);
        selectText.GetComponent<Text>().DOFade(0, textMoveSpeed).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(textMoveSpeed);
        rectTransform.DOAnchorPosX(textMoveRange * -plusMinus, 0);
        selectText.text = modeNames[nowNumber];
        rectTransform.DOAnchorPosX(0, textMoveSpeed).SetEase(Ease.OutCubic);
        selectText.GetComponent<Text>().DOFade(1, textMoveSpeed).SetEase(Ease.OutCubic);


        yield return null;
    }
}
