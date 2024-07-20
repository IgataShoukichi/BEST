using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{

    [SerializeField] GameObject fillPanel;
    [SerializeField] GameObject text;
    public bool timerStarted = false;
    bool displayAsFloat = true;//floatで表示するか(falseならint)
    float startTime = 0;
    float maxTime = 0;
    float limitTime = 0;

    public UnityEvent onTimerFinished = new UnityEvent();



    void Start()
    {
        this.gameObject.SetActive(false);
    }

    
    void Update()
    {
        if (timerStarted)
        {
            limitTime -= Time.deltaTime;
            fillPanel.GetComponent<Image>().fillAmount = limitTime / maxTime;
            if (limitTime / maxTime <= 0.3f)
            {
                fillPanel.GetComponent<Image>().color = new Color32(230, 50, 0, 255);
            }
            else
            {
                fillPanel.GetComponent<Image>().color = new Color32(0, 230, 50, 255);
            }
            if (displayAsFloat)
            {
                text.GetComponent<Text>().text = (limitTime+1.0f).ToString("0.0");
            }
            else
            {
                text.GetComponent<Text>().text = ((int)limitTime+1.0f).ToString();
            }
            if (limitTime < 0.0f)
            {
                TimerStop();
            }
        }
    }

    public void TimerReset(float maxTime)
    {
        fillPanel.GetComponent<Image>().fillAmount = 1;
        text.GetComponent<Text>().text = maxTime.ToString();
    }
    public void TimerStart(float maxTime,bool displayAsFloat = true)
    {
        this.gameObject.SetActive(true);
        startTime = Time.deltaTime;
        this.maxTime = maxTime;
        this.limitTime = maxTime;
        this.displayAsFloat = displayAsFloat;
        timerStarted = true;
        Debug.Log("TimerStart");
    }
    public void TimerStop()
    {
        timerStarted = false;
        this.gameObject.SetActive(false);
        onTimerFinished.Invoke();//タイムアップ
        Debug.Log("TimerStop");
    }
}
