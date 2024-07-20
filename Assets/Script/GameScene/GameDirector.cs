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
    //��

    [SerializeField][Header("�SWave�̍��v����")] float allWaveTime = 270f;//4��30�b
    [SerializeField][Header("�l�̐ݒ�\nWave�̍ő吔")] int maxWaveCount = 3;
    [SerializeField][Header("�eWave�̖��O")] string[] waveNames = new string[] { "���[�j���O", "�����`", "�f�B�i�[" };
    [SerializeField][Header("�ŏ��̂��q����̐����Ԋu(�Œ�)")] float customerSpawnTimeFirst = 2f;
    [SerializeField][Header("���q����̐����Ԋu(�ŏ�)")] float customerSpawnTimeMin = 5f;
    [SerializeField][Header("���q����̐����Ԋu(�ő�)")] float customerSpawnTimeMax = 12f;

    [SerializeField][Header("�{��Q�[�W�̍ŏ�")] int minEvaluationGauge = 0;
    [SerializeField][Header("�{��Q�[�W�̍ő�")] int maxEvaluationGauge = 100;
    [SerializeField][Header("�{��Q�[�W���ő�ɂȂ��Ă���I���܂ł̗P�\����")] float maxEvaluationFinishTime = 10f;

    [SerializeField][Header("Player�����ʒu")] GameObject playerRespawnPositions;
    List<Vector3> respawnPlayerPositions = new List<Vector3>();

    //UI
    float panelMoveTime = 0.3f;
    float panelTextMoveRange = 50f;

    /*
    //�e�v���C���[�����ʒu
    Vector3[] respawnPlayerPositions = new Vector3[4] {
        new Vector3(-5,1.5f,6f),
        new Vector3(-3, 1.5f, 6f),
        new Vector3(-1, 1.5f, 6f),
        new Vector3(1, 1.5f, 6f)
    };
    */
    //�]���n
    public enum EvaluationGaugeValueType//�]���^�C�v
    {
        Guide,//�ē�(�ĂԂ܂�)
        Guide2,//�ē�(�Ă�ł���Ȃɍ���܂�)
        Call,//�X�����Ă�
        Order,//���������Ȃ�
        CashRegister,//��v�o���Ȃ�
        Dirty//��������
    }


    //�ǂ�����ɂȂ鎞��
    public readonly Dictionary<EvaluationGaugeValueType, int> goodGaugeTimes = new Dictionary<EvaluationGaugeValueType, int>() { 
        {EvaluationGaugeValueType.Guide, 10 },
        {EvaluationGaugeValueType.Guide2, 10 },
        {EvaluationGaugeValueType.Call, 10 },
        {EvaluationGaugeValueType.Order, 10 },
        {EvaluationGaugeValueType.CashRegister, 10 },
        {EvaluationGaugeValueType.Dirty, 10 }
    };

    //�ǂ�����̃|�C���g
    public readonly Dictionary<EvaluationGaugeValueType, int> goodGaugePoints = new Dictionary<EvaluationGaugeValueType, int>() {
        {EvaluationGaugeValueType.Guide, 5 },
        {EvaluationGaugeValueType.Guide2, 5 },
        {EvaluationGaugeValueType.Call, 5 },
        {EvaluationGaugeValueType.Order, 5 },
        {EvaluationGaugeValueType.CashRegister, 5 },
        {EvaluationGaugeValueType.Dirty, 5 }
    };

    //�m�[�}������̃|�C���g
    public readonly Dictionary<EvaluationGaugeValueType, int> normalGaugePoints = new Dictionary<EvaluationGaugeValueType, int>(){
        {EvaluationGaugeValueType.Guide, 2 },
        {EvaluationGaugeValueType.Guide2, 2 },
        {EvaluationGaugeValueType.Call, 2 },
        {EvaluationGaugeValueType.Order, 2 },
        {EvaluationGaugeValueType.CashRegister, 2 },
        {EvaluationGaugeValueType.Dirty, 2 }
    };

    //�{�肪���܂�n�߂�܂ł̎���
    public readonly Dictionary<EvaluationGaugeValueType, int> angerGaugeTimes = new Dictionary<EvaluationGaugeValueType, int>() {
        {EvaluationGaugeValueType.Guide, 10 },
        {EvaluationGaugeValueType.Guide2, 10 },
        {EvaluationGaugeValueType.Call,10 },
        {EvaluationGaugeValueType.Order,10 },
        {EvaluationGaugeValueType.CashRegister,10 },
        {EvaluationGaugeValueType.Dirty,10 }
    };

    //�{�蔻��̃|�C���g�����b���Ƃɕς�邩�ς�邩
    public readonly Dictionary<EvaluationGaugeValueType, int> angerGaugeIntervals = new Dictionary<EvaluationGaugeValueType, int>() {
        {EvaluationGaugeValueType.Guide, 4 },
        {EvaluationGaugeValueType.Guide2, 4 },
        {EvaluationGaugeValueType.Call, 4 },
        {EvaluationGaugeValueType.Order, 4 },
        {EvaluationGaugeValueType.CashRegister, 4 },
        {EvaluationGaugeValueType.Dirty, 4 }
    };

    //�{�蔻��̃|�C���g�����ŉ��|�C���g�ς�邩
    public readonly Dictionary<EvaluationGaugeValueType, int> angerGaugePoints = new Dictionary<EvaluationGaugeValueType, int>() {
        {EvaluationGaugeValueType.Guide, -1 },
        {EvaluationGaugeValueType.Guide2, -1 },
        {EvaluationGaugeValueType.Call,-1 },
        {EvaluationGaugeValueType.Order,-1 },
        {EvaluationGaugeValueType.CashRegister,-1 },
        {EvaluationGaugeValueType.Dirty,-1 }
    };

    /////////////////////////////////////////////////////

    [SerializeField][Header("�X�N���v�g")] PlayerController playerController;//�X�N���v�g
    [SerializeField] Menu menu;//�X�N���v�g
    [SerializeField] FoodStandManager foodStandManager;//�X�N���v�g
    [SerializeField] CustomerList customerList;//�X�N���v�g
    [SerializeField] GameObject informationPanel;//�C���t�H���[�V�����p�l��
    [SerializeField] DashBoard dashBoard;//�_�b�V���{�[�h
    [SerializeField] EvaluationGauge evaluationGauge;//�]���Q�[�W�X�N���v�g
    [SerializeField] EvaluationAnger evaluationAnger;//�{�萔�p�l��
    [SerializeField] HelpPanel helpPanel;//Help�p�l��

    [SerializeField][Header("UI")] Text startEndText;
    [SerializeField] GameObject startEndPanel;
    [SerializeField] RectTransform evaluationPanel;//�]���̃p�l��
    [SerializeField] ButtonManager buttonManager;

    [SerializeField] GameObject dontTapPanel;//��ʂ�G��Ȃ�����

    [SerializeField][Header("�I�u�W�F�N�g")] GameObject foodStands;//�t�[�h�X�^���h�������Ă���e�I�u�W�F�N�g
    [SerializeField] GameObject tables;//�e�[�u���������Ă���e�I�u�W�F�N�g
    [SerializeField] GameObject cashRegister;//���W
    [SerializeField] GameObject cleaningTools;//�|���p������Ă���e�I�u�W�F�N�g
    [SerializeField] GameObject waterSpots;//�E�H�[�^�[�T�[�o�[�������Ă���e�I�u�W�F�N�g
    
    //prefab
    [SerializeField][Header("Prefab")] GameObject waterPrefab;//�v���C���[�������̃I�u�W�F�N�g�v���n�u
    [SerializeField] GameObject obonPrefab;//�������悹�邨�~�̃I�u�W�F�N�g�v���n�u
    [SerializeField] GameObject wagonPrefab;//�ł����������悹�郏�S���̃I�u�W�F�N�g�v���n�u
    [SerializeField] GameObject cleanupObjectPrefab;//�e�[�u����|��������̃I�u�W�F�N�g�v���n�u
    [SerializeField] GameObject familyPrefab;//���q����̐e�I�u�W�F�N�g�̃v���n�u
    //�ʒu
    [SerializeField] Transform CustomerSpawnPosition;//���q���񂪃X�|�[������n�_

    [SerializeField] CustomerMovePoint customerMovePoint;//���q����p�̃|�C���g�܂Ƃ�

    [SerializeField][Header("�f�[�^�x�[�X")] FoodDataBase foodDB;
    [SerializeField] CustomerGroupDataBase customerDB;

    [SerializeField][Header("�v���n�u")] GameObject cleanupObject;


    /////////////////////////////////////////////////////

    PlayerRPC playerRPC;
    List<PlayerRPC> playerRPCs = new List<PlayerRPC>();
    
    
    //�v���C���[�A�^�b�`�p
    GameObject myPlayer;

    //���݂̃E�F�[�u
    int nowWaveCount = 0;

    //�{��Q�[�W�֘A
    int nowEvaluationGaugeValue = 50;//���݂̓{��Q�[�W
    bool nowMinGauge = false;//���݃Q�[�W���ݒ�l��菬�����Ȃ��Ă��邩
    float maxGaugeTime = 0f;//�Q�[�W���ő�ɂȂ��Ă���̎���

    [System.NonSerialized] public bool gameStarted = false;
    bool nowSync = true;//���ݓ��������K�v��


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

            PhotonNetwork.IsMessageQueueRunning = true;//�������ĊJ
            if(Application.platform != RuntimePlatform.Android)
            {
                //�J�[�\��
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
            myPlayer = PhotonNetwork.Instantiate("Player", respawnPlayerPositions[tempPlayerList.IndexOf(PhotonNetwork.LocalPlayer)], Quaternion.identity);//resource�t�H���_�ɓ����
            //myPlayer = PhotonNetwork.Instantiate("Player", respawnPlayerPositions[PhotonNetwork.LocalPlayer.ActorNumber - 1], Quaternion.identity);//resource�t�H���_�ɓ����
            playerController.gameDirector = this.GetComponent<GameDirector>();
            playerController.targetObject = myPlayer;
            playerController.playerCamera = Camera.main;
            playerController.menu = this.menu;
            playerController.customerList = this.customerList;
            playerController.dontTapPanel = this.dontTapPanel;
            playerController.TargetSetting();
            playerController.onDown.AddListener(() => customerList.Close());
            myPlayer.AddComponent<AudioListener>();

            evaluationGauge.SettingGauge(minEvaluationGauge,maxEvaluationGauge,nowEvaluationGaugeValue);//�{��Q�[�W�̐ݒ�
            //if(GameVariable.isMasterClient)//�}�X�^�[���Ŕ��f
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
            //�X�L���ݒ�
            myPlayer.GetComponent<PlayerSelector>()
                .SetSkin(GameVariable.allPlayerSkins[PhotonNetwork.LocalPlayer.ActorNumber]);
            //menu.onMenuOpen.AddListener(() => playerController.ControllableChange(false));
            //menu.onMenuClose.AddListener(() => playerController.ControllableChange(true));
            //customerList.onCustomerListOpen.AddListener(() => CustomerListOpen());
            //customerList.onCustomerListClose.AddListener(() => CustomerListClose());

            #region GameVariable�ݒ�

            foreach (Transform trans in tables.transform)
            {
                Table tempTable = trans.GetComponent<Table>();
                tempTable.playerRPC = playerRPC;
                tempTable.tableNumber = GameVariable.tableList.Count + 1;
                trans.name = tempTable.tableNumber.ToString();
                tempTable.menu = this.menu;//UI�p
                tempTable.foodStandManager = this.foodStandManager;
                tempTable.onOrderCreated.AddListener(() => playerController.CommandTriggerUpdate());
                tempTable.TableSetting();
                //�{��Q�[�W�֌W
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

            //���W
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
                //���q�l���X�g���V���t��
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
            informationPanel.GetComponent<CanvasGroup>().alpha = 0f;//��\��
            evaluationPanel.GetComponent<CanvasGroup>().alpha = 0f;//��\��
            buttonManager.GetComponent<CanvasGroup>().alpha = 0f;//��\��
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
            allPlayerObject = GameObject.FindGameObjectsWithTag("Player");//��ŏC��
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
            //Family�p
            tempRPC.EvaluationGuideSetting(goodGaugeTimes[EvaluationGaugeValueType.Guide2],
                angerGaugeTimes[EvaluationGaugeValueType.Guide2],
                angerGaugeIntervals[EvaluationGaugeValueType.Guide2]);
            //�X�L���ݒ�
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

        //�\���p�ɕ��ёւ�
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


            //�J�n
            SoundList.Instance.SoundEffectPlay(7, 0.5f);//�J�n�̉�
            playerController.nowSelectScene = PlayerController.InputScene.Game;
            playerController.ControllableChange(true);//����\�ɂ���
            informationPanel.GetComponent<CanvasGroup>().alpha = 1f;//�\��
            evaluationPanel.GetComponent<CanvasGroup>().alpha = 1f;//�\��
            buttonManager.GetComponent<CanvasGroup>().alpha = 1f;//��\��
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

    void GameFinish()//�ʏ�I��
    {
        nowSync = false;
        SoundList.Instance.SoundEffectPlay(8, 0.5f);//�I���̉�
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

    void GameOver()//�Q�[���I�[�o�[
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
        Debug.Log("Wave�؂�ւ��");
        if(dashBoard.NowWaveCount() >= 3)//�f�B�i�[�ɂȂ�����]�����\���ɂ���
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
            //�l����
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

    // ���v���C���[�����[������ޏo�������ɌĂ΂��R�[���o�b�N
    public override void OnPlayerLeftRoom(Player exitPlayer)
    {
        if (nowSync)
        {
            Debug.Log($"{exitPlayer.NickName}���ޏo���܂���");
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
            //    tempMainText.text = "�_�E����...";
            //}
            //else if(tempMainText.text == "�_�E����...")
            //{
            //    tempMainText.text = "";
            //}

            if (nowMinGauge)//�{�肪�ő�ɂȂ��Ă���
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
