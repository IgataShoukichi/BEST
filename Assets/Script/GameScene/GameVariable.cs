using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameVariable
{
    ////////////////////////////////////////////
    //設定変更

    public static int maxRegisterWaitingCustomers = 3;//最大レジ待ちができるお客さんの数
    public static int set3DUILayer = 10;//3DUIのレイヤー

    public static readonly string[] skinName = new string[]
    {
        "ノーマル1",
        "ノーマル2",
        "ノーマル3",
        "ノーマル4",
        "ノーマル5",
        "ノーマル6",
        "ノーマル7",
        "ノーマル8",
        "ノーマル9",
        "ノーマル10",
        "店長",
        "バイト1",
        "バイト2",
        "バイト3",
        "バイト4",
        "バイト5",
        "返却マン",
        "カオス(グリーン)",
        "カオス(ブルー)",
        "カオス(レッド)"
    };

    ////////////////////////////////////////////

    //Network管理関係
    public static int masterClientActorNumber = -1;//マスタークライアントの番号
    public static bool isMasterClient = false;//自分がマスタークライアントか
    public static bool nowApplicationFocus = false;//画面を選択しているか(リセットなし)

    //データベース
    public static FoodDataBase foodDataBase = null;
    public static CustomerGroupDataBase customerDataBase = null;
    
    //オブジェクト関係
    public static Dictionary<int,int> allPlayerSkins = new Dictionary<int,int>();//全プレイヤーのSkin
    public static List<Transform> allPlayerTransform = new List<Transform>();//全プレイヤーのトランスフォーム 
    public static List<GameObject> foodObjectList = new List<GameObject>();//食べ物リスト
    public static List<GameObject> foodStandList = new List<GameObject>();//食べ物出現スタンドリスト
    public static List<GameObject> tableList = new List<GameObject>();//テーブルリスト
    public static CashRegister cashRegister = null;//レジ
    public static List<GameObject> cleaningToolList = new List<GameObject>();//掃除用具リスト
    public static List<GameObject> waterSpotList = new List<GameObject>();//ウォーターサーバーリスト

    public static CustomerMovePoint customerMovePoint = null;//お客さんの移動ポイント

    //ゲーム内関係
    public static bool gameStarted = false;//ゲームを起動したか
    public static List<GameObject> orderList = new List<GameObject>();//注文リスト
    public static List<CustomerGroup> customerGroupList = new List<CustomerGroup>();//ゲームに登場する団体リスト
    public static List<int> customerShuffleList = new List<int>();//シャッフルされた実際に使う団体リスト番号
    public static List<int> nowStandbyCustomerList = new List<int>();//現在の待っている団体リスト番号
    public static List<Family> nowCustomerScriptList = new List<Family>();//現在出ているお客さんのスクリプト
    public static int foodCount = 0;//出した料理の数
    public static int customerCount = 0;//来た団体様の数
    //public static int nowRegisterWaitingCustomers = 0;//現在レジ待ちをしているお客さんの数
    public static List<Family> nowRegisterReadyCustomers = new List<Family>();//レジに行こうとしているお客さんの数
    public static List<Family> nowRegisterWaitingCustomers = new List<Family>();//現在レジ待ちをしているお客さんのスクリプト
    //public static bool nowRegisterMoveCustomer = false;//現在お客さんがレジに向けて移動しているか

    //ゲーム内(怒り関係)
    public static UnityEvent onAngerChange = new UnityEvent();
    public static int angerGuideCount = 0;
    public static int angerCallCount = 0;
    public static int angerOrderCount = 0;
    public static int angerCashCount = 0;

    //ゲーム内カウント
    public static int guideCustomerResult = 0;//案内した数
    public static int orderResult = 0;//注文を聞いた数
    public static int carryfoodResult = 0;//料理を運んだ数
    public static int cashResult = 0;//会計した数
    public static int cleanupTableResult = 0;//片づけた数
    public static int pushResult = 0;//押した数


    //最終結果
    public static int finalEvaluation = 0;//最終的な評価


    ////////////////////////////////////////////
    public static void AngerChangeGuideCount(bool plus)
    {
        angerGuideCount += plus ? 1 : -1;
        onAngerChange.Invoke();
    }
    public static void AngerChangeCallCount(bool plus)
    {
        angerCallCount += plus ? 1 : -1;
        onAngerChange.Invoke();
    }
    public static void AngerChangeOrderCount(bool plus)
    {
        angerOrderCount += plus ? 1 : -1;
        onAngerChange.Invoke();
    }
    public static void AngerChangeCashCount(bool plus)
    {
        angerCashCount += plus ? 1 : -1;
        onAngerChange.Invoke();
    }


    ////////////////////////////////////////////

    public static void Reset()
    {
        masterClientActorNumber = -1;
        isMasterClient = false;

        foodDataBase = null;
        customerDataBase = null;

        allPlayerSkins.Clear();
        allPlayerTransform.Clear();
        foodObjectList.Clear();
        foodStandList.Clear();
        tableList.Clear();
        cashRegister = null;
        cleaningToolList.Clear();
        waterSpotList.Clear();
        customerMovePoint = null;

        orderList.Clear();
        customerGroupList.Clear();
        customerShuffleList.Clear();
        nowStandbyCustomerList.Clear();
        nowCustomerScriptList.Clear();
        foodCount = 0;
        customerCount = 0;
        nowRegisterReadyCustomers.Clear();
        nowRegisterWaitingCustomers.Clear();

        onAngerChange.RemoveAllListeners();
        angerGuideCount = 0;
        angerCallCount = 0;
        angerOrderCount = 0;
        angerCashCount = 0;

        guideCustomerResult = 0;
        orderResult = 0;
        carryfoodResult = 0;
        cashResult = 0;
        cleanupTableResult = 0;
        pushResult = 0;

        finalEvaluation = 0;
    }
}
