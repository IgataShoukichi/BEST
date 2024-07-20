using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.ComponentModel.Design;
using Unity.VisualScripting;

public class SwipeSelect : MonoBehaviour
{
    public Action<int> onUpdateSelect = null;

    [SerializeField] EventTrigger backButton;
    [SerializeField] EventTrigger nextButton;
    [SerializeField] Text selectText;
    List<string> modeNames = new List<string>();
    int nowModeSelectNumber = 0;

    Coroutine textMoveCoroutine = null;
    float textMoveRange = 10;
    float textMoveSpeed = 0.1f;

    void Awake()
    {

        EventTrigger.Entry entry1 = new EventTrigger.Entry();
        EventTrigger.Entry entry2 = new EventTrigger.Entry();
        entry1.eventID = EventTriggerType.PointerClick;
        entry2.eventID = EventTriggerType.PointerClick;
        entry1.callback.AddListener((eventDate) => { BackNext(true); });
        entry2.callback.AddListener((eventDate) => { BackNext(false); });
        nextButton.GetComponent<EventTrigger>().triggers.Add(entry1);
        backButton.GetComponent<EventTrigger>().triggers.Add(entry2);
        this.gameObject.SetActive(false);

    }

    public int SwipeSetting(string[] modeNames,int nowSelectNumber = 0)
    {
        this.modeNames.Clear();
        this.modeNames.AddRange(modeNames);
        nowModeSelectNumber = nowSelectNumber;
        selectText.text = modeNames[nowModeSelectNumber];
        this.gameObject.SetActive(true);

        return nowModeSelectNumber;
    }

    public int ReturnSelect()
    {
        return nowModeSelectNumber;
    }

    public void BackNext(bool next)
    {
        if (next)
        {
            nowModeSelectNumber++;
            if (nowModeSelectNumber > this.modeNames.Count - 1)
            {
                nowModeSelectNumber = 0;
            }
            SoundList.Instance.SoundEffectPlay(2,0.3f);
            StartCoroutine(ButtonDown(nextButton.gameObject));
        }
        else
        {
            nowModeSelectNumber--;
            if (nowModeSelectNumber < 0)
            {
                nowModeSelectNumber = this.modeNames.Count - 1;
            }
            SoundList.Instance.SoundEffectPlay(2,0.3f);
            StartCoroutine(ButtonDown(backButton.gameObject));
        }
        if(textMoveCoroutine != null)
        {
            StopCoroutine(textMoveCoroutine);
        }
        textMoveCoroutine = StartCoroutine(TextChange(next));
        onUpdateSelect?.Invoke(nowModeSelectNumber);
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
        selectText.text = modeNames[nowModeSelectNumber];
        rectTransform.DOAnchorPosX(0, textMoveSpeed).SetEase(Ease.OutCubic);
        selectText.GetComponent<Text>().DOFade(1, textMoveSpeed).SetEase(Ease.OutCubic);


        yield return null;
    }
}
