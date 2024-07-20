using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Table : MonoBehaviour
{
    [System.NonSerialized] public UnityEvent onOrderCreated = new UnityEvent();

    [System.NonSerialized] public UnityEvent onGoodCallAdd = new UnityEvent();
    [System.NonSerialized] public UnityEvent onGoodOrderAdd = new UnityEvent();
    [System.NonSerialized] public UnityEvent onNormalCallAdd = new UnityEvent();
    [System.NonSerialized] public UnityEvent onNormalOrderAdd = new UnityEvent();
    [System.NonSerialized] public UnityEvent onAngerCallAdd = new UnityEvent();
    //[System.NonSerialized] public UnityEvent onAngerCallStop = new UnityEvent();
    [System.NonSerialized] public UnityEvent onAngerOrderAdd = new UnityEvent();
    //[System.NonSerialized] public UnityEvent onAngerOrderStop = new UnityEvent();

    //�]���֌W
    enum EvaluationMode
    {
        Null,
        Call,
        Order
    };
    EvaluationMode nowEvaluationMode = EvaluationMode.Null;

    enum EvaluationStep
    {
        Null,
        Good,
        Normal,
        Bad
    }
    EvaluationStep nowCallEvaluationStep = EvaluationStep.Null;
    EvaluationStep nowOrderEvaluationStep = EvaluationStep.Null;
    Coroutine callEvaluationCoroutine = null;
    Coroutine orderEvaluationCoroutine = null;
    



    //Old
    //Coroutine goodCallCoroutine = null;
    //Coroutine goodOrderCoroutine = null;

    //Coroutine angerCallCoroutine = null;
    //Coroutine angerOrderCoroutine = null;
    //bool goodCallFlag = false;
    //bool goodOrderFlag = false;
    //bool angerCallFlag = false;
    //bool angerOrderFlag = false;
    float goodCallTime = 0;
    float goodOrderTime = 0;
    float angerCallFirstTime = 0f;
    float angerOrderFirstTime = 0f;
    float angerCallInterval = 0;
    float angerOrderInterval = 0;

    //float limitDistance = 3f;

    [System.NonSerialized] public PlayerRPC playerRPC;
    public int tableNumber;
    public Menu menu;
    public FoodStandManager foodStandManager;
    [SerializeField] List<Transform> customerPositions;
    [SerializeField] List<Transform> customerStandingPositions;
    [SerializeField] List<Transform> foodPositions;
    [SerializeField] List<Transform> waterPositions;

    [SerializeField][Header("UI")] GameObject tableMark;
    [SerializeField] GameObject angerEffect;
    [SerializeField] Image iconImage;
    [SerializeField] Text tableNumberText;
    [SerializeField] List<Sprite> tableIcon;

    //Customer
    [SerializeField][Header("�A�蓹")] List<int> returnRoad = new List<int>();
    int returnRoadPathCount = 0;//DOPath�p

    List<Food> foodOrderList = new List<Food>();//������������
    int nowFoodCount = 0;//���݂̗����̐�
    public bool usedTable = false;//�e�[�u���ɂ��q���񂪂��邩
    int usedCustomerNumber = -1;//�����Ă�q�̒c�̊Ǘ��ԍ�
    Family nowFamilyScript;//���ݍ����Ă���q�̃X�N���v�g

    GameObject wagonStandby;//�҂��Ă��郏�S��
    float wagonDistance = 3.5f;//���S��������������̋���
    bool nowWaitWagon = false;//���݃��S����҂��Ă��邩

    public bool nowOrder = false;//�I�[�_�[���悤�Ƃ��Ă��邩
    public bool finishEat = false;//�H�׏I�������
    public bool cleanpuTrue = false;//�Еt���ł��邩
    public bool putWater = false;//����u������
    Tween tween;

    void Start()
    {
        returnRoadPathCount = returnRoad.Count;
        tableMark.SetActive(false);
        angerEffect?.SetActive(false);
        foreach (Transform temp in waterPositions)
        {
            temp.gameObject.SetActive(false);
            temp.GetChild(0).GetChild(1).gameObject.SetActive(true);
        }
        nowFoodCount = 0;
    }

    void Update()
    {
        if(nowWaitWagon && wagonStandby != null)
        {
            if(Vector3.Distance(this.gameObject.transform.position, wagonStandby.transform.position) < wagonDistance)
            {
                nowWaitWagon = false;
                if (GameVariable.isMasterClient)
                {
                    playerRPC.PutWagonTable(wagonStandby.name, tableNumber.ToString());
                }
            }
        }
    }

    public void TableSetting()
    {
        tableNumberText.text = tableNumber.ToString();
    }

    public int GetCustomerMaxCount()
    {
        return customerPositions.Count;
    }

    public bool GetNowSitdownCustomer()
    {
        return usedTable;
    }

    public void EvaluationCallSetTime(float goodFirstTime,float firstTime,float interval)
    {
        goodCallTime = goodFirstTime;
        angerCallFirstTime = firstTime;
        angerCallInterval = interval;
    }

    public void EvaluationOrderSetTime(float goodFirstTime, float firstTime,float interval)
    {
        goodOrderTime = goodFirstTime;
        angerOrderFirstTime = firstTime;
        angerOrderInterval = interval;
    }

    void Call()
    {
        iconImage.sprite = tableIcon[0];
        tableMark.SetActive(true);
        //tableMark.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        tableMark.GetComponent<Image>().color = new Color32(0, 80, 150, 255);
        //tween = orderMark.GetComponent<RectTransform>().DOAnchorPosY(0.2f,0.5f).SetEase(Ease.InOutCubic).SetLoops(-1,LoopType.Yoyo);
        nowOrder = true;
        menu.CallCreate(tableNumber);
        SoundList.Instance.SoundEffectPlay(5, 0.3f);

        //goodCallFlag = false;
        //angerCallFlag = false;
        //goodCallCoroutine = StartCoroutine(GoodCallDelay(goodCallTime));
        //angerCallCoroutine = StartCoroutine(AngerCallDelay(angerCallFirstTime,angerCallInterval));
        nowEvaluationMode = EvaluationMode.Call;
        callEvaluationCoroutine = StartCoroutine(CallEvaluation(false, angerCallInterval, goodCallTime, angerCallFirstTime));
        onOrderCreated.Invoke();

    }

    public void CreateOrder()
    {
        //�]��
        StopCoroutine(callEvaluationCoroutine);
        callEvaluationCoroutine = null;
        tableMark.SetActive(false);
        angerEffect?.SetActive(false);
        switch (nowCallEvaluationStep)
        {
            case EvaluationStep.Good:
                onGoodCallAdd.Invoke();
                break;
            case EvaluationStep.Normal:
                onNormalCallAdd.Invoke();
                break;
            case EvaluationStep.Bad:
                GameVariable.AngerChangeCallCount(false);
                break;

        }
        nowCallEvaluationStep = EvaluationStep.Null;
        /*
        if (angerCallFlag)
        {
            GameVariable.AngerChangeCallCount(false);
        }
        else if (!goodCallFlag)
        {
            if(goodCallCoroutine != null)
            {
                StopCoroutine(goodCallCoroutine);
            }
            onGoodCallAdd.Invoke();
        }
        else
        {
            onNormalCallAdd.Invoke();
        }
        goodCallCoroutine = null;
        goodCallFlag = false;
        StopCoroutine(angerCallCoroutine);
        angerCallFlag = false;
        angerCallCoroutine = null;
        */
        //if (tween != null)
        //{
        //tween.Kill();
        //tween = null;
        //tableMark.SetActive(false);
        //}
        nowOrder = false;
        if (usedCustomerNumber > -1)
        {
            iconImage.sprite = tableIcon[1];
            tableMark.SetActive(true);
            //tableMark.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            tableMark.GetComponent<Image>().color = new Color32(0, 80, 150, 255);
            foreach (int foodNumber in GameVariable.customerDataBase.GetCustomer(usedCustomerNumber).GetCustomerOrderFoodNumber())
            {
                foodOrderList.Add(GameVariable.foodDataBase.GetFood(foodNumber));
            }
            nowFoodCount = 0;
            foodStandManager.FoodCreate(tableNumber,foodOrderList);
            menu.OrderCreate(tableNumber, foodOrderList);

            //goodOrderFlag = false;
            //angerOrderFlag = false;
            //goodOrderCoroutine = StartCoroutine(GoodOrderDelay(goodOrderTime));
            //angerOrderCoroutine = StartCoroutine(AngerOrderDelay(angerOrderFirstTime,angerOrderInterval));
            orderEvaluationCoroutine = StartCoroutine(OrderEvaluation(false, angerOrderInterval, goodOrderTime, angerOrderFirstTime));
            nowEvaluationMode = EvaluationMode.Order;
        }
        nowFamilyScript.lookMenuAnimation(false);
    }

    public void WagonReady(GameObject wagonObject)
    {
        wagonStandby = wagonObject;
        nowWaitWagon = true;

    }

    public bool SetFood()
    {
        if (foodOrderList.Count > 1)//�m�[�}���T�C�Y
        {
            foreach (Food food in foodOrderList)
            {
                foreach (Transform foodPosition in foodPositions)
                {
                    if (foodPosition.childCount <= 0)
                    {
                        GameObject prefab = food.GetFoodModel();//���f�����f�[�^�x�[�X����擾
                        GameObject createObject = Instantiate(prefab);
                        createObject.transform.SetParent(foodPosition.transform, true);
                        createObject.name = food.GetFoodNumber().ToString();//�t�[�h�ԍ�
                        createObject.transform.localPosition = Vector3.zero;
                        createObject.transform.localRotation = Quaternion.identity;
                        createObject.transform.localScale = prefab.transform.localScale / 2;
                        GameVariable.foodCount++;
                        nowFoodCount++;
                        break;
                    }
                }
            }
        }
        else if (foodOrderList.Count <= 1 && foodOrderList[0].GetFoodBig())//�߂���f�J�T�C�Y
        {
            Food food = foodOrderList[0];
            if (food.GetFoodBig())
            {
                GameObject prefab = food.GetFoodModel();//���f�����f�[�^�x�[�X����擾
                GameObject createObject = Instantiate(prefab);
                createObject.transform.SetParent(foodPositions[foodPositions.Count - 1].transform, true);
                createObject.name = food.GetFoodNumber().ToString();//�t�[�h�ԍ�
                createObject.transform.localPosition = Vector3.zero;
                createObject.transform.localRotation = Quaternion.identity;
                createObject.transform.localScale = prefab.transform.localScale / 2;
                GameVariable.foodCount++;
                nowFoodCount++;
                nowWaitWagon = false;
            }
        }
        //�]��
        nowEvaluationMode = EvaluationMode.Null;
        StopCoroutine(orderEvaluationCoroutine);
        orderEvaluationCoroutine = null;
        tableMark.SetActive(false);
        angerEffect?.SetActive(false);
        switch (nowOrderEvaluationStep)
        {
            case EvaluationStep.Good:
                onGoodOrderAdd.Invoke();
                break;
            case EvaluationStep.Normal:
                onNormalOrderAdd.Invoke();
                break;
            case EvaluationStep.Bad:
                GameVariable.AngerChangeOrderCount(false);
                break;

        }
        nowOrderEvaluationStep = EvaluationStep.Null;
        /*
        if (angerOrderFlag)
        {
            GameVariable.AngerChangeOrderCount(false);
        }
        else if (!goodOrderFlag)
        {
            if(goodOrderCoroutine != null)
            {
                StopCoroutine(goodOrderCoroutine);
            }
            onGoodOrderAdd.Invoke();
        }
        else
        {
            onNormalOrderAdd.Invoke();
        }
        tableMark.SetActive(false);
        goodOrderCoroutine = null;
        goodOrderFlag = false;
        StopCoroutine(angerOrderCoroutine);
        angerOrderFlag = false;
        angerOrderCoroutine = null;
        */
        menu.UpdateOrders(tableNumber);
        //�H�׎n��
        Invoke(nameof(FinishEat), GameVariable.customerDataBase.GetCustomer(usedCustomerNumber).GetCustomerEatingEndTime());
        nowFamilyScript.eatAnimation(true);
        return true;
        /*
        Debug.Log(foodObject.name);
        if (0 < foodOrderList.Count)
        {
            foreach (Transform foodPosition in foodPositions)
            {
                if (foodPosition.childCount <= 0)
                {
                    foodObject.transform.SetParent(foodPosition.transform, true);
                    foodObject.transform.localPosition = Vector3.zero;
                    foodObject.transform.localRotation = Quaternion.identity;
                    break;
                }
            }
            string[] foodInformation = foodObject.name.Split(',');
            UpdateOrder(int.Parse(foodInformation[0]));
            //nowFoodCount++;
            return true;
        }
        return false;
        */
    }

    public void SetWater()
    {
        putWater = true;
        foreach (Transform temp in waterPositions)//�R�b�v������ɂ���
        {
            temp.GetChild(0).GetChild(1).gameObject.SetActive(true);
        }
        for (int i = 0;i < GameVariable.customerDataBase.GetCustomer(usedCustomerNumber).GetCustomerDetail().Count; i++)
        {
            waterPositions[i].gameObject.SetActive(true);
        }
        //�����ɐ���u���������
    }

    public bool FoodPutInformation(string tableName)
    {
        string[] tableInformation = tableName.Split(',');
        if (int.Parse(tableInformation[0]) == tableNumber && nowFoodCount <= 0)
        {
            return true;
        }
        return false;
    }

    public void CleanupTable()
    {
        foreach (Transform foodPosition in foodPositions)
        {
            if (foodPosition.childCount > 0)
            {
                //GameObject temp = GameVariable.foodObjectList.Find(n => n.name == foodPosition.GetChild(0).gameObject.name);
                //GameVariable.foodObjectList.Remove(foodPosition.GetChild(0).gameObject);
                Destroy(foodPosition.GetChild(0).gameObject);
            }
        }
        foreach (Transform temp in waterPositions)
        {
            temp.gameObject.SetActive(false);
        }
        foodOrderList.Clear();
        nowFoodCount = 0;
        usedTable = false;
        finishEat = false;
        putWater = false;
        cleanpuTrue = false;
    }

    

    //��
    public bool SitdownCustomers(int customerNumber)
    {
        if (!usedTable && customerNumber >= 0)
        {
            usedTable = true;
            usedCustomerNumber = customerNumber;

            nowFamilyScript = GameVariable.nowCustomerScriptList.Find(n => n.gameObject.name == customerNumber.ToString());
            nowFamilyScript.SitdownCustomer(this.GetComponent<Table>(),customerStandingPositions);
            int count = 0;
            foreach(GameObject customer in nowFamilyScript.childCustomer)
            {
                customer.transform.SetParent(customerPositions[count]);
                customer.transform.localPosition = Vector3.zero;
                customer.transform.localRotation = Quaternion.identity;
                count++;
            }
            SetWater();//��
            Invoke(nameof(Call), GameVariable.customerDataBase.GetCustomer(usedCustomerNumber).GetCustomerOrderTime());
            return true;
        }
        else
        {
            return false;
        }
    }

    //��
    public void FinishEat()
    {
        if (usedTable)
        {
            if(nowFamilyScript != null)
            {
                nowFamilyScript.eatAnimation(false);
            }
            
            //�H�׏I�����I�u�W�F�N�g�ɂ���
            foreach (Transform foodPosition in foodPositions)
            {
                if (foodPosition.childCount > 0)
                {
                    foodPosition.GetChild(0).GetChild(1).gameObject.SetActive(false);
                    //GameObject temp = GameVariable.foodObjectList.Find(n => n.name == foodPosition.GetChild(0).gameObject.name);
                    //GameVariable.foodObjectList.Remove(temp);

                    //foodObject.transform.SetParent(foodPosition.transform, true);
                    //foodObject.transform.localPosition = Vector3.zero;
                    //foodObject.transform.rotation = Quaternion.identity;
                }
            }
            foreach (Transform temp in waterPositions)//�R�b�v������ɂ���
            {
                temp.GetChild(0).GetChild(1).gameObject.SetActive(false);
            }
            if (GameVariable.isMasterClient)
            {
                nowFamilyScript.AddStandbyCustomers();
                //StartCoroutine(StandupReadyCustomers());
            }
        }
    }

    public void StandupCustomers()
    {
        finishEat = true;
        cleanpuTrue = true;
        nowFamilyScript = null;
        usedCustomerNumber = -1;
    }


    public void EvaluationStop()
    {
        switch (nowEvaluationMode)
        {
            case EvaluationMode.Call:
                if(nowCallEvaluationStep != EvaluationStep.Null && callEvaluationCoroutine != null)
                {
                    StopCoroutine(callEvaluationCoroutine);
                }
                //if (!goodCallFlag && goodCallCoroutine != null)
                //{
                //    StopCoroutine(goodCallCoroutine);
                //}
                //if (!angerCallFlag && angerCallCoroutine != null)
                //{
                //    StopCoroutine(angerCallCoroutine);
                //}
                break;
            case EvaluationMode.Order:
                if (nowOrderEvaluationStep != EvaluationStep.Null && orderEvaluationCoroutine != null)
                {
                    StopCoroutine(orderEvaluationCoroutine);
                }
                //if (!goodOrderFlag && goodOrderCoroutine != null)
                //{
                //    StopCoroutine(goodOrderCoroutine);
                //}
                //if (!angerOrderFlag && angerOrderCoroutine != null)
                //{
                //    StopCoroutine(angerOrderCoroutine);
                //}
                break;
        }
    }

    public void EvaluationAnger()
    {
        EvaluationStop();
        switch (nowEvaluationMode)
        {
            case EvaluationMode.Call:
                if (nowCallEvaluationStep != EvaluationStep.Bad)
                {
                    //angerCallCoroutine = StartCoroutine(AngerCallDelay(0f, angerCallInterval));
                    callEvaluationCoroutine = StartCoroutine(CallEvaluation(true, angerCallInterval));
                }
                break;
            case EvaluationMode.Order:
                if (nowOrderEvaluationStep != EvaluationStep.Bad)
                {
                    //angerOrderCoroutine = StartCoroutine(AngerOrderDelay(0f, angerOrderInterval));
                    orderEvaluationCoroutine = StartCoroutine(OrderEvaluation(true, angerOrderInterval));
                }
                break;
        }

    }

    IEnumerator CallEvaluation(bool firstAnger, float badInterval, float goodTime = 0,float badTime = 0)
    {
        if (!firstAnger)
        {
            nowCallEvaluationStep = EvaluationStep.Good;
            nowFamilyScript.ChangeCustomerFace(0);
            yield return new WaitForSeconds(goodTime);
            nowCallEvaluationStep = EvaluationStep.Normal;
            nowFamilyScript.ChangeCustomerFace(1);
            tableMark.GetComponent<Image>().color = new Color32(0, 150, 50, 255);
            yield return new WaitForSeconds(badTime);
        }
        nowCallEvaluationStep = EvaluationStep.Bad;
        nowFamilyScript.ChangeCustomerFace(2);
        GameVariable.AngerChangeCallCount(true);
        tableMark.GetComponent<Image>().color = new Color32(170, 0, 0, 255);
        angerEffect?.SetActive(true);
        while (true)
        {
            onAngerCallAdd.Invoke();
            yield return new WaitForSeconds(badInterval);
        }
    }

    IEnumerator OrderEvaluation(bool firstAnger, float badInterval, float goodTime = 0, float badTime = 0)
    {
        if (!firstAnger)
        {
            nowOrderEvaluationStep = EvaluationStep.Good;
            nowFamilyScript.ChangeCustomerFace(0);
            yield return new WaitForSeconds(goodTime);
            nowOrderEvaluationStep = EvaluationStep.Normal;
            nowFamilyScript.ChangeCustomerFace(1);
            tableMark.GetComponent<Image>().color = new Color32(0, 150, 50, 255);
            yield return new WaitForSeconds(badTime);
        }
        nowOrderEvaluationStep = EvaluationStep.Bad;
        nowFamilyScript.ChangeCustomerFace(2);
        GameVariable.AngerChangeOrderCount(true);
        tableMark.GetComponent<Image>().color = new Color32(170, 0, 0, 255);
        angerEffect?.SetActive(true);
        while (true)
        {
            onAngerOrderAdd.Invoke();
            yield return new WaitForSeconds(badInterval);
        }
    }

    /*
    IEnumerator GoodCallDelay(float time)
    {
        yield return new WaitForSeconds(time);
        tableMark.GetComponent<Image>().color = new Color32(0, 150, 50, 255);
        goodCallFlag = true;
    }

    IEnumerator GoodOrderDelay(float time)
    {
        yield return new WaitForSeconds(time);
        tableMark.GetComponent<Image>().color = new Color32(0, 150, 50, 255);
        goodOrderFlag = true;
    }

    IEnumerator AngerCallDelay(float firstTime, float interval)
    {
        yield return new WaitForSeconds(firstTime);
        angerCallFlag = true;
        GameVariable.AngerChangeCallCount(true);
        tableMark.GetComponent<Image>().color = new Color32(170, 0, 0, 255);
        while (true)
        {
            onAngerCallAdd.Invoke();
            yield return new WaitForSeconds(interval);
        }
        //angerCallFlag = true;
        
    }

    IEnumerator AngerOrderDelay(float firstTime, float interval)
    {
        yield return new WaitForSeconds(firstTime);
        angerOrderFlag = true;
        GameVariable.AngerChangeOrderCount(true);
        tableMark.GetComponent<Image>().color = new Color32(170, 0, 0, 255);
        while (true)
        {
            onAngerOrderAdd.Invoke();
            yield return new WaitForSeconds(interval);
        }
        //angerOrderFlag = true;
        
    }
    */
}
