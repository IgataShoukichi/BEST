using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using Unity.VisualScripting;

public class EvaluationGauge : MonoBehaviour
{
    [System.NonSerialized] public UnityEvent onMinEnterGauge = new UnityEvent();//�ŏ��l�ȉ��ɂȂ����Ƃ�
    [System.NonSerialized] public UnityEvent onMinExitGauge = new UnityEvent();//�ŏ��l�ȉ�����Ȃ��Ȃ�����
    [System.NonSerialized] public UnityEvent onMaxEnterGauge = new UnityEvent();//�ő�l�ȏ�ɂȂ����Ƃ�
    [System.NonSerialized] public UnityEvent onMaxExitGauge = new UnityEvent();//�ő�l�ȏザ��Ȃ��Ȃ�����

    [SerializeField] GameObject gaugeBase;//�Q�[�W�̃x�[�X
    [SerializeField] GameObject needle;//�j

    [SerializeField] Image gaugeImage;
    [SerializeField] Text percentText;

    float minGauge = 0f;
    float maxGauge = 0f;

    int minGaugeValue = 0;//�ŏ��l
    int maxGaugeValue = 0;//�ő�l
    int setGaugeValue = 0;//�Z�b�g���ꂽ�ڕW�l
    int nowGaugeValue = 0;//���݂̒l

    float delayTime = 0.25f;//�l��ύX����܂ł̒x��
    Coroutine coroutine = null;//�R���[�`��
    bool isSet = false;//�Z�b�g���������Ă��邩
    bool isMin = false;//�ŏ��l�ȉ��ɂȂ�����
    bool isMax = false;//�ő�l�ȏ�ɂȂ�����

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
            //��r�Ɖ��Z���Z
            nowGaugeValue = nowGaugeValue < setGaugeValue ? ++nowGaugeValue : 
                (nowGaugeValue > setGaugeValue ? --nowGaugeValue : nowGaugeValue);
            //�ŏ��l���m
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
            //�ő�l���m
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
            //0�`1�ɕϊ�
            float nowValueClamp01 = Mathf.Clamp01((float)nowGaugeValue / (float)maxGaugeValue);

            //�j�̈ʒu��ύX
            needle.GetComponent<RectTransform>().DOLocalMoveX(Mathf.Lerp(minGauge,maxGauge,nowValueClamp01),delayTime).SetEase(Ease.Linear);

            //Clamp����FillAmount��ς���
            //gaugeImage.DOFillAmount(nowValueClamp01, delayTime).SetEase(Ease.Linear);
            yield return new WaitForSeconds(delayTime);
            //�p�[�Z���g�\��
            //percentText.text = (nowValueClamp01 * 100) + "%";
            
        }
        yield return null;
    }

}
