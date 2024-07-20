using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "Customer", menuName = "CreateCustomer")]

public class Customer : ScriptableObject //ゲームオブジェクトに取り付ける必要がないスクリプト
{
    //入力
    [SerializeField][Header("3Dモデル(GameObject)")]
    private GameObject customerModels;//モデル
    [SerializeField][Header("顔テクスチャー(Texture2D)")]
    private List<Texture2D> customerFace;//モデル


    //情報のセット
    public GameObject GetCustomerModels()
    {
        return customerModels;
    }
    public List<Texture2D> GetCustomerFace()
    {
        return customerFace;
        
    }
}
