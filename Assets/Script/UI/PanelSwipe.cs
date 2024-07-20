using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class PanelSwipe : MonoBehaviour
{
    [SerializeField] EventTrigger backButton;
    [SerializeField] EventTrigger nextButton;
    [SerializeField] Image imagePanel;
    [SerializeField] Text panelCountText;
    [SerializeField] List<Sprite> SpriteList;
    int nowSelectNumber = 0;

    Coroutine panelMoveCoroutine = null;
    float panelMoveRange = 10;
    float panelMoveSpeed = 0.1f;

    void Awake()
    {
        this.gameObject.SetActive(true);
        EventTrigger.Entry entry1 = new EventTrigger.Entry();
        EventTrigger.Entry entry2 = new EventTrigger.Entry();
        entry1.eventID = EventTriggerType.PointerClick;
        entry2.eventID = EventTriggerType.PointerClick;
        entry1.callback.AddListener((eventDate) => { BackNext(true); });
        entry2.callback.AddListener((eventDate) => { BackNext(false); });
        nextButton.GetComponent<EventTrigger>().triggers.Add(entry1);
        backButton.GetComponent<EventTrigger>().triggers.Add(entry2);
        nowSelectNumber = 0;
        imagePanel.sprite = SpriteList[nowSelectNumber];
        panelCountText.text = $"{nowSelectNumber+1}/{SpriteList.Count}";
    }

    public void PanelSelect(int number)
    {
        nowSelectNumber = number;
        imagePanel.sprite = SpriteList[nowSelectNumber];
    }

    public void BackNext(bool next)
    {
        if (panelMoveCoroutine != null)
        {
            StopCoroutine(panelMoveCoroutine);
        }
        if (next)
        {
            nowSelectNumber++;
            if (nowSelectNumber > SpriteList.Count - 1)
            {
                nowSelectNumber = SpriteList.Count - 1;
            }
            else
            {
                SoundList.Instance.SoundEffectPlay(2, 0.3f);
                StartCoroutine(ButtonDown(nextButton.gameObject));
                panelMoveCoroutine = StartCoroutine(PanelChange(next));
            }
        }
        else
        {
            nowSelectNumber--;
            if (nowSelectNumber < 0)
            {
                nowSelectNumber = 0;
            }
            else
            {
                SoundList.Instance.SoundEffectPlay(2, 0.3f);
                StartCoroutine(ButtonDown(backButton.gameObject));
                panelMoveCoroutine = StartCoroutine(PanelChange(next));
            }
        }
    }

    IEnumerator ButtonDown(GameObject button)
    {
        button.transform.DOScale(Vector2.one * 0.8f, panelMoveSpeed).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(panelMoveSpeed);
        button.transform.DOScale(Vector2.one, panelMoveSpeed).SetEase(Ease.OutCubic);
        yield return null;
    }

    IEnumerator PanelChange(bool next)
    {
        RectTransform rectTransform = imagePanel.GetComponent<RectTransform>();
        float plusMinus = 1;
        if(!next)
        {
            plusMinus = -1;
        }

        rectTransform.DOAnchorPosX(panelMoveRange * plusMinus, panelMoveSpeed).SetEase(Ease.OutCubic);
        imagePanel.DOFade(0, panelMoveSpeed).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(panelMoveSpeed);
        rectTransform.DOAnchorPosX(panelMoveRange * -plusMinus, 0);
        imagePanel.sprite = SpriteList[nowSelectNumber];
        rectTransform.DOAnchorPosX(0, panelMoveSpeed).SetEase(Ease.OutCubic);
        imagePanel.DOFade(1, panelMoveSpeed).SetEase(Ease.OutCubic);
        panelCountText.text = $"{nowSelectNumber + 1}/{SpriteList.Count}";

        yield return null;
    }
}
