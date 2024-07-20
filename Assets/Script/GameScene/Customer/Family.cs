//お客様をまとめてるオブジェクトにつけるスクリプト
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using DG.Tweening;
using Unity.VisualScripting;

public class Family : MonoBehaviour
{
    [System.NonSerialized] public UnityEvent onGoodGuide2Add = new UnityEvent();
    [System.NonSerialized] public UnityEvent onAngerGuide2Add = new UnityEvent();

    [System.NonSerialized] public PlayerRPC playerRPC;
    [System.NonSerialized] public int registerWaitNumber = -1;
    [System.NonSerialized] public Transform customerParentPos;
    [System.NonSerialized] public List<Transform> standingPosition;//テーブル側から代入
    [System.NonSerialized] public Table table;

    [System.NonSerialized] public GameObject parentObject;

    [System.NonSerialized] public List<GameObject> childCustomer = new List<GameObject>();
    CustomerGroup nowCustomerGroup = null;
    Vector3[] spawnPositions = new Vector3[4] { 
        new Vector3(0, 0, 0),
        new Vector3(-1, 0, 0), 
        new Vector3(-2, 0, 0), 
        new Vector3(-3, 0, 0) 
    };

    float limitDistance = 3f;//テーブルからの距離

    //Coroutine goodGuide2Coroutine = null;
    //Coroutine angerGuide2Coroutine = null;
    //bool goodGuide2Flag = false;
    //float goodGuide2Time = 0f;
    //float angerGuide2FirstTime = 0f;
    //float angerGuide2Interval = 0f;

    private void Awake()
    {
        parentObject = this.gameObject;
    }
    void Start()
    {

    }

    void Update()
    {

    }

    public void EvaluationGuideSetting(float goodFirstTime, float firstTime, float interval)
    {
        //goodGuide2Time = goodFirstTime;
        //angerGuide2FirstTime = firstTime;
        //angerGuide2Interval = interval;
    }

    IEnumerator GoodGuide2Delay(float time)
    {
        yield return new WaitForSeconds(time);
        //goodGuide2Flag = true;
    }

    IEnumerator AngerGuide2Delay(float firstTime, float interval)
    {
        yield return new WaitForSeconds(firstTime);
        //while (true)
        //{
        //    onAngerGuide2Add.Invoke();
        //    yield return new WaitForSeconds(interval);
        //}
        //angerCallFlag = true;

    }

    public void CreateCustomer(CustomerGroup customerGroup,GameObject targetObject)
    {
        nowCustomerGroup = customerGroup;
        this.gameObject.name = customerGroup.GetCustomerNumber().ToString();
        foreach (Customer tempCustomer in customerGroup.GetCustomerDetail())
        {
            GameObject prefab = tempCustomer.GetCustomerModels();
            GameObject customerObject = Instantiate(prefab);
            customerObject.name = childCustomer.Count.ToString();
            customerObject.transform.SetParent(parentObject.transform);
            customerObject.transform.localPosition = spawnPositions[childCustomer.Count];
            NewCustomerController customerScript = customerObject.AddComponent<NewCustomerController>();
            customerScript.AnimationSetting();
            customerObject.tag = "Customer";
            customerObject.layer = 7; //案内しているプレイヤーと当たらないようにレイヤーを分ける
            if (childCustomer.Count <= 0)
            {
                customerScript.SetFollow(targetObject,true);
            }
            else
            {
                customerScript.SetFollow(childCustomer[childCustomer.Count -1], false);
            }
            childCustomer.Add(customerObject);
        }
        customerParentPos = childCustomer[0].transform;//先頭の人

        //goodGuide2Flag = false;
        //goodGuide2Coroutine = StartCoroutine(GoodGuide2Delay(goodGuide2Time));
        //angerGuide2Coroutine = StartCoroutine(AngerGuide2Delay(angerGuide2FirstTime, angerGuide2Interval));
    }

    public void ChangeCustomerFace(int mode)
    {
        int i = 0;
        foreach(GameObject tempObject in childCustomer)
        {
            if(tempObject != null)
            {
                Material tempMaterial = tempObject.transform.GetChild(1).GetComponent<Renderer>().material;
                Texture2D texture2D = nowCustomerGroup.GetCustomerDetail()[i].GetCustomerFace()[mode];
                tempMaterial.SetTexture("_MainTex", texture2D);
            }
            i++;
        }
    }

    public void SitdownCustomer(Table table,List<Transform> standingPosition)
    {
        foreach(GameObject customerObject in childCustomer)
        {
            customerObject.layer = 0;
            customerObject.GetComponent<NewCustomerController>().StopFollow();
            customerObject.GetComponent<NewCustomerController>().customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.sitCus);
        }
        lookMenuAnimation(true);
        this.table = table;
        this.standingPosition = new List<Transform>(standingPosition);

        //if (!goodGuide2Flag)
        //{
        //    StopCoroutine(goodGuide2Coroutine);
        //    onGoodGuide2Add.Invoke();
        //}
        //goodGuide2Coroutine = null;
        //goodGuide2Flag = false;
        //StopCoroutine(angerGuide2Coroutine);
        //angerGuide2Coroutine = null;
    }

    public void lookMenuAnimation(bool mode)
    {
        foreach (GameObject customerObject in childCustomer)
        {
            if (mode)
            {
                customerObject.GetComponent<NewCustomerController>().customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.look_menuCus);
            }
            else
            {
                customerObject.GetComponent<NewCustomerController>().customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.sitCus);
            }
        }
    }

    public void eatAnimation(bool mode)
    {
        foreach (GameObject customerObject in childCustomer)
        {
            if (mode)
            {
                customerObject.GetComponent<NewCustomerController>().customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.eat_foodCus);
            }
            else
            {
                customerObject.GetComponent<NewCustomerController>().customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.sitCus);
            }
        }
    }

    public void AddStandbyCustomers()
    {
        GameVariable.cashRegister.AddStandbyCustomers(this);
    }

    public IEnumerator StandupCustomer()
    {
        if (GameVariable.isMasterClient)
        {
            //お客さんを移動させる
            //NavMesh用のオブジェクトを作って消す
            GameObject tempNav = new GameObject("NavMeshAgent");
            tempNav.transform.position = standingPosition[0].position;
            tempNav.transform.localRotation = Quaternion.identity;
            
            NavMeshAgent navMeshAgent = tempNav.AddComponent<NavMeshAgent>();
            navMeshAgent.SetDestination(GameVariable.customerMovePoint.GetFirstPosition());
            yield return new WaitUntil(() => navMeshAgent.hasPath != false);
            navMeshAgent.isStopped = true;
            var temp = navMeshAgent.path.corners;
            Destroy(tempNav);
            //周りにプレイヤーがいないか確認
            int count = 0;
            while (count < GameVariable.allPlayerTransform.Count)
            {
                count = 0;
                yield return new WaitForSeconds(0.2f);
                foreach (Transform tempPlayer in GameVariable.allPlayerTransform)
                {
                    float distance = Vector3.Distance(this.table.gameObject.transform.position, tempPlayer.transform.position);
                    if (distance >= limitDistance)
                    {
                        count++;
                    }
                }
            }
            playerRPC.GoHome(int.Parse(this.gameObject.name), temp);
        }
    }

    public void GoHome(Vector3[] FirstRoute)
    {
        int customerTempCount = 0;
        foreach (GameObject customer in childCustomer)
        {
            customer.transform.SetParent(parentObject.transform);
            customer.transform.position = standingPosition[customerTempCount].position;
            customer.transform.localRotation = Quaternion.identity;
            customerTempCount++;
            customer.GetComponent<NewCustomerController>().customerAnimation.AnimationStop(CustomerAnimation.AnimationName.sitCus);
            customer.GetComponent<NewCustomerController>().customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.neutralCus);
        }
        registerWaitNumber = GameVariable.nowRegisterWaitingCustomers.Count;
        GameVariable.nowRegisterWaitingCustomers.Add(this.GetComponent<Family>());

        table.StandupCustomers();
        customerParentPos.GetComponent<NewCustomerController>().customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.walkCus);
        customerParentPos.DOPath(FirstRoute, customerParentPos.GetComponent<NewCustomerController>().speed,PathType.CatmullRom).SetSpeedBased().SetEase(Ease.Linear).SetLookAt(0.001f, Vector3.forward).OnComplete(() => RegisterAndExit());//一定速度
        int count = 0;
        foreach (GameObject customerObject in childCustomer)
        {
            if(customerObject != customerParentPos.gameObject)
            {
                customerObject.GetComponent<NewCustomerController>().SetFollow(childCustomer[count - 1], false);
            }
            count++;
        }
    }

    void RegisterAndExit()
    {
        foreach (GameObject customerObject in childCustomer)
        {
            customerObject.GetComponent<NewCustomerController>().StopFollow();
        }
        customerParentPos.DOKill();
        Vector3 temp = GameVariable.customerMovePoint.GetRegisterRoute()[registerWaitNumber];
        temp.y = customerParentPos.position.y;
        customerParentPos.LookAt(temp);
        customerParentPos.GetComponent<NewCustomerController>().customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.walkCus);
        customerParentPos.DOMove(GameVariable.customerMovePoint.GetRegisterRoute()[registerWaitNumber], customerParentPos.GetComponent<NewCustomerController>().speed).SetSpeedBased().SetEase(Ease.Linear).OnComplete(() => AddRegister(this));//一定速度
        int count = 0;
        foreach (GameObject customerObject in childCustomer)
        {
            if (customerObject != customerParentPos.gameObject && count == 1)
            {
                customerObject.transform.DOPath(GameVariable.customerMovePoint.GetExitRoute().ToArray(), customerParentPos.GetComponent<NewCustomerController>().speed, PathType.CatmullRom).SetSpeedBased().SetEase(Ease.Linear).SetLookAt(0.001f, Vector3.forward).OnComplete(() => EndExit());//一定速度
                customerObject.GetComponent<NewCustomerController>().customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.walkCus);
            }
            else if(customerObject != customerParentPos.gameObject && count != 1 )
            {
                customerObject.GetComponent<NewCustomerController>().SetFollow(childCustomer[count - 1], false);
                customerObject.GetComponent<NewCustomerController>().customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.walkCus);
            }
            
            count++;
        }
    }

    void AddRegister(Family family)
    {
        customerParentPos.GetComponent<NewCustomerController>().customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.neutralCus);
        Vector3 temp;
        if (registerWaitNumber == 0)
        {
            temp = GameVariable.cashRegister.transform.position;
        }
        else
        {
            temp = GameVariable.customerMovePoint.GetRegisterRoute()[registerWaitNumber - 1];
        }
        temp.y = customerParentPos.position.y;
        customerParentPos.LookAt(temp);
        GameVariable.cashRegister.AddCustomers(family);
    }

    public void EndRegister()
    {
        List<Vector3> registerExitRoute = new List<Vector3>(GameVariable.customerMovePoint.GetExitRoute());
        registerExitRoute.RemoveAt(0);
        GameVariable.nowCustomerScriptList.Remove(this.GetComponent<Family>());
        customerParentPos.GetComponent<NewCustomerController>().customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.walkCus);
        customerParentPos.DOPath(GameVariable.customerMovePoint.GetExitRoute().ToArray(), customerParentPos.GetComponent<NewCustomerController>().speed, PathType.CatmullRom).SetSpeedBased().SetEase(Ease.Linear).SetLookAt(0.001f, Vector3.forward).OnComplete(() => Destroy(parentObject));//一定速度
    }

    void EndExit()
    {
        foreach(GameObject customerObject in childCustomer)
        {
            if(customerObject != customerParentPos.gameObject)
            {
                customerObject.GetComponent<NewCustomerController>().StopFollow();
                Destroy(customerObject);
            }
        }
    }
}
