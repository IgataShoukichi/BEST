using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvaluationAnger : MonoBehaviour
{
    Color32 angerColor = new Color32(170, 0, 0, 255);
    Color32 normalColor = new Color32(150, 150, 150, 255);


    [SerializeField] Image guideMark;
    [SerializeField] Text guideNumber;
    [SerializeField] Image callMark;
    [SerializeField] Text callNumber;
    [SerializeField] Image orderMark;
    [SerializeField] Text orderNumber;
    [SerializeField] Image cashMark;
    [SerializeField] Text cashNumber;


    void Start()
    {
        EvaluationChange();
    }

    public void EvaluationChange()
    {
        IconChange(GameVariable.angerGuideCount, guideMark, guideNumber);
        IconChange(GameVariable.angerCallCount, callMark, callNumber);
        IconChange(GameVariable.angerOrderCount, orderMark, orderNumber);
        IconChange(GameVariable.angerCashCount, cashMark, cashNumber);
    }

    void IconChange(int count,Image mark,Text number)
    {
        mark.color = count <= 0 ? normalColor : angerColor;
        number.text = count.ToString();
    }
}
