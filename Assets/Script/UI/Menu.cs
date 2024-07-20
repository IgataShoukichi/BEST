using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class Menu : MonoBehaviour
{
    [System.NonSerialized] public UnityEvent onMenuOpen = new UnityEvent();
    [System.NonSerialized] public UnityEvent onMenuClose = new UnityEvent();

    [SerializeField] GameObject backPanel;
    [SerializeField] GameObject frontPanel;

    [SerializeField][Header("(コール)\nプレハブ")] GameObject callPanelPrefab;
    [SerializeField][Header("プレハブが入るオブジェクト")] GameObject callContent;
    [SerializeField][Header("何もない時用テキスト")] Text callEmptyText;
    [SerializeField][Header("(注文)\nプレハブ")] GameObject orderPanelPrefab;
    [SerializeField][Header("プレハブが入るオブジェクト")] GameObject orderContent;
    [SerializeField][Header("何もない時用テキスト")] Text orderEmptyText;
    
    [SerializeField][Header("開け閉めボタンテキスト")] Text MenuButtonText;

    bool menuOpen = false;
    float moveSpeed = 0.2f;
    int nowCallCount = 0;

    void Start()
    {
        backPanel.GetComponent<CanvasGroup>().alpha = 0;
        backPanel.SetActive(false);
        frontPanel.GetComponent<CanvasGroup>().alpha = 0;
        frontPanel.SetActive(false);
        menuOpen = backPanel.activeSelf;
        foreach(Transform child in callContent.transform)
        {
            Destroy(child.gameObject);
        }
        foreach(Transform child in orderContent.transform)
        {
            Destroy(child.gameObject);
        }
    }


    void Update()
    {
        
    }

    public void MenuButton()
    {
        if (!menuOpen)
        {
            Debug.Log("MenuOpen");
            backPanel.SetActive(true);
            backPanel.GetComponent<CanvasGroup>().DOFade(1, moveSpeed);
            frontPanel.SetActive(true);
            frontPanel.GetComponent<CanvasGroup>().DOFade(1, moveSpeed).SetDelay(moveSpeed/2);
            MenuButtonText.text = "閉じる";
            onMenuOpen.Invoke();
            menuOpen = true;
        }
        else
        {
            Debug.Log("MenuClose");
            frontPanel.GetComponent<CanvasGroup>().DOFade(0, moveSpeed).OnComplete(() => frontPanel.SetActive(false));
            backPanel.GetComponent<CanvasGroup>().DOFade(0, moveSpeed).SetDelay(moveSpeed/2).OnComplete(() => backPanel.SetActive(false));
            MenuButtonText.text = "Menu";
            onMenuClose.Invoke();
            menuOpen = false;
        }
    }

    public void CallCreate(int tableNumber)
    {
        GameObject newCall = Instantiate(callPanelPrefab);
        newCall.name = tableNumber.ToString();
        newCall.transform.GetChild(0).GetComponent<Text>().text = tableNumber.ToString();
        newCall.transform.SetParent(callContent.transform);
        newCall.transform.localScale = callPanelPrefab.transform.localScale;
        nowCallCount++;
        callEmptyText.text = "";
    }
    public void UpdateCalls(int tableNumber)
    {
        foreach(Transform temp in callContent.transform)
        {
            if(temp.name == tableNumber.ToString())
            {
                Destroy(temp.gameObject);
                nowCallCount--;
                if (nowCallCount <= 0)
                {
                    callEmptyText.text = "呼ばれてないよ";
                }
            }
        }
    }


    public void OrderCreate(int tableNumber, List<Food> orders)
    {
        GameObject newOrder = Instantiate(orderPanelPrefab);
        newOrder.name = tableNumber.ToString();
        newOrder.transform.SetParent(orderContent.transform);
        newOrder.transform.localScale = orderPanelPrefab.transform.localScale;
        newOrder.GetComponent<OrderPrefab>().OrderSetting(tableNumber, orders);
        GameVariable.orderList.Add(newOrder);
        UpdateCalls(tableNumber);
        orderEmptyText.text = "";
    }

    public void UpdateOrders(int tableNumber/*,int orderListNumber*/)
    {
        GameObject temp = GameVariable.orderList.Find(n => n.name == tableNumber.ToString());
        //temp.GetComponent<OrderPrefab>().UpdateOrder(orderListNumber);
        //if(temp.GetComponent<OrderPrefab>().orderCount <= 0)
        //{
            Destroy(temp);
            GameVariable.orderList.Remove(temp);
        //}
        if(GameVariable.orderList.Count <= 0)
        {
            orderEmptyText.text = "何もないよ";
        }
    }
}
