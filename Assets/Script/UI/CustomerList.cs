using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class CustomerList : MonoBehaviour
{
    [System.NonSerialized] public UnityEvent onCustomerListOpen = new UnityEvent();
    [System.NonSerialized] public UnityEvent onCustomerListClose = new UnityEvent();
    [System.NonSerialized] public UnityEvent onGoodAdd = new UnityEvent();
    [System.NonSerialized] public UnityEvent onNormalAdd = new UnityEvent();
    [System.NonSerialized] public UnityEvent onAngerAdd = new UnityEvent();
    //[System.NonSerialized] public UnityEvent onAngerStop = new UnityEvent();

    [SerializeField] GameDirector gameDirector;

    [SerializeField][Header("お客さんパネルプレハブ")] GameObject customerPanelPrefab;
    [SerializeField][Header("バックポインター")] GameObject backPointer;
    [SerializeField][Header("お客さんスクロールビュー")] ScrollRect scrollRect;
    [SerializeField][Header("プレハブが入るオブジェクト")] GameObject customerContent;
    [SerializeField][Header("何もない時用テキスト")] Text emptyText;

    [SerializeField][Header("UI")] GameObject customerMark;
    [SerializeField] GameObject angerEffect;
    [SerializeField] Image iconImage;
    [SerializeField] Text customerNumberText;
    [SerializeField] GameObject okButton;

    [SerializeField][Header("")] GameObject backPanel;
    [SerializeField] GameObject frontPanel;
    [SerializeField] GameObject selectPanel;
    [SerializeField] GameObject checkPanel;
    [SerializeField] GameObject dontTapPanel;//ボタン操作出来なくするため

    [System.NonSerialized] public bool isOpenFlag = false;//現在開いているか

    [SerializeField] Text customerText;
    int nowWatingNumber = 1;

    float moveSpeed = 0.2f;

    //評価ゲージ関係
    float goodLimitTime = 0;
    float angerFirstTime = 0f;
    float angerInterval = 0;

    //評価
    enum EvaluationStep
    {
        Null,
        Good,
        Normal,
        Bad
    }
    Dictionary<int, EvaluationStep> nowGuideEvaluationSteps = new Dictionary<int, EvaluationStep>();
    Dictionary<int, Coroutine> GuideEvaluationCoroutines = new Dictionary<int, Coroutine>();

    //old
    //Dictionary<int,bool> goodFlags = new Dictionary<int, bool>();//良い判定を題していたか
    //Dictionary<int,bool> angerFlags = new Dictionary<int, bool>();//良い判定を題していたか

    //Dictionary<int,Coroutine> goodCoroutines = new Dictionary<int,Coroutine>();//コルーチン
    //Dictionary<int, Coroutine> angerCoroutines = new Dictionary<int, Coroutine>();//コルーチン管理

    //リスト部分
    int nowSelectPanelNumber = 0;//お客さんのパネル番号
    int nowSelectCustomerNumber = 0;//お客さんの管理番号

    CustomerGroup tempCustomer = null;

    void Start()
    {
        foreach(Transform child in customerContent.transform)
        {
            Destroy(child.gameObject);
        }
        DisplayReset();
        customerMark.SetActive(false);
        angerEffect?.SetActive(false);
    }

    public void EvaluationSetting(float gootLimitTime, float angerLimitTime,float angerInterval)
    {
        this.goodLimitTime = gootLimitTime;
        angerFirstTime = angerLimitTime;
        this.angerInterval = angerInterval;
    }

    void DisplayReset()
    {
        backPanel.GetComponent<CanvasGroup>().alpha = 0;
        backPanel.SetActive(false);
        selectPanel.GetComponent<CanvasGroup>().alpha = 1;
        selectPanel.SetActive(true);
        checkPanel.GetComponent<CanvasGroup>().alpha = 0;
        checkPanel.SetActive(false);
        dontTapPanel.SetActive(true);
        frontPanel.GetComponent<CanvasGroup>().alpha = 0;
        frontPanel.SetActive(false);
    }

    void CheckCustomerMark()
    {
        if(nowGuideEvaluationSteps.Count > 0)
        {
            if (!customerMark.activeSelf)
            {
                customerMark.SetActive(true);
            }
            customerNumberText.text = nowGuideEvaluationSteps.Count.ToString();
            if (nowGuideEvaluationSteps.ContainsValue(EvaluationStep.Bad))//マイナス
            {
                customerMark.GetComponent<Image>().color = new Color32(170, 0, 0, 255);
                angerEffect?.SetActive(true);
            }
            else if (nowGuideEvaluationSteps.ContainsValue(EvaluationStep.Normal))//ノーマル
            {
                customerMark.GetComponent<Image>().color = new Color32(0, 150, 50, 255);
                angerEffect?.SetActive(false);
            }
            else if (nowGuideEvaluationSteps.ContainsValue(EvaluationStep.Good))//プラス
            {
                customerMark.GetComponent<Image>().color = new Color32(0, 80, 150, 255);
                angerEffect?.SetActive(false);
            }
        }
        else
        {
            if (customerMark.activeSelf)
            {
                customerMark.SetActive(false);
                angerEffect?.SetActive(false);
            }
        }
        /*
        if (goodFlags.Count > 0)
        {
            if (!customerMark.activeSelf)
            {
                customerMark.SetActive(true);
            }
            customerNumberText.text = goodFlags.Count.ToString();
            if (!goodFlags.ContainsValue(true))//プラス
            {
                customerMark.GetComponent<Image>().color = new Color32(0, 80, 150, 255);
            }
            else if (!angerFlags.ContainsValue(true))//ノーマル
            {
                customerMark.GetComponent<Image>().color = new Color32(0, 150, 50, 255);
            }
            else//マイナス
            {
                customerMark.GetComponent<Image>().color = new Color32(170, 0, 0, 255);
            }
        }
        else
        {
            if (customerMark.activeSelf)
            {
                customerMark.SetActive(false);
            }
        }
        */
    }

    public void CustomerCreate(CustomerGroup customerGroup)
    {
        int number = nowWatingNumber;
        GameObject newCustomer = Instantiate(customerPanelPrefab);
        newCustomer.name = number.ToString();
        newCustomer.transform.SetParent(customerContent.transform);
        newCustomer.transform.localScale = customerPanelPrefab.transform.localScale;
        newCustomer.GetComponent<RectTransform>().pivot = Vector2.one * 0.5f;//中心を設定
        newCustomer.GetComponent<CustomerPrefab>().CustomerSetting(this.GetComponent<CustomerList>(), number, customerGroup);

        nowGuideEvaluationSteps.Add(number, EvaluationStep.Null);
        GuideEvaluationCoroutines.Add(number, StartCoroutine(GuideEvaluation(number,goodLimitTime,angerFirstTime,angerInterval)));
        //goodFlags.Add(number, false);
        //angerFlags.Add(number, false);
        //goodCoroutines.Add(number, StartCoroutine(GoodDelay(goodLimitTime, number)));
        //怒りコルーチン
        //angerCoroutines.Add(number, StartCoroutine(AngerDelay(angerFirstTime, angerInterval, number)));

        nowWatingNumber++;
        SoundList.Instance.SoundEffectPlay(6, 0.2f);
        CheckCustomerMark();
        MoveBackPointer(nowSelectPanelNumber);
        //MoveBackPointer(nowSelectNumber);
        //emptyText.text = "";
    }

    public void CustomerUpdate(int tapCustomerNumber)
    {
        foreach(Transform temp in customerContent.transform)
        {
            if(temp.gameObject.name == tapCustomerNumber.ToString())
            {
                Destroy(temp.gameObject);
            }
        }

        if (nowGuideEvaluationSteps.ContainsKey(tapCustomerNumber) && GuideEvaluationCoroutines.ContainsKey(tapCustomerNumber))
        {
            StopCoroutine(GuideEvaluationCoroutines[tapCustomerNumber]);
            GuideEvaluationCoroutines.Remove(tapCustomerNumber);
            switch (nowGuideEvaluationSteps[tapCustomerNumber])
            {
                case EvaluationStep.Good:
                    onGoodAdd.Invoke();
                    break;
                case EvaluationStep.Normal:
                    onNormalAdd.Invoke();
                    break;
                case EvaluationStep.Bad:
                    GameVariable.AngerChangeGuideCount(false);
                    break;
            }
            nowGuideEvaluationSteps.Remove(tapCustomerNumber);
        }
        CheckCustomerMark();
        /*
        if (goodFlags.ContainsKey(tapCustomerNumber) && angerFlags.ContainsKey(tapCustomerNumber))
        {

            if(goodFlags[tapCustomerNumber] && !angerFlags[tapCustomerNumber])//Normal判定
            {
                onNormalAdd.Invoke();
            }

            if (goodCoroutines.ContainsKey(tapCustomerNumber))
            {
                if (!goodFlags[tapCustomerNumber])//コルーチン終了前なら得点を入れる
                {
                    StopCoroutine(goodCoroutines[tapCustomerNumber]);
                    onGoodAdd.Invoke();
                }
                goodCoroutines.Remove(tapCustomerNumber);
                goodFlags.Remove(tapCustomerNumber);
            }

            //怒り発動前ならコルーチンを停止
            if (angerCoroutines.ContainsKey(tapCustomerNumber))
            {
                if (angerFlags[tapCustomerNumber])
                {
                    GameVariable.AngerChangeGuideCount(false);

                }
                StopCoroutine(angerCoroutines[tapCustomerNumber]);
                angerCoroutines.Remove(tapCustomerNumber);
                angerFlags.Remove(tapCustomerNumber);
            }
        }

        if (customerContent.transform.childCount <= 0)
        {
            //emptyText.text = "何もないよ";
        }
        CheckCustomerMark();
        */
    }

    public void ControllerGuideOK()//コントローラー用
    {
        if(customerContent.transform.childCount > 0)
        {
            customerContent.transform.GetChild(nowSelectPanelNumber)
                .GetComponent<CustomerPrefab>().SelectPanel(out nowSelectCustomerNumber, out tempCustomer);
            OKButton();

            //customerContent.transform.GetChild(nowSelectNumber).GetComponent<CustomerPrefab>().ClickPanel();
        }
        
    }

    public void TapGuideOK(int number,CustomerGroup customerGroup,GameObject selectObject)//タッチ用
    {
        //nowSelectPanelNumber = number;
        nowSelectCustomerNumber = number;
        tempCustomer = customerGroup;

        for (int i = 0; i < customerContent.transform.childCount; i++)
        {
            if(customerContent.transform.GetChild(i).gameObject == selectObject)
            {
                nowSelectPanelNumber = i;
                MoveBackPointer(nowSelectPanelNumber);
                break;
            }
        }

        
        //Close();
        //gameDirector.GuideCustomer(nowSelectNumber, tempCustomer);
        /*
        dontTapPanel.SetActive(true);
        selectPanel.GetComponent<CanvasGroup>().DOFade(0, moveSpeed).OnComplete(() => selectPanel.SetActive(false));
        checkPanel.SetActive(true);
        checkPanel.GetComponent<CanvasGroup>().DOFade(1, moveSpeed).OnComplete(() => dontTapPanel.SetActive(false));
        tempNumber = number;
        tempCustomer = customerGroup;
        customerText.text = $"{customerGroup.GetCustomerName()} 様({customerGroup.GetCustomerDetail().Count}人)\nを案内しますか?";
        */
    }

    IEnumerator GuideEvaluation(int number,float goodTime,float badTime,float badInterval)
    {
        nowGuideEvaluationSteps[number] = EvaluationStep.Good;
        yield return new WaitForSeconds(goodTime);
        nowGuideEvaluationSteps[number] = EvaluationStep.Normal;
        yield return new WaitForSeconds(badTime);
        nowGuideEvaluationSteps[number] = EvaluationStep.Bad;
        GameVariable.AngerChangeGuideCount(true);
        CheckCustomerMark();
        while (true)
        {
            onAngerAdd.Invoke();
            yield return new WaitForSeconds(badInterval);
        }
    }

    /*
    IEnumerator GoodDelay(float time,int number)
    {
        yield return new WaitForSeconds(time);
        goodFlags[number] = true;
        CheckCustomerMark();
    }

    IEnumerator AngerDelay(float firstTime,float interval,int number)
    {
        yield return new WaitForSeconds(firstTime);
        angerFlags[number] = true;
        GameVariable.AngerChangeGuideCount(true);
        CheckCustomerMark();
        while (true)
        {
            onAngerAdd.Invoke();
            yield return new WaitForSeconds(interval);
        }
        //angerFlags[number] = true;
        
    }
    */


    public void Open()
    {
        nowSelectPanelNumber = 0;
        nowSelectCustomerNumber = 0;
        Debug.Log("CustomerListOpen");
        dontTapPanel.SetActive(true);
        backPanel.SetActive(true);
        backPanel.GetComponent<CanvasGroup>().DOFade(1, moveSpeed);
        frontPanel.SetActive(true);
        frontPanel.GetComponent<CanvasGroup>().DOFade(1, moveSpeed).OnComplete(() => dontTapPanel.SetActive(false));
        isOpenFlag = true;
        scrollRect.verticalNormalizedPosition = 1f;
        MoveBackPointer(nowSelectPanelNumber);
        onCustomerListOpen.Invoke();
    }

    public void Close()
    {
        dontTapPanel.SetActive(true);

        backPanel.GetComponent<CanvasGroup>().DOFade(0, moveSpeed).OnComplete(() => backPanel.SetActive(false));
        frontPanel.GetComponent<CanvasGroup>().DOFade(0, moveSpeed).OnComplete(() => DisplayReset());
        isOpenFlag = false;
        onCustomerListClose.Invoke();
    }


    public void OKButton()
    {
        Close();
        gameDirector.GuideCustomer(nowSelectCustomerNumber, tempCustomer);
    }
    public void BackButton()
    {
        dontTapPanel.SetActive(true);
        checkPanel.GetComponent<CanvasGroup>().DOFade(0, moveSpeed).OnComplete(() => checkPanel.SetActive(false));
        selectPanel.SetActive(true);
        selectPanel.GetComponent<CanvasGroup>().DOFade(1, moveSpeed).OnComplete(() => dontTapPanel.SetActive(false));
    }

    //コントローラー系
    public void UpDown(bool up)
    {
        int totalContentCount = customerContent.transform.childCount;
        if (totalContentCount > 0)
        {
            if (up)//上
            {
                if (nowSelectPanelNumber > 0)
                {
                    nowSelectPanelNumber--;
                }
            }
            else//下
            {
                if (nowSelectPanelNumber < totalContentCount - 1)
                {
                    nowSelectPanelNumber++;
                }
            }

            float contentPosition = customerContent.GetComponent<RectTransform>().localPosition.y;
            float contentSize = customerContent.transform.parent.parent.GetComponent<RectTransform>().rect.height;
            Transform child = customerContent.transform.GetChild(nowSelectPanelNumber);
            float childPosition = -child.GetComponent<RectTransform>().localPosition.y;
            float childHalfSize = child.GetComponent<RectTransform>().rect.height / 2;
            //下がる
            if (childPosition + childHalfSize > contentPosition + contentSize)
            {
                float temp = (childPosition + childHalfSize) - (contentPosition + contentSize);
                customerContent.GetComponent<RectTransform>().DOLocalMoveY(contentPosition + temp, moveSpeed);
            }
            //上がる
            else if (childPosition - childHalfSize < contentPosition)
            {
                float temp = (contentPosition) - (childPosition - childHalfSize);
                customerContent.GetComponent<RectTransform>().DOLocalMoveY(contentPosition - temp, moveSpeed);
            }
            else
            {
                //MoveBackPointer(nowSelectPanelNumber);
            }
            MoveBackPointer(nowSelectPanelNumber);
        }
    }

    void MoveBackPointer(int selectNumber)
    {
        for (int i = 0; i < customerContent.transform.childCount; i++)
        {
            customerContent.transform.GetChild(i).GetComponent<Image>().DOKill();
            if (i == selectNumber)
            {
                if(customerContent.transform.GetChild(i).GetComponent<Image>().color.a < 0.3f)
                {
                    customerContent.transform.GetChild(i).GetComponent<Image>().DOFade(0.4f, moveSpeed / 2);
                }
            }
            else
            {
                customerContent.transform.GetChild(i).GetComponent<Image>().DOFade(0f, moveSpeed / 2);
            }
        }

        if (customerContent.transform.childCount > 0)
        {
            //Vector3 contentPosition = customerContent.transform.GetChild(selectNumber).GetComponent<RectTransform>().position;
            //Vector3 basePosition = backPointer.GetComponent<RectTransform>().position;

            //if (backPointer.activeSelf)
            //{
            //
            //}
            //else
            //{
                okButton.GetComponent<Button>().enabled = true;
                okButton.GetComponent<Image>().DOFade(1f, 0f);
            //}
        }
        else
        {
            //backPointer.SetActive(false);
            okButton.GetComponent<Button>().enabled = false;
            okButton.GetComponent<Image>().DOFade(0.7f, 0f);
        }

        /*
        if (customerContent.transform.childCount > 0)
        {
            Vector3 contentPosition = customerContent.transform.GetChild(selectNumber).GetComponent<RectTransform>().position;
            Vector3 basePosition = backPointer.GetComponent<RectTransform>().position;
            
            if (backPointer.activeSelf)
            {
                backPointer.GetComponent<RectTransform>().DOMoveY(contentPosition.y, 0.2f).SetEase(Ease.OutCubic);
                //backPointer.GetComponent<RectTransform>().position =
                        //new Vector3(basePosition.x, contentPosition.y, basePosition.z);
                //backPointer.GetComponent<RectTransform>().position =
                    //customerContent.transform.GetChild(selectNumber).GetComponent<Transform>().position;
            }
            else
            {
                backPointer.SetActive(true);
                //backPointer.GetComponent<RectTransform>().sizeDelta =
                //customerContent.transform.GetChild(selectNumber).GetComponent<RectTransform>().sizeDelta * 1.01f;
                backPointer.GetComponent<RectTransform>().position = 
                    new Vector3(basePosition.x,contentPosition.y, basePosition.z);
                //backPointer.GetComponent<RectTransform>().position =
                    //customerContent.transform.GetChild(selectNumber).GetComponent<Transform>().position;
                okButton.GetComponent<Button>().enabled = true;
                okButton.GetComponent<Image>().DOFade(1f, 0f);
            }
        }
        else
        {
            backPointer.SetActive(false);
            okButton.GetComponent<Button>().enabled = false;
            okButton.GetComponent<Image>().DOFade(0.7f, 0f);
        }
        */
    }
}
