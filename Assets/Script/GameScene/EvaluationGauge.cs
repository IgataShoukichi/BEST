using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using Unity.VisualScripting;

public class EvaluationGauge : MonoBehaviour
{
    [System.NonSerialized] public UnityEvent onMinEnterGauge = new UnityEvent();//最小値以下になったとき
    [System.NonSerialized] public UnityEvent onMinExitGauge = new UnityEvent();//最小値以下じゃなくなった時
    [System.NonSerialized] public UnityEvent onMaxEnterGauge = new UnityEvent();//最大値以上になったとき
    [System.NonSerialized] public UnityEvent onMaxExitGauge = new UnityEvent();//最大値以上じゃなくなった時

    [SerializeField] GameObject gaugeBase;//ゲージのベース
    [SerializeField] GameObject needle;//針

    [SerializeField] Image gaugeImage;
    [SerializeField] Text percentText;

    float minGauge = 0f;
    float maxGauge = 0f;

    int minGaugeValue = 0;//最小値
    int maxGaugeValue = 0;//最大値
    int setGaugeValue = 0;//セットされた目標値
    int nowGaugeValue = 0;//現在の値

    float delayTime = 0.25f;//値を変更するまでの遅延
    Coroutine coroutine = null;//コルーチン
    bool isSet = false;//セットが完了しているか
    bool isMin = false;//最小値以下になったか
    bool isMax = false;//最大値以上になったか

    void Start()
    {
        //gaugeImage.fillAmount = 0;
    }

    public void SettingGauge(int minValue, int maxValue,int nowValue)
    {
        minGaugeValue = minValue;
        maxGaugeValue = maxValue;
        nowGaugeValue = nowValue;
        minGauge = -gaugeBase.GetComponent<RectTransform>().rect.width / 2;
        maxGauge = gaugeBase.GetComponent<RectTransform>().rect.width / 2;
        float nowValueClamp01 = Mathf.Clamp01((float)nowGaugeValue / (float)maxGaugeValue);
        needle.GetComponent<RectTransform>().DOLocalMoveX(Mathf.Lerp(minGauge, maxGauge, nowValueClamp01), 0);
        //gaugeImage.fillAmount = 0;
        //percentText.text = "0%";
        isSet = true;
        isMin = false;
        isMax = false;
    }

    public void ChangeGauge(int setValue)
    {
        if(isSet)
        {
            setGaugeValue = setValue;
            if(coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(GaugeValueChangeDelay());
        }
    }


    IEnumerator GaugeValueChangeDelay()
    {
        while(nowGaugeValue != setGaugeValue)
        {
            //比較と加算減算
            nowGaugeValue = nowGaugeValue < setGaugeValue ? ++nowGaugeValue : 
                (nowGaugeValue > setGaugeValue ? --nowGaugeValue : nowGaugeValue);
            //最小値検知
            if(nowGaugeValue <= minGaugeValue && !isMin)
            {
                onMinEnterGauge.Invoke();
                isMin = true;
            }
            else if(nowGaugeValue > minGaugeValue && isMin)
            {
                onMinExitGauge.Invoke();
                isMin = false;
            }
            //最大値検知
            if(nowGaugeValue >= maxGaugeValue && !isMax)
            {
                onMaxEnterGauge.Invoke();
                isMax = true;
            }
            else if (nowGaugeValue < maxGaugeValue && isMax)
            {
                onMaxExitGauge.Invoke();
                isMax = false;
            }
            //0～1に変換
            float nowValueClamp01 = Mathf.Clamp01((float)nowGaugeValue / (float)maxGaugeValue);

            //針の位置を変更
            needle.GetComponent<RectTransform>().DOLocalMoveX(Mathf.Lerp(minGauge,maxGauge,nowValueClamp01),delayTime).SetEase(Ease.Linear);

            //ClampしてFillAmountを変える
            //gaugeImage.DOFillAmount(nowValueClamp01, delayTime).SetEase(Ease.Linear);
            yield return new WaitForSeconds(delayTime);
            //パーセント表示
            //percentText.text = (nowValueClamp01 * 100) + "%";
            
        }
        yield return null;
    }

}
