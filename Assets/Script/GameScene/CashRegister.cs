using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class CashRegister : MonoBehaviour
{
    //修正予定
    [System.NonSerialized] public UnityEvent onCashRegisterUpdate = new UnityEvent();
    [System.NonSerialized] public UnityEvent onGoodCashAdd = new UnityEvent();
    [System.NonSerialized] public UnityEvent onNormalCashAdd = new UnityEvent();
    [System.NonSerialized] public UnityEvent onAngerCashAdd = new UnityEvent();
    //[System.NonSerialized] public UnityEvent onAngerCashStop = new UnityEvent();

    [SerializeField] MiniAudio miniAudio;
    [SerializeField][Header("UI")] GameObject cashMark = null;
    [SerializeField] GameObject angerEffect;
    [SerializeField] Image iconImage;

    enum EvaluationStep
    {
        Null,
        Good,
        Normal,
        Bad
    }
    Dictionary<Family, EvaluationStep> nowEvaluationSteps = new Dictionary<Family, EvaluationStep>();
    Dictionary<Family, Coroutine> cashEvaluationCoroutines = new Dictionary<Family, Coroutine>();

    //old
    //Dictionary<Family,Coroutine> goodCashCoroutines = new Dictionary<Family,Coroutine>();
    //Dictionary<Family, Coroutine> angerCashCoroutines = new Dictionary<Family, Coroutine>();
    //Dictionary<Family, bool> goodCashFlags = new Dictionary<Family, bool>();
    //Dictionary<Family, bool> angerCashFlags = new Dictionary<Family, bool>();
    float goodCashTime = 0f;
    float angerCashTime = 0f;
    float angerCashInterval = 0f;

    float moveSpeed = 5f;

    [System.NonSerialized] public bool nowCheck = false;//会計ができるか
    [System.NonSerialized] public bool nowWorking = false;//現在作業しているか
    [System.NonSerialized] public bool nowCustomerMove = false;//現在レジにお客さんが向かっているか
    [SerializeField] Vector3[] customerWatingPosition;

    void Start()
    {
        cashMark.SetActive(false);
        angerEffect?.SetActive(false);
    }

    public void EvaluationSetting(float goodTime,float angerFirstTime,float angerInterval)
    {
        goodCashTime = goodTime;
        angerCashTime = angerFirstTime;
        angerCashInterval = angerInterval;
    }


    public void EvaluationStop()
    {
        Family family = GameVariable.nowRegisterWaitingCustomers[0];
        if (nowEvaluationSteps[family] != EvaluationStep.Null && cashEvaluationCoroutines[family] != null)
        {
            StopCoroutine(cashEvaluationCoroutines[family]);
        }
        /*
        if(!goodCashFlags[GameVariable.nowRegisterWaitingCustomers[0]] && goodCashCoroutines[GameVariable.nowRegisterWaitingCustomers[0]] != null)
        {
            StopCoroutine(goodCashCoroutines[GameVariable.nowRegisterWaitingCustomers[0]]);
        }
        if (!angerCashFlags[GameVariable.nowRegisterWaitingCustomers[0]] && angerCashCoroutines[GameVariable.nowRegisterWaitingCustomers[0]] != null)
        {
            StopCoroutine(angerCashCoroutines[GameVariable.nowRegisterWaitingCustomers[0]]);
        }
        */
    }

    public void EvaluationAnger()
    {
        EvaluationStop();
        Family family = GameVariable.nowRegisterWaitingCustomers[0];
        if(nowEvaluationSteps[family] != EvaluationStep.Bad)
        {
            cashEvaluationCoroutines.Remove(family);
            cashEvaluationCoroutines.Add(family, StartCoroutine(CashEvaluation(family, true, angerCashInterval)));
        }
        /*
        if (!angerCashFlags[GameVariable.nowRegisterWaitingCustomers[0]])
        {
            //angerCashCoroutines[GameVariable.nowRegisterWaitingCustomers[0]] = null;
            Family temp = GameVariable.nowRegisterWaitingCustomers[0];
            angerCashCoroutines.Remove(GameVariable.nowRegisterWaitingCustomers[0]);
            angerCashCoroutines.Add(temp, StartCoroutine(AngerCashDelay(0f, angerCashInterval, GameVariable.nowRegisterWaitingCustomers[0])));
        }
        */
    }

    IEnumerator CashEvaluation(Family family,bool firstAnger,float badInterval,float goodTime = 0,float badTime = 0)
    {
        if (!firstAnger)
        {
            nowEvaluationSteps[family] = EvaluationStep.Good;
            family.ChangeCustomerFace(0);
            yield return new WaitForSeconds(goodTime);
            nowEvaluationSteps[family] = EvaluationStep.Normal;
            family.ChangeCustomerFace(1);
            CheckCashMark();
            yield return new WaitForSeconds(badTime);
        }
        nowEvaluationSteps[family] = EvaluationStep.Bad;
        family.ChangeCustomerFace(2);
        GameVariable.AngerChangeCashCount(true);
        CheckCashMark();
        while (true)
        {
            onAngerCashAdd.Invoke();
            yield return new WaitForSeconds(badInterval);
        }
    }
    /*
    IEnumerator GoodCashDelay(float time, Family family)
    {
        yield return new WaitForSeconds(time);
        goodCashFlags[family] = true;
        CheckCashMark();
    }

    IEnumerator AngerCashDelay(float time,float interval, Family family)
    {
        yield return new WaitForSeconds(time);
        angerCashFlags[family] = true;
        GameVariable.AngerChangeCashCount(true);
        CheckCashMark();
        while (true)
        {
            onAngerCashAdd.Invoke();
            yield return new WaitForSeconds(interval);
        }
        
    }
    */
    void CheckCashMark()
    {
        if(nowEvaluationSteps.Count > 0)
        {
            if (!cashMark.activeSelf)
            {
                cashMark.SetActive(true);
            }
            if (nowEvaluationSteps.ContainsValue(EvaluationStep.Bad))
            {
                cashMark.GetComponent<Image>().color = new Color32(170, 0, 0, 255);
                angerEffect?.SetActive(true);
            }
            else if (nowEvaluationSteps.ContainsValue(EvaluationStep.Normal))
            {
                cashMark.GetComponent<Image>().color = new Color32(0, 150, 50, 255);
                angerEffect?.SetActive(false);
            }
            else if (nowEvaluationSteps.ContainsValue(EvaluationStep.Good))
            {
                cashMark.GetComponent<Image>().color = new Color32(0, 80, 150, 255);
                angerEffect?.SetActive(false);
            }
        }
        else
        {
            if (cashMark.activeSelf)
            {
                cashMark.SetActive(false);
                angerEffect?.SetActive(false);
            }
        }
        /*
        if (goodCashFlags.Count > 0)
        {
            if (!cashMark.activeSelf)
            {
                cashMark.SetActive(true);
            }
            if (angerCashFlags.ContainsValue(true))//マイナス
            {
                cashMark.GetComponent<Image>().color = new Color32(170, 0, 0, 255);
            }
            else if (goodCashFlags.ContainsValue(true))//ノーマル
            {
                cashMark.GetComponent<Image>().color = new Color32(0, 150, 50, 255);
            }
            else//プラス
            {
                cashMark.GetComponent<Image>().color = new Color32(0, 80, 150, 255);
            }
        }
        else
        {
            if (cashMark.activeSelf)
            {
                cashMark.SetActive(false);
            }
        }
        */
    }




    public void CompleteCash()
    {
        nowCheck = false;
        nowWorking = false;
        miniAudio?.SoundPlay(0, 0.5f);

        Family nowFamily = GameVariable.nowRegisterWaitingCustomers[0];
        StopCoroutine(cashEvaluationCoroutines[nowFamily]);
        cashEvaluationCoroutines.Remove(nowFamily);
        switch (nowEvaluationSteps[nowFamily])
        {
            case EvaluationStep.Good:
                onGoodCashAdd.Invoke();
                break;
            case EvaluationStep.Normal:
                onNormalCashAdd.Invoke();
                break;
            case EvaluationStep.Bad:
                GameVariable.AngerChangeCashCount(false);
                break;

        }
        nowEvaluationSteps.Remove(nowFamily);

        /*
        if (angerCashFlags[GameVariable.nowRegisterWaitingCustomers[0]])
        {
            GameVariable.AngerChangeCashCount(false);
        }
        else if (!goodCashFlags[GameVariable.nowRegisterWaitingCustomers[0]])
        {
            if(goodCashCoroutines[GameVariable.nowRegisterWaitingCustomers[0]] != null)
            {
                StopCoroutine(goodCashCoroutines[GameVariable.nowRegisterWaitingCustomers[0]]);
            }
            onGoodCashAdd.Invoke();
        }
        else
        {
            onNormalCashAdd.Invoke();
        }
        goodCashCoroutines.Remove(GameVariable.nowRegisterWaitingCustomers[0]);
        goodCashFlags.Remove(GameVariable.nowRegisterWaitingCustomers[0]);
        StopCoroutine(angerCashCoroutines[GameVariable.nowRegisterWaitingCustomers[0]]);
        angerCashCoroutines.Remove(GameVariable.nowRegisterWaitingCustomers[0]);
        angerCashFlags.Remove(GameVariable.nowRegisterWaitingCustomers[0]);
        */

        //Family family = GameVariable.nowCustomerScriptList.Find(n => n.name == GameVariable.nowRegisterWaitingCustomers[0].name);
        nowFamily.EndRegister();
        UpdateCustomers();

    }

    public void AddStandbyCustomers(Family family)
    {
        GameVariable.nowRegisterReadyCustomers.Add(family);
        CheckCustomer();
    }

    void CheckCustomer()//レジに行こうとしている人がいるか確認
    {
        if(GameVariable.isMasterClient)
        {
            if (GameVariable.nowRegisterReadyCustomers.Count > 0 && GameVariable.nowRegisterWaitingCustomers.Count < GameVariable.maxRegisterWaitingCustomers && !nowCustomerMove)
            {
                //GameVariable.nowRegisterWaitingCustomers.Add(GameVariable.nowRegisterReadyCustomers[0]);
                StartCoroutine(GameVariable.nowRegisterReadyCustomers[0].StandupCustomer());
                GameVariable.nowRegisterReadyCustomers.RemoveAt(0);
                nowCustomerMove = true;
            }
        }
    }

    void DestroyCustomer(Family family)
    {
        GameVariable.nowCustomerScriptList.Remove(family);
        Destroy(family.gameObject);
    }

    public void AddCustomers(Family family)
    {
        nowCheck = true;
        nowEvaluationSteps.Add(family, EvaluationStep.Null);
        cashEvaluationCoroutines.Add(family, StartCoroutine(CashEvaluation(family, false, angerCashInterval, goodCashTime, angerCashTime)));
        //goodCashFlags.Add(family, false);
        //angerCashFlags.Add(family, false);
        //goodCashCoroutines.Add(family, StartCoroutine(GoodCashDelay(goodCashTime, family)));
        //angerCashCoroutines.Add(family, StartCoroutine(AngerCashDelay(angerCashTime,angerCashInterval, family)));

        onCashRegisterUpdate.Invoke();
        nowCustomerMove = false;
        CheckCustomer();
        CheckCashMark();
    }

    public void UpdateCustomers()
    {
        GameVariable.nowRegisterWaitingCustomers.RemoveAt(0);
        if(GameVariable.nowRegisterWaitingCustomers.Count > 0)
        {
            foreach (Family temp in GameVariable.nowRegisterWaitingCustomers)
            {
                temp.registerWaitNumber--;
                temp.customerParentPos.GetComponent<NewCustomerController>().customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.walkCus);
                temp.customerParentPos.DOMove(GameVariable.customerMovePoint.GetRegisterRoute()[temp.registerWaitNumber], moveSpeed).SetSpeedBased().SetEase(Ease.Linear).OnComplete(() => SetLook(temp.customerParentPos, temp.registerWaitNumber));//一定速度
            }
        }
        else
        {
            nowCheck = false;
        }
        CheckCustomer();
        CheckCashMark();
    }

    void SetLook(Transform customer,int number)
    {
        Debug.Log("LookAt" + customer + "  " + number);
        Vector3 temp;
        customer.GetComponent<NewCustomerController>().customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.neutralCus);
        if (number == 0)
        {
            temp = this.transform.position;
        }
        else
        {
            temp = GameVariable.customerMovePoint.GetRegisterRoute()[number - 1];
        }
        temp.y = customer.position.y;
        customer.LookAt(temp);
        nowCheck = true;
        onCashRegisterUpdate.Invoke();
    }
}
