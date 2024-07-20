using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "CustomerGroup", menuName = "CreateCustomerGroup")]

public class CustomerGroup : ScriptableObject //ゲームオブジェクトに取り付ける必要がないスクリプト
{
    //入力
    [SerializeField][Header("団体管理番号(int)")]
    private int customerNumber;//団体様の管理番号
    [SerializeField][Header("団体の名前(string)")]
    private string customerName;//団体様の名前
    [SerializeField][Header("お客さん(Customer)")]
    private List<Customer> customerDetail;//お客さん単体の情報
    [SerializeField][Header("注文までの時間(float)")]
    private float customerOrderTime;//注文までの時間
    [SerializeField][Header("注文料理管理番号(int)")]
    private List<int> customerOrderFoodNumber;//アイテムの調理時間
    [SerializeField][Header("食べ終わるまでの時間(float)")]
    private float customerEatingEndTime;//アイテムが割れるか



    //情報のセット
    public int GetCustomerNumber()
    {
        return customerNumber;
    }
    public string GetCustomerName()
    {
        return customerName;
    }
    public List<Customer> GetCustomerDetail()
    {
        return customerDetail;
    }
    public float GetCustomerOrderTime()
    {
        return customerOrderTime;
    }
    public List<int> GetCustomerOrderFoodNumber()
    {
        return customerOrderFoodNumber;
    }
    public float GetCustomerEatingEndTime()
    {
        return customerEatingEndTime;
    }
}
