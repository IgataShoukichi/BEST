using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class ButtonCoolTime : MonoBehaviour
{
    [System.NonSerialized] public UnityEvent onStartCoolTime = new UnityEvent();
    [System.NonSerialized] public UnityEvent onFinishCoolTime = new UnityEvent();
    Button baseButton;
    [SerializeField] Image coolTimeImage;

    void Start()
    {
        if (GetComponent<Button>())
        {
            baseButton = GetComponent<Button>();
        }
        coolTimeImage.fillAmount = 0;
    }

    public void CoolTimeSetting(float time,Color32 colorOn, Color32 colorOff)
    {
        onStartCoolTime.Invoke();
        if(baseButton != null)
        {
            baseButton.enabled = false;
        }
        coolTimeImage.fillAmount = 1;
        coolTimeImage.DOFillAmount(0, time).SetEase(Ease.Linear).OnComplete(() => FinishCoolTime());
    }

    void FinishCoolTime()
    {
        if(baseButton != null)
        {
            baseButton.enabled = true;
        }
        onFinishCoolTime.Invoke();
    }
}
