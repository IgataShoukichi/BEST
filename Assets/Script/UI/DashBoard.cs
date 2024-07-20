using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

public class DashBoard : MonoBehaviour
{
    [SerializeField] GameObject timerHand;
    [SerializeField] Text timerText;
    [SerializeField] Text waveName;
    [SerializeField] Text waveCount;

    [System.NonSerialized]public List<string> waveNames = new List<string>();

    public bool timerStarted = false;
    bool timerSerReady = false;//これをTrueにしないとスタート出来ない
    float setStartTime = 0;
    float maxTime = 0;
    float nowTime = 0;
    float resetWaveInterval = 0;
    float maxWaveInterval = 0;
    float nowWaveInterval = 0;
    int nowWaveCount = 0;
    int maxWaveCount = 0;

    [System.NonSerialized] public UnityEvent onUpdateWave = new UnityEvent();
    [System.NonSerialized] public UnityEvent onTimerFinished = new UnityEvent();



    void Start()
    {
        //this.gameObject.SetActive(false);
        timerHand.transform.rotation = Quaternion.identity;
        timerText.text = "";
    }


    void Update()
    {
        if (timerStarted)
        {
            //nowTime = Mathf.Max(0f, (unchecked(PhotonNetwork.ServerTimestamp) / 1000f) - setStartTime);
            //nowTime = (PhotonNetwork.ServerTimestamp / 1000f) - setStartTime; 
            //nowWaveInterval = (PhotonNetwork.ServerTimestamp / 1000f) - resetWaveInterval;
            nowTime += Time.deltaTime;
            nowWaveInterval += Time.deltaTime;
            timerHand.transform.rotation = Quaternion.Euler(Vector3.Lerp(Vector3.zero, Vector3.back * 180, nowTime / maxTime));
            timerText.text = $"{(int)((maxTime + 1 - nowTime) / 60)}:{((int)((maxTime + 1 - nowTime) % 60)).ToString("00")}";

            if (nowWaveInterval >= maxWaveInterval && nowWaveCount < maxWaveCount)
            {
                nowWaveInterval = 0f;
                resetWaveInterval = nowTime;
                WaveUpdate();
            }

            if (nowTime >= maxTime)
            {
                TimerStop();
            }
        }
    }

    public void TimerReady(float maxTime, int maxWaveCount, List<string> waveNames)
    {
        timerHand.transform.rotation = Quaternion.identity;
        this.maxTime = maxTime;
        this.nowTime = 0f;
        nowWaveInterval = 0f;
        maxWaveInterval = maxTime / (float)maxWaveCount;
        nowWaveCount = 0;
        this.maxWaveCount = maxWaveCount;
        this.waveNames = new List<string>(waveNames);
        WaveUpdate();
        timerSerReady = true;
    }
    public void TimerStart()
    {
        if (timerSerReady)
        {
            //setStartTime = PhotonNetwork.ServerTimestamp / 1000f; //ミリ秒を直す
            //resetWaveInterval = setStartTime;
            timerSerReady = false;
            //this.gameObject.SetActive(true);
            timerStarted = true;
            Debug.Log("TimerStart");
        }
        else
        {
            Debug.LogWarning("TimerReadyを実行してください");
        }
    }
    public void TimerStop(bool normalFinish = true)
    {
        timerStarted = false;
        //this.gameObject.SetActive(false);
        Debug.Log("TimerStop");
        if (normalFinish)
        {
            onTimerFinished.Invoke();//タイムアップ
        }
    }

    public int NowWaveCount()
    {
        return nowWaveCount;
    }

    void WaveUpdate()
    {
        if(waveCount != null)
        {
            waveCount.text = $"Wave {nowWaveCount + 1}/{maxWaveCount}";
        }
        if(waveName != null)
        {
            waveName.text = waveNames[nowWaveCount];
        }
        nowWaveCount++;
        onUpdateWave.Invoke();
    }
}
