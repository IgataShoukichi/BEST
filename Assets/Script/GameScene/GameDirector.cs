using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameDirector : MonoBehaviourPunCallbacks
{
    /////////////////////////////////////////////////////
    //可変

    [SerializeField][Header("全Waveの合計時間")] float allWaveTime = 270f;//4分30秒
    [SerializeField][Header("値の設定\nWaveの最大数")] int maxWaveCount = 3;
    [SerializeField][Header("各Waveの名前")] string[] waveNames = new string[] { "モーニング", "ランチ", "ディナー" };
    [SerializeField][Header("最初のお客さんの生成間隔(固定)")] float customerSpawnTimeFirst = 2f;
    [SerializeField][Header("お客さんの生成間隔(最小)")] float customerSpawnTimeMin = 5f;
    [SerializeField][Header("お客さんの生成間隔(最大)")] float customerSpawnTimeMax = 12f;

    [SerializeField][Header("怒りゲージの最小")] int minEvaluationGauge = 0;
    [SerializeField][Header("怒りゲージの最大")] int maxEvaluationGauge = 100;
    [SerializeField][Header("怒りゲージが最大になってから終了までの猶予時間")] float maxEvaluationFinishTime = 10f;

    [SerializeField][Header("Playerが立つ位置")] GameObject playerRespawnPositions;
    List<Vector3> respawnPlayerPositions = new List<Vector3>();

    //UI
    float panelMoveTime = 0.3f;
    float panelTextMoveRange = 50f;

    /*
    //各プレイヤーが立つ位置
    Vector3[] respawnPlayerPositions = new Vector3[4] {
        new Vector3(-5,1.5f,6f),
        new Vector3(-3, 1.5f, 6f),
        new Vector3(-1, 1.5f, 6f),
        new Vector3(1, 1.5f, 6f)
    };
    */
    //評価系
    public enum EvaluationGaugeValueType//評価タイプ
    {
        Guide,//案内(呼ぶまで)
        Guide2,//案内(呼んでから席に座るまで)
        Call,//店員を呼ぶ
        Order,//料理が来ない
        CashRegister,//会計出来ない
        Dirty//床が汚い
    }


    //良い判定になる時間
    public readonly Dictionary<EvaluationGaugeValueType, int> goodGaugeTimes = new Dictionary<EvaluationGaugeValueType, int>() { 
        {EvaluationGaugeValueType.Guide, 10 },
        {EvaluationGaugeValueType.Guide2, 10 },
        {EvaluationGaugeValueType.Call, 10 },
        {EvaluationGaugeValueType.Order, 10 },
        {EvaluationGaugeValueType.CashRegister, 10 },
        {EvaluationGaugeValueType.Dirty, 10 }
    };

    //良い判定のポイント
    public readonly Dictionary<EvaluationGaugeValueType, int> goodGaugePoints = new Dictionary<EvaluationGaugeValueType, int>() {
        {EvaluationGaugeValueType.Guide, 5 },
        {EvaluationGaugeValueType.Guide2, 5 },
        {EvaluationGaugeValueType.Call, 5 },
        {EvaluationGaugeValueType.Order, 5 },
        {EvaluationGaugeValueType.CashRegister, 5 },
        {EvaluationGaugeValueType.Dirty, 5 }
    };

    //ノーマル判定のポイント
    public readonly Dictionary<EvaluationGaugeValueType, int> normalGaugePoints = new Dictionary<EvaluationGaugeValueType, int>(){
        {EvaluationGaugeValueType.Guide, 2 },
        {EvaluationGaugeValueType.Guide2, 2 },
        {EvaluationGaugeValueType.Call, 2 },
        {EvaluationGaugeValueType.Order, 2 },
        {EvaluationGaugeValueType.CashRegister, 2 },
        {EvaluationGaugeValueType.Dirty, 2 }
    };

    //怒りがたまり始めるまでの時間
    public readonly Dictionary<EvaluationGaugeValueType, int> angerGaugeTimes = new Dictionary<EvaluationGaugeValueType, int>() {
        {EvaluationGaugeValueType.Guide, 10 },
        {EvaluationGaugeValueType.Guide2, 10 },
        {EvaluationGaugeValueType.Call,10 },
        {EvaluationGaugeValueType.Order,10 },
        {EvaluationGaugeValueType.CashRegister,10 },
        {EvaluationGaugeValueType.Dirty,10 }
    };

    //怒り判定のポイントが何秒ごとに変わるか変わるか
    public readonly Dictionary<EvaluationGaugeValueType, int> angerGaugeIntervals = new Dictionary<EvaluationGaugeValueType, int>() {
        {EvaluationGaugeValueType.Guide, 4 },
        {EvaluationGaugeValueType.Guide2, 4 },
        {EvaluationGaugeValueType.Call, 4 },
        {EvaluationGaugeValueType.Order, 4 },
        {EvaluationGaugeValueType.CashRegister, 4 },
        {EvaluationGaugeValueType.Dirty, 4 }
    };

    //怒り判定のポイントが一回で何ポイント変わるか
    public readonly Dictionary<EvaluationGaugeValueType, int> angerGaugePoints = new Dictionary<EvaluationGaugeValueType, int>() {
        {EvaluationGaugeValueType.Guide, -1 },
        {EvaluationGaugeValueType.Guide2, -1 },
        {EvaluationGaugeValueType.Call,-1 },
        {EvaluationGaugeValueType.Order,-1 },
        {EvaluationGaugeValueType.CashRegister,-1 },
        {EvaluationGaugeValueType.Dirty,-1 }
    };

    /////////////////////////////////////////////////////

    [SerializeField][Header("スクリプト")] PlayerController playerController;//スクリプト
    [SerializeField] Menu menu;//スクリプト
    [SerializeField] FoodStandManager foodStandManager;//スクリプト
    [SerializeField] CustomerList customerList;//スクリプト
    [SerializeField] GameObject informationPanel;//インフォメーションパネル
    [SerializeField] DashBoard dashBoard;//ダッシュボード
    [SerializeField] EvaluationGauge evaluationGauge;//評価ゲージスクリプト
    [SerializeField] EvaluationAnger evaluationAnger;//怒り数パネル
    [SerializeField] HelpPanel helpPanel;//Helpパネル

    [SerializeField][Header("UI")] Text startEndText;
    [SerializeField] GameObject startEndPanel;
    [SerializeField] RectTransform evaluationPanel;//評価のパネル
    [SerializeField] ButtonManager buttonManager;

    [SerializeField] GameObject dontTapPanel;//画面を触れなくする

    [SerializeField][Header("オブジェクト")] GameObject foodStands;//フードスタンドが入っている親オブジェクト
    [SerializeField] GameObject tables;//テーブルが入っている親オブジェクト
    [SerializeField] GameObject cashRegister;//レジ
    [SerializeField] GameObject cleaningTools;//掃除用具が入っている親オブジェクト
    [SerializeField] GameObject waterSpots;//ウォーターサーバーが入っている親オブジェクト
    
    //prefab
    [SerializeField][Header("Prefab")] GameObject waterPrefab;//プレイヤーが持つ水のオブジェクトプレハブ
    [SerializeField] GameObject obonPrefab;//料理を乗せるお盆のオブジェクトプレハブ
    [SerializeField] GameObject wagonPrefab;//でかい料理を乗せるワゴンのオブジェクトプレハブ
    [SerializeField] GameObject cleanupObjectPrefab;//テーブルを掃除した後のオブジェクトプレハブ
    [SerializeField] GameObject familyPrefab;//お客さんの親オブジェクトのプレハブ
    //位置
    [SerializeField] Transform CustomerSpawnPosition;//お客さんがスポーンする地点

    [SerializeField] CustomerMovePoint customerMovePoint;//お客さん用のポイントまとめ

    [SerializeField][Header("データベース")] FoodDataBase foodDB;
    [SerializeField] CustomerGroupDataBase customerDB;

    [SerializeField][Header("プレハブ")] GameObject cleanupObject;


    /////////////////////////////////////////////////////

    PlayerRPC playerRPC;
    List<PlayerRPC> playerRPCs = new List<PlayerRPC>();
    
    
    //プレイヤーアタッチ用
    GameObject myPlayer;

    //現在のウェーブ
    int nowWaveCount = 0;

    //怒りゲージ関連
    int nowEvaluationGaugeValue = 50;//現在の怒りゲージ
    bool nowMinGauge = false;//現在ゲージが設定値より小さくなっているか
    float maxGaugeTime = 0f;//ゲージが最大になってからの時間

    [System.NonSerialized] public bool gameStarted = false;
    bool nowSync = true;//現在同期中が必要か


    void Awake()
    {
        foreach(Transform trans in playerRespawnPositions.transform)
        {
            respawnPlayerPositions.Add(trans.position);
        }
    }

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            this.customerList.gameObject.SetActive(true);

            PhotonNetwork.IsMessageQueueRunning = true;//同期を再開
            if(Application.platform != RuntimePlatform.Android)
            {
                //カーソル
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            nowSync = true;
            List<Player> tempPlayerList = new List<Player>(PhotonNetwork.PlayerList);
            foreach(Player temp in tempPlayerList)
            {
                Debug.Log($"{temp.NickName} {temp.ActorNumber}");
            }
            //tempPlayerList.Sort((a, b) => a.ActorNumber - b.ActorNumber);
            Debug.Log(tempPlayerList.IndexOf(PhotonNetwork.LocalPlayer));
            myPlayer = PhotonNetwork.Instantiate("Player", respawnPlayerPositions[tempPlayerList.IndexOf(PhotonNetwork.LocalPlayer)], Quaternion.identity);//resourceフォルダに入れる
            //myPlayer = PhotonNetwork.Instantiate("Player", respawnPlayerPositions[PhotonNetwork.LocalPlayer.ActorNumber - 1], Quaternion.identity);//resourceフォルダに入れる
            playerController.gameDirector = this.GetComponent<GameDirector>();
            playerController.targetObject = myPlayer;
            playerController.playerCamera = Camera.main;
            playerController.menu = this.menu;
            playerController.customerList = this.customerList;
            playerController.dontTapPanel = this.dontTapPanel;
            playerController.TargetSetting();
            playerController.onDown.AddListener(() => customerList.Close());
            myPlayer.AddComponent<AudioListener>();

            evaluationGauge.SettingGauge(minEvaluationGauge,maxEvaluationGauge,nowEvaluationGaugeValue);//怒りゲージの設定
            //if(GameVariable.isMasterClient)//マスター側で判断
            //{
                evaluationGauge.onMinEnterGauge.AddListener(() => AngerGaugeMaxStart());
                evaluationGauge.onMinExitGauge.AddListener(() => nowMinGauge = false);
            //}

            customerList.EvaluationSetting(goodGaugeTimes[EvaluationGaugeValueType.Guide],
                angerGaugeTimes[EvaluationGaugeValueType.Guide], 
                angerGaugeIntervals[EvaluationGaugeValueType.Guide]);
            customerList.onGoodAdd.AddListener(() => ChangeEvaluationGauge(goodGaugePoints[EvaluationGaugeValueType.Guide]));
            customerList.onNormalAdd.AddListener(() => ChangeEvaluationGauge(normalGaugePoints[EvaluationGaugeValueType.Guide]));
            customerList.onAngerAdd.AddListener(() => ChangeEvaluationGauge(angerGaugePoints[EvaluationGaugeValueType.Guide]));
            //customerList.onAngerStop.AddListener(() => ChangeAngerGauge(-GoodGaugeTime[EvaluationGaugeValueType.Guide]));

            playerRPC = myPlayer.GetComponent<PlayerRPC>();
            playerRPC.onCommandTriggerFalse.AddListener(() => playerController.CommandTriggerFalse());
            //スキン設定
            myPlayer.GetComponent<PlayerSelector>()
                .SetSkin(GameVariable.allPlayerSkins[PhotonNetwork.LocalPlayer.ActorNumber]);
            //menu.onMenuOpen.AddListener(() => playerController.ControllableChange(false));
            //menu.onMenuClose.AddListener(() => playerController.ControllableChange(true));
            //customerList.onCustomerListOpen.AddListener(() => CustomerListOpen());
            //customerList.onCustomerListClose.AddListener(() => CustomerListClose());

            #region GameVariable設定

            foreach (Transform trans in tables.transform)
            {
                Table tempTable = trans.GetComponent<Table>();
                tempTable.playerRPC = playerRPC;
                tempTable.tableNumber = GameVariable.tableList.Count + 1;
                trans.name = tempTable.tableNumber.ToString();
                tempTable.menu = this.menu;//UI用
                tempTable.foodStandManager = this.foodStandManager;
                tempTable.onOrderCreated.AddListener(() => playerController.CommandTriggerUpdate());
                tempTable.TableSetting();
                //怒りゲージ関係
                tempTable.EvaluationCallSetTime(goodGaugeTimes[EvaluationGaugeValueType.Call], angerGaugeTimes[EvaluationGaugeValueType.Call], angerGaugeIntervals[EvaluationGaugeValueType.Call]);
                tempTable.EvaluationOrderSetTime(goodGaugeTimes[EvaluationGaugeValueType.Order],angerGaugeTimes[EvaluationGaugeValueType.Order], angerGaugeIntervals[EvaluationGaugeValueType.Order]);
                tempTable.onGoodCallAdd.AddListener(() => ChangeEvaluationGauge(goodGaugePoints[EvaluationGaugeValueType.Call]));
                tempTable.onGoodOrderAdd.AddListener(() => ChangeEvaluationGauge(goodGaugePoints[EvaluationGaugeValueType.Order]));
                tempTable.onNormalCallAdd.AddListener(() => ChangeEvaluationGauge(normalGaugePoints[EvaluationGaugeValueType.Call]));
                tempTable.onNormalOrderAdd.AddListener(() => ChangeEvaluationGauge(normalGaugePoints[EvaluationGaugeValueType.Order]));
                tempTable.onAngerCallAdd.AddListener(() => ChangeEvaluationGauge(angerGaugePoints[EvaluationGaugeValueType.Call]));
                //tempTable.onAngerCallStop.AddListener(() => ChangeAngerGauge(-GoodGaugeTime[EvaluationGaugeValueType.Call]));
                tempTable.onAngerOrderAdd.AddListener(() => ChangeEvaluationGauge(angerGaugePoints[EvaluationGaugeValueType.Order]));
                //tempTable.onAngerOrderStop.AddListener(() => ChangeAngerGauge(-GoodGaugeTime[EvaluationGaugeValueType.Order]));
                GameVariable.tableList.Add(trans.gameObject);

            }
            int foodStandCount = 0;
            foreach (Transform trans in foodStands.transform)
            {
                trans.gameObject.name = "FoodStand" + foodStandCount;
                trans.GetComponent<FoodStand>().onFoodUpdate.AddListener(() => playerController.CommandTriggerUpdate());
                GameVariable.foodStandList.Add(trans.gameObject);
                foodStandCount++;
            }
            int cleaningToolCount = 0;
            //foreach (Transform trans in cleaningTools.transform)
            //{
            //    trans.gameObject.name = "CleaningTool" + cleaningToolCount.ToString();
            //    foreach (Transform childTrans in trans)
            //    {
            //        if (childTrans.gameObject.tag == "CleaningTool")
            //        {
            //            childTrans.gameObject.name = trans.gameObject.name;
            //        }
            //    }
            //    GameVariable.cleaningToolList.Add(trans.gameObject);
            //    cleaningToolCount++;
            //}

            //レジ
            cashRegister.GetComponent<CashRegister>().onCashRegisterUpdate.AddListener(() => playerController.CommandTriggerUpdate());
            cashRegister.GetComponent<CashRegister>().EvaluationSetting(goodGaugeTimes[EvaluationGaugeValueType.CashRegister],
                angerGaugeTimes[EvaluationGaugeValueType.CashRegister],
                angerGaugeIntervals[EvaluationGaugeValueType.CashRegister]);
            cashRegister.GetComponent<CashRegister>().onGoodCashAdd.AddListener(() => ChangeEvaluationGauge(goodGaugePoints[EvaluationGaugeValueType.CashRegister]));
            cashRegister.GetComponent<CashRegister>().onNormalCashAdd.AddListener(() => ChangeEvaluationGauge(normalGaugePoints[EvaluationGaugeValueType.CashRegister]));
            cashRegister.GetComponent<CashRegister>().onAngerCashAdd.AddListener(() => ChangeEvaluationGauge(angerGaugePoints[EvaluationGaugeValueType.CashRegister]));
            GameVariable.cashRegister = cashRegister.GetComponent<CashRegister>();
            //foreach (Transform trans in cashRegister.transform)
            //{
            //    trans.GetComponent<CashRegister>().onCashRegisterUpdate.AddListener(() => playerController.CommandTriggerUpdate());
            //    trans.GetComponent<CashRegister>().EvaluationSetting(goodGaugeTimes[EvaluationGaugeValueType.CashRegister],
            //        angerGaugeTimes[EvaluationGaugeValueType.CashRegister],
            //        angerGaugeIntervals[EvaluationGaugeValueType.CashRegister]);
            //    trans.GetComponent<CashRegister>().onGoodCashAdd.AddListener(() => ChangeEvaluationGauge(goodGaugePoints[EvaluationGaugeValueType.CashRegister]));
            //    trans.GetComponent<CashRegister>().onAngerCashAdd.AddListener(() => ChangeEvaluationGauge(angerGaugePoints[EvaluationGaugeValueType.CashRegister]));
            //    //trans.GetComponent<CashRegister>().onAngerCashStop.AddListener(() => ChangeAngerGauge(-goodGaugeTimes[EvaluationGaugeValueType.CashRegister]));
            //    GameVariable.cashRegisterList.Add(trans.gameObject);
            //
            //}
            //foreach(Transform trans in waterSpots.transform)
            //{
            //    trans.name = GameVariable.waterSpotList.Count.ToString();
            //    GameVariable.waterSpotList.Add(trans.gameObject);
            //}

            foodStandManager.FoodStandSetting(obonPrefab, wagonPrefab);
            GameVariable.onAngerChange.AddListener(() => evaluationAnger.EvaluationChange());
            GameVariable.foodDataBase = foodDB;
            GameVariable.customerDataBase = customerDB;
            GameVariable.customerGroupList = new List<CustomerGroup>(customerDB.GetCustomerList());
            GameVariable.customerMovePoint = customerMovePoint;
            if (GameVariable.isMasterClient)
            {
                //お客様リストをシャフル
                foreach (CustomerGroup temp in GameVariable.customerGroupList)
                {
                    GameVariable.customerShuffleList.Add(temp.GetCustomerNumber());
                }
                for (int i = GameVariable.customerShuffleList.Count - 1; i > 0; --i)
                {
                    int j = Random.Range(0, i + 1);
                    int temp = GameVariable.customerShuffleList[i];
                    GameVariable.customerShuffleList[i] = GameVariable.customerShuffleList[j];
                    GameVariable.customerShuffleList[j] = temp;
                }
            }

            #endregion

            dashBoard.TimerReady(allWaveTime, maxWaveCount, new List<string>(waveNames));
            informationPanel.GetComponent<CanvasGroup>().alpha = 0f;//非表示
            evaluationPanel.GetComponent<CanvasGroup>().alpha = 0f;//非表示
            buttonManager.GetComponent<CanvasGroup>().alpha = 0f;//非表示
            dontTapPanel.SetActive(false);
            startEndPanel.SetActive(true);
            startEndPanel.transform.localScale = new Vector3(1, 0, 1);
            StartCoroutine(GetOtherPlayerScript());
        }
    }

    IEnumerator GetOtherPlayerScript()
    {
        GameObject[] allPlayerObject;
        do
        {
            yield return new WaitForSeconds(0.5f);
            allPlayerObject = GameObject.FindGameObjectsWithTag("Player");//後で修正
        } while (allPlayerObject.Length < PhotonNetwork.PlayerList.Length);
        Debug.Log(allPlayerObject.Length);
        foreach (GameObject temp in allPlayerObject)
        {
            PlayerRPC tempRPC = temp.GetComponent<PlayerRPC>();
            tempRPC.gameDirector = this;
            tempRPC.onGameStart.AddListener(() => GameStart());
            tempRPC.onGameFinish.AddListener(() => GameFinish());
            tempRPC.onCommandTriggerUpdate.AddListener(() => playerController.CommandTriggerUpdate());
            tempRPC.onCommandTriggerTrue.AddListener(() => playerController.CommandTriggerTrue());
            //tempRPC.onCommandTriggerFalse.AddListener(() => playerController.CommandTriggerFalse());
            tempRPC.onActionTriggerUpdate.AddListener(() => playerController.ActionTrigger());

            tempRPC.evaluationGauge = evaluationGauge;
            tempRPC.helpPanel = helpPanel;
            tempRPC.waterPrefab = this.waterPrefab;
            tempRPC.cleanupObjectPrefab = this.cleanupObjectPrefab;

            tempRPC.customerList = this.customerList;
            tempRPC.familyPrefab = this.familyPrefab;
            tempRPC.customerSpawnPosition = this.CustomerSpawnPosition;

            tempRPC.SettingNameTag(tempRPC.gameObject.GetComponent<PhotonView>().Owner.NickName);
            //Family用
            tempRPC.EvaluationGuideSetting(goodGaugeTimes[EvaluationGaugeValueType.Guide2],
                angerGaugeTimes[EvaluationGaugeValueType.Guide2],
                angerGaugeIntervals[EvaluationGaugeValueType.Guide2]);
            //スキン設定
            temp.GetComponent<PlayerSelector>()
                .SetSkin(GameVariable.allPlayerSkins[tempRPC.gameObject.GetComponent<PhotonView>().Owner.ActorNumber]);
            playerRPCs.Add(tempRPC);
            GameVariable.allPlayerTransform.Add(tempRPC.gameObject.transform);
        }
        playerRPC.SettingOK();

        if(GameVariable.isMasterClient)
        {
            int count = 0;
            do
            {
                yield return new WaitForSeconds(0.5f);
                count = 0;
                foreach (PlayerRPC script in playerRPCs)
                {
                    if (script.gameSettingOK)
                    {
                        count++;
                    }
                }
            } while (count < PhotonNetwork.PlayerList.Length);
            Debug.Log("SettingOK GameStart");
            playerRPC.GameStart();
        }

        //表示用に並び替え
        playerRPCs.Sort((a, b) => a.gameObject.GetComponent<PhotonView>().Owner.ActorNumber - b.gameObject.GetComponent<PhotonView>().Owner.ActorNumber);
    }

    void GameStart()
    {
        StartCoroutine(GameStartDelay());
        IEnumerator GameStartDelay()
        {
            startEndText.DOFade(0, 0);
            startEndText.transform.DOLocalMoveX(-panelTextMoveRange, 0);

            yield return new WaitForSeconds(1.0f);
            FadePanel.Instance.ManualSceneFadeMode(false);
            yield return new WaitForSeconds(1.0f);
            startEndPanel.transform.DOScaleY(1, panelMoveTime);
            yield return new WaitForSeconds(0.5f);
            startEndText.text = "Ready...";
            startEndText.transform.DOLocalMoveX(0, panelMoveTime).SetEase(Ease.OutCubic);
            startEndText.DOFade(1, panelMoveTime).SetEase(Ease.OutCubic);
            yield return new WaitForSeconds(1f);
            startEndText.transform.DOLocalMoveX(panelTextMoveRange, panelMoveTime).SetEase(Ease.InCubic);
            startEndText.DOFade(0, panelMoveTime).SetEase(Ease.InCubic);
            yield return new WaitForSeconds(panelMoveTime * 2);
            startEndText.transform.DOLocalMoveX(-panelTextMoveRange, 0);
            startEndText.text = "Start!";
            startEndText.transform.DOLocalMoveX(0, panelMoveTime).SetEase(Ease.OutCubic);
            startEndText.DOFade(1, panelMoveTime).SetEase(Ease.OutCubic);


            //開始
            SoundList.Instance.SoundEffectPlay(7, 0.5f);//開始の音
            playerController.nowSelectScene = PlayerController.InputScene.Game;
            playerController.ControllableChange(true);//操作可能にする
            informationPanel.GetComponent<CanvasGroup>().alpha = 1f;//表示
            evaluationPanel.GetComponent<CanvasGroup>().alpha = 1f;//表示
            buttonManager.GetComponent<CanvasGroup>().alpha = 1f;//非表示
            dashBoard.onTimerFinished.AddListener(() => GameFinishMaster());
            dashBoard.onUpdateWave.AddListener(() => WaveManager());
            dashBoard.TimerStart();
            gameStarted = true;
            playerController.ActionTrigger();
            SoundList.Instance.BGM(SoundList.PlayMode.Play, 2, 0.5f);
            if (GameVariable.isMasterClient)
            {
                StartCoroutine(AddCustomer());
            }
            
            yield return new WaitForSeconds(1.0f);
            startEndPanel.transform.DOScaleY(0, panelMoveTime).OnComplete(() => startEndText.text = "");
        }
    }

    void GameFinishMaster()
    {
        if (GameVariable.isMasterClient)
        {
            playerRPC.GameFinish(nowEvaluationGaugeValue);
        }
    }

    void GameFinish()//通常終了
    {
        nowSync = false;
        SoundList.Instance.SoundEffectPlay(8, 0.5f);//終了の音
        //GameVariable.finalEvaluation = nowEvaluationGaugeValue;
        dontTapPanel.SetActive(true);
        playerController.ControllableChange(false);
        gameStarted = false;
        startEndText.text = "Finish!";
        startEndText.DOFade(0, 0);
        startEndText.transform.DOLocalMoveX(-panelTextMoveRange, 0);
        startEndPanel.transform.DOScaleY(1, panelMoveTime);
        startEndText.transform.DOLocalMoveX(0, panelMoveTime).SetEase(Ease.OutCubic).SetDelay(panelMoveTime);
        startEndText.DOFade(1, panelMoveTime).SetEase(Ease.OutCubic).SetDelay(panelMoveTime);


        StartCoroutine(Delay());
        IEnumerator Delay()
        {
            yield return new WaitForSeconds(3.0f);
            SoundList.Instance.BGM(SoundList.PlayMode.Stop);
            FadePanel.Instance.AutoSceneFadeMode("ResultScene");
        }
    }

    void GameOver()//ゲームオーバー
    {
        dashBoard.onTimerFinished.RemoveAllListeners();
        dashBoard.onUpdateWave.RemoveAllListeners();
        dashBoard.TimerStop(false);
        dontTapPanel.SetActive(true);
        playerController.ControllableChange(false);
        gameStarted = false;
        startEndText.text = "GameOver...";
    }

    void WaveManager()
    {
        Debug.Log("Wave切り替わり");
        if(dashBoard.NowWaveCount() >= 3)//ディナーになったら評価を非表示にする
        {
            Vector2 tempSize = evaluationPanel.sizeDelta;
            evaluationGauge.gameObject.GetComponent<CanvasGroup>().
                DOFade(0, 0.5f).SetEase(Ease.OutCubic);
            evaluationPanel.GetComponent<RectTransform>().
                DOSizeDelta(new Vector2(tempSize.x,tempSize.y / 2),0.5f).
                SetEase(Ease.OutCubic).SetDelay(0.5f);
        }
    }

    public void ChangeEvaluationGauge(int changeValue)
    {
        if (GameVariable.isMasterClient && gameStarted)
        {

            nowEvaluationGaugeValue += changeValue;
            //値調整
            nowEvaluationGaugeValue = nowEvaluationGaugeValue > maxEvaluationGauge ? maxEvaluationGauge : nowEvaluationGaugeValue < minEvaluationGauge ? minEvaluationGauge : nowEvaluationGaugeValue;
            playerRPC.ChangeEvaluationGauge(nowEvaluationGaugeValue);
        }
    }

    void AngerGaugeMaxStart()
    {
        maxGaugeTime = 0f;
        nowMinGauge = true;
    }

    IEnumerator AddCustomer()
    {
        while(GameVariable.customerShuffleList.Count > 0 && gameStarted)
        {
            if(GameVariable.customerCount <= 1)
            {
                yield return new WaitForSeconds(customerSpawnTimeFirst);
            }
            else
            {
                yield return new WaitForSeconds(Random.Range(customerSpawnTimeMin, customerSpawnTimeMax));
            }
            playerRPC.AddCustomer(GameVariable.customerGroupList[GameVariable.customerShuffleList[0]].GetCustomerNumber());
            GameVariable.customerShuffleList.RemoveAt(0);
            GameVariable.customerCount++;
        }
        yield return null;
    }

    public void GuideCustomer(int tapNumber,CustomerGroup customerGroup)
    {
        playerRPC.GuideCustomer(tapNumber, customerGroup);
    }

    // 他プレイヤーがルームから退出した時に呼ばれるコールバック
    public override void OnPlayerLeftRoom(Player exitPlayer)
    {
        if (nowSync)
        {
            Debug.Log($"{exitPlayer.NickName}が退出しました");
            SoundList.Instance.BGM(SoundList.PlayMode.Stop);
            PhotonNetwork.Disconnect();
            FadePanel.Instance.AutoSceneFadeMode("TitleScene");
        }
    }


    
    void Update()
    {
        if (gameStarted)
        {

            //if (playerRPC.nowDown)
            //{
            //    tempMainText.text = "ダウン中...";
            //}
            //else if(tempMainText.text == "ダウン中...")
            //{
            //    tempMainText.text = "";
            //}

            if (nowMinGauge)//怒りが最大になってたら
            {
                maxGaugeTime += Time.deltaTime;
                if(maxGaugeTime >= maxEvaluationFinishTime)
                {
                    //nowMinGauge = false;
                    //evaluationGauge.onMinEnterGauge.RemoveAllListeners();
                    //evaluationGauge.onMinExitGauge.RemoveAllListeners();
                    //GameOver();
                }
            }

        }
        
    }

    
}
