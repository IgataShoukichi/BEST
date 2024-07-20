using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "Food", menuName = "CreateFood")]

public class Food : ScriptableObject //ゲームオブジェクトに取り付ける必要がないスクリプト
{
    //入力
    [SerializeField][Header("管理番号(int)")]
    private int foodNumber;//アイテムの管理番号
    [SerializeField][Header("名前(string)")]
    private string foodName;//アイテムの名前
    [SerializeField][Header("3Dモデル(GameObject)")]
    private GameObject foodModel;//アイテムのオブジェクト
    [SerializeField][Header("UI用のモデル画像(Sprite)")]
    private Sprite foodSprite;//アイテムの画像
    [SerializeField][Header("色(Color32)")]
    private Color32 foodColor;//アイテムの色
    [SerializeField][Header("調理時間(int:秒)")]
    private int foodTime;//アイテムの調理時間
    [SerializeField][Header("器が割れるか(bool)")]
    private bool foodBreak;//アイテムが割れるか
    [SerializeField][Header("大きい料理か(bool)")]
    private bool foodBig;//大きい料理か



    //情報のセット
    public int GetFoodNumber()
    {
        return foodNumber;
    }
    public string GetFoodName()
    {
        return foodName;
    }
    public GameObject GetFoodModel()
    {
        return foodModel;
    }
    public Sprite GetFoodSprite()
    {
        return foodSprite;
    }
    public Color32 GetFoodColor()
    {
        return foodColor;
    }
    public int GetFoodTime()
    {
        return foodTime;
    }
    public bool GetFoodBrake()
    {
        return foodBreak;
    }
    public bool GetFoodBig()
    {
        return foodBig;
    }
}
