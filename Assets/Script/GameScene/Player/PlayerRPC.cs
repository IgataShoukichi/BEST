using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerRPC : MonoBehaviourPunCallbacks,IPunObservable
{
    ////////////////////////////////////////////////////////////////

    float jumpPower = 700f;
    float playerAttackPower = 600f;
    float downTime = 0.7f;//�_�E�����Ă��鎞��

    const float interpolationTime = 0.05f;//��Ԏ���

    //�|�C���g�z��
    public enum PlayerPlusCategory//���_�|�C���g
    {
        CustomerGuide,//���q����ē�
        PutWater,//���̒�
        Order,//���������
        PutFood,//������z�V����
        CleanupTable,//�e�[�u����ЂÂ���
        CashRegister,//��v������
        Cleaning//�����|������
    };

    public readonly Dictionary<PlayerPlusCategory, int> playerPlusPoint = new Dictionary<PlayerPlusCategory, int>()
    {
        { PlayerPlusCategory.CustomerGuide,2},
        { PlayerPlusCategory.PutWater,1},
        { PlayerPlusCategory.Order,2},
        { PlayerPlusCategory.PutFood,3},
        { PlayerPlusCategory.CleanupTable,2},
        { PlayerPlusCategory.CashRegister ,1},
        { PlayerPlusCategory.Cleaning,2},

    };

    public enum PlayerMinusCategory//���_�|�C���g
    {
        Attack,//����(�������l)
        Rush,//�ːi(�������l)
        Drop//�����𗎂Ƃ�(���Ƃ����l)
    };


    public readonly Dictionary<PlayerMinusCategory, int> playerMinusPoint = new Dictionary<PlayerMinusCategory, int>()
    {
        {PlayerMinusCategory.Attack , -1 },
        {PlayerMinusCategory.Rush , -1 },
        {PlayerMinusCategory.Drop , -1 },
    };
    ////////////////////////////////////////////////////////////////

    public Action<string> onDestinationTextChange = null;

    //UnityEvent
    [System.NonSerialized] public UnityEvent onGameStart = new UnityEvent();//�Q�[�����X�^�[�g����Ƃ�
    [System.NonSerialized] public UnityEvent onGameFinish = new UnityEvent();//�Q�[�����I�����ꂽ�Ƃ�
    [System.NonSerialized] public UnityEvent onCommandTriggerUpdate = new UnityEvent();//�����蔻��X�V
    [System.NonSerialized] public UnityEvent onCommandTriggerTrue = new UnityEvent();//����������
    [System.NonSerialized] public UnityEvent onCommandTriggerFalse = new UnityEvent();//���ꂽ�Ƃ��܂��̓L�����Z������Ƃ�
    [System.NonSerialized] public UnityEvent onActionTriggerUpdate = new UnityEvent();//�A�N�V�������X�V���ꂽ�Ƃ�
    [System.NonSerialized] public UnityEvent onDown = new UnityEvent();//�������_�E�������Ƃ�
    [System.NonSerialized] public UnityEvent onRecover = new UnityEvent();//�������_�E�����痧����������
    [System.NonSerialized] public UnityEvent onStopMove = new UnityEvent();//�������~�߂�����

    //�v���C���[�̓��_
    [System.NonSerialized] public int playerPoint = 0;
    [System.NonSerialized] public GameDirector gameDirector;
    

    Transform myTransform = null;
    Rigidbody myRigidbody = null;
    [SerializeField] PlayerSelector playerSelector;
    [System.NonSerialized] public PlayerAnimationController animationController;
    [SerializeField] MiniAudio miniAudio;
    [SerializeField] OtherPlayerTrigger otherPlayerTrigger;
    [SerializeField] Transform havingPosition;
    FixedJoint playerJoint;//���S���̂���
    [SerializeField] Text playerNameText;
    [SerializeField] GameObject taskCompleteMark;
    [SerializeField] GameObject helpMark;

    //�G�t�F�N�g
    [SerializeField] ParticleSystem walkEffect;
    [SerializeField] ParticleSystem landEffect;
    [SerializeField] ParticleSystem pushEffect;

    Vector3 taskCompleteMarkLocalPosition;
    float tackComplateMarkScale;
    Vector3 helpMarkLocalPosition;

    //���`��ԗp
    float interpolationElapsedTime = 0f;
    //transform
    Vector3 baseTransform;
    Vector3 receptionTransform;
    //rotation
    Quaternion baseRotation;
    Quaternion receptionRotation;

    //Director����
    [System.NonSerialized] public EvaluationGauge evaluationGauge;//�{��Q�[�W(Director)
    [System.NonSerialized] public HelpPanel helpPanel;//�w���v�p�l��
    [System.NonSerialized] public GameObject waterPrefab;//�v���C���[�������̃I�u�W�F�N�g�v���n�u(Director)
    [System.NonSerialized] public GameObject cleanupObjectPrefab;//�e�[�u����|��������̃I�u�W�F�N�g�v���n�u(Director)
    [System.NonSerialized] public CustomerList customerList;
    [System.NonSerialized] public GameObject familyPrefab;
    [System.NonSerialized] public Transform customerSpawnPosition;

    [System.NonSerialized] public bool gameSettingOK = false;

    [System.NonSerialized] public bool nowHaving = false;//���݂��̂������Ă��邩
    GameObject nowHavingCleaningTool;//���ݎ����Ă���Cleaning�c�[��
    GameObject parentObject = null;//�ڐG���̃I�u�W�F�N�g
    [System.NonSerialized] public CustomerGroup nowGuideCustomer = null;//���݈ē����̂��q����
    string touchStandName = null;//���ݐG��Ă���X�^���h
    string touchNormalFoodTableName = null;//���ݐG��Ă���e�[�u��
    string havingNormalFoodTableName = null;//���ݎ����Ă��闿���̃e�[�u��
    string touchBigFoodTableName = null;//���ݐG��Ă���e�[�u��
    string havingBigFoodTableName = null;//���ݎ����Ă��闿���̃e�[�u��

    GameObject touchTable = null;//���ݐG��Ă���e�[�u��
    GameObject touchWaterSpot = null;//���ݐG��Ă���E�H�[�^�[�T�[�o�[
    GameObject touchCleaningToolStand = null;//���ݐG��Ă���|���p��
    GameObject nowWorkingObject = null;//���ݍ�ƒ��̃I�u�W�F�N�g
    //Action exitWorkingAction = null;//���̐l�ɉ����ꂽ�肵�č�Ƃ����f���ꂽ�Ƃ��Ɏ��s����Action
    Vector3 havingScale = Vector3.one;

    //Family�p
    float goodGuide2Time = 0f;
    float angerGuide2FirstTime = 0f;
    float angerGuide2Interval = 0f;

    //���ʗp
    //�ړ��n
    [System.NonSerialized] public bool nowJoint = false;//���݃W���C���g���Ă��邩
    [System.NonSerialized] public bool nowJump = false;//���݃W�����v����

    //�R�}���h�n
    [System.NonSerialized] public bool nowWarking = false;//���ݍ�Ƃ��Ă��邩
    [System.NonSerialized] public bool havingOK = false;//���Ă邩���ĂȂ���
    [System.NonSerialized] public bool havingWagon = false;//���S�������Ă邩
    [System.NonSerialized] public bool havingWater = false;//�������Ă邩
    [System.NonSerialized] public bool foodPut = false;//�H�ו���u���邩
    [System.NonSerialized] public bool putWagon = false;//���S����u���邩
    [System.NonSerialized] public bool foodDrop = false;//�H�ו����̂Ă��邩
    [System.NonSerialized] public bool orderTrue = false;//�������󂯂��邩
    [System.NonSerialized] public bool guideCustomer = false;//���q������ē��ł��邩
    [System.NonSerialized] public bool sitdownCustomer = false;//���q��������点���邩
    [System.NonSerialized] public bool cleaningToolPut = false;//�|���p���u���邩
    [System.NonSerialized] public bool cleanupTable = false;//�e�[�u����|���ł��邩
    [System.NonSerialized] public bool workCashRegister = false;//��v�ł��邩
    bool waterPut = false;//����u���邩

    //�A�N�V�����n
    [System.NonSerialized] public bool actionTrue1 = false;//�A�N�V����1�����s�ł��邩
    [System.NonSerialized] public bool actionTrue2 = false;//�A�N�V����2�����s�ł��邩

    [System.NonSerialized] public bool action1CoolTime = false;//�A�N�V����1�{�^�����N�[���^�C������
    [System.NonSerialized] public bool action2CoolTime = false;//�A�N�V����2�{�^�����N�[���^�C������
    [System.NonSerialized] public bool nowDown = false;//���݃_�E�����Ă��邩



    void Awake()
    {
        myTransform = this.transform;
        myRigidbody = this.GetComponent<Rigidbody>();
        playerSelector.onSetSkin.AddListener((controller) => animationController = controller);
    }

    //void OnDrawGizmos()
    //{
    //   Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(this.transform.position, 0.5f);
    //    //Gizmos.DrawWireCube(transform.position + Vector3.down * 0.2f, new Vector3(0.8f, 0.4f, 0.8f));
    //}

    void Start()
    {
        this.gameObject.layer = 9;
        
        otherPlayerTrigger.onOtherPlayerTriggerEnter.RemoveAllListeners();
        otherPlayerTrigger.onOtherPlayerTriggerExit.RemoveAllListeners();
        otherPlayerTrigger.onOtherPlayerTriggerEnter.AddListener(() => ActionTriggerUpdate(true,2));
        otherPlayerTrigger.onOtherPlayerTriggerExit.AddListener(() => ActionTriggerUpdate(false,2));

        taskCompleteMarkLocalPosition = taskCompleteMark.transform.localPosition;
        tackComplateMarkScale = taskCompleteMark.transform.localScale.x;
        helpMarkLocalPosition = helpMark.transform.localPosition;
        taskCompleteMark.SetActive(false);
        helpMark.SetActive(false);
        ActionTriggerUpdate(true);
    }

    void Update()
    {
        if(!photonView.IsMine)//�Ȃ߂炩�Ɉړ�����
        {
            //if(myRigidbody.velocity != Vector3.zero)
            //{
            //    myAnimationController.AnimationPlay(PlayerAnimationController.AnimationName.walk);
            //}
            //else
            //{
            //    myAnimationController.AnimationPlay(PlayerAnimationController.AnimationName.neutral);
            //}
            interpolationElapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(baseTransform, receptionTransform, interpolationElapsedTime / interpolationTime);
            transform.rotation = Quaternion.Lerp(baseRotation,receptionRotation, interpolationElapsedTime / interpolationTime);
        }
    }

    void FixedUpdate()
    {
        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(myTransform.position);
            stream.SendNext(myTransform.rotation);
            stream.SendNext(myRigidbody.velocity);
        }
        else
        {
            baseTransform = transform.position;
            receptionTransform = (Vector3)stream.ReceiveNext();
            baseRotation = transform.rotation;
            receptionRotation = (Quaternion)stream.ReceiveNext();
            myRigidbody.velocity = (Vector3)stream.ReceiveNext();
            interpolationElapsedTime = 0f;
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.layer != 8)
        {
            parentObject = collision.transform.parent.gameObject;
            Debug.Log(parentObject.name);
            onCommandTriggerUpdate.Invoke();
        }
    }

    public void SettingNameTag(string nameText)
    {
        if (!photonView.IsMine)
        {
            playerNameText.text = nameText;
        }
        else
        {
            playerNameText.text = "";
        }
    }

    public void TackCompleteMark()
    {
        taskCompleteMark.SetActive(true);
        taskCompleteMark.transform.localScale = Vector3.zero;
        taskCompleteMark.transform.GetChild(0).GetComponent<Image>().fillAmount = 0f;
        //playerNameText.enabled = false;
        taskCompleteMark.transform.DOScale(tackComplateMarkScale, 0.2f).SetEase(Ease.OutCubic);
        taskCompleteMark.transform.GetChild(0).GetComponent<Image>().DOFillAmount(1f, 0.3f).SetDelay(0.1f).SetEase(Ease.OutCubic);
        //miniAudio?.SoundPlay(1, 1f);//�^�X�N������
        if(photonView.IsMine)
        {
            SoundList.Instance.SoundEffectPlay(13, 1f);//�^�X�N������
        }
        
        taskCompleteMark.transform.DOScale(0, 0.3f).SetDelay(0.7f).
            SetEase(Ease.InCubic).OnComplete(() => TaskComplateDelay());
        /*
        taskCompleteMark.SetActive(true);
        playerNameText.enabled = false;
        taskCompleteMark.transform.localPosition = taskCompleteMarkLocalPosition + Vector3.down * 0.2f;
        //taskCompleteMark.transform.DOLocalMoveY(taskCompleteMarkLocalPosition.y - 0.2f, 0f);
        taskCompleteMark.transform.DOLocalMoveY(taskCompleteMarkLocalPosition.y, 0.2f).SetEase(Ease.OutCubic);
        taskCompleteMark.transform.DOLocalMoveY(taskCompleteMarkLocalPosition.y + 0.2f, 0.2f).SetDelay(0.5f).
            SetEase(Ease.InCubic).OnComplete(TaskComplateDelay);
        */
    }

    void TaskComplateDelay()
    {
        taskCompleteMark.SetActive(false);
        taskCompleteMark.transform.localScale = Vector3.one * tackComplateMarkScale;
        //playerNameText.enabled = true;
    }

    public void HelpMark()
    {
        helpMark.SetActive(true);
        helpMark.GetComponent<CanvasGroup>().DOKill();
        helpMark.GetComponent<CanvasGroup>().alpha = 1f;
        helpMark.transform.localPosition = helpMarkLocalPosition;
        helpMark.GetComponent<CanvasGroup>().DOFade(0f, 0.2f).SetDelay(2f).SetEase(Ease.InCubic).
            OnComplete(() => helpMark.SetActive(false));

    }

    public void CommandTriggerUpdate()
    {
        if(parentObject != null)
        {
            //����or�u������
            if (!nowHaving)//�����Ă��Ȃ��ꍇ
            {
                if (parentObject.tag == "FoodStand" && nowGuideCustomer == null)
                {
                    if (null != parentObject.GetComponent<FoodStand>().FoodGetInformation())
                    {
                        touchNormalFoodTableName = parentObject.GetComponent<FoodStand>().FoodGetInformation();
                        touchStandName = parentObject.name;
                        havingOK = true;
                    }
                    else
                    {
                        touchStandName = null;
                        havingOK = false;
                    }
                }
                else if(parentObject.tag == "Wagon" && nowGuideCustomer == null)
                {
                    if (parentObject.GetComponent<Wagon>().SetPossible() != null)
                    {
                        touchBigFoodTableName = parentObject.GetComponent<Wagon>().SetPossible();
                        havingWagon = true;
                    }
                    else
                    {
                        havingWagon = false;
                    }
                }
                else if(parentObject.tag == "WaterSpot" && nowGuideCustomer == null)
                {
                    if (!parentObject.GetComponent<WaterSpot>().nowUse)
                    {
                        touchWaterSpot = parentObject;
                        havingWater = true;
                    }
                    else
                    {
                        havingWater = false;
                    }
                }
                else if (parentObject.tag == "Table")
                {
                    if (!parentObject.GetComponent<Table>().usedTable && nowGuideCustomer != null)
                    {
                        if(parentObject.GetComponent<Table>().GetCustomerMaxCount() >= nowGuideCustomer.GetCustomerDetail().Count)
                        {
                            touchTable = parentObject;
                            sitdownCustomer = true;
                        }
                    }
                    else
                    {
                        sitdownCustomer = false;
                        if (parentObject.GetComponent<Table>().nowOrder && nowGuideCustomer == null)//�I�[�_�[
                        {
                            touchTable = parentObject;
                            orderTrue = true;
                        }
                        else
                        {
                            orderTrue = false;
                        }
                        if (parentObject.GetComponent<Table>().finishEat && parentObject.GetComponent<Table>().cleanpuTrue && nowGuideCustomer == null)//�Еt��
                        {
                            touchTable = parentObject;
                            cleanupTable = true;
                        }
                        else
                        {
                            cleanupTable = false;
                        }
                    }
                }
                else if(parentObject.tag == "CleaningTool" && nowGuideCustomer == null)
                {
                    if (parentObject.GetComponent<CleaningToolStand>().nowCleaningTool)
                    {
                        touchCleaningToolStand = parentObject;
                        havingOK = true;
                    }
                    else
                    {
                        touchCleaningToolStand = null;
                        havingOK = false;
                    }
                }
                else if (parentObject.tag == "FoodDrop" && nowGuideCustomer == null)
                {
                    foodDrop = false;
                }
                else if(parentObject.tag == "CustomerList" && nowGuideCustomer == null)
                {
                    if(!customerList.isOpenFlag)
                    {
                        guideCustomer = true;
                    }
                    else
                    {
                        guideCustomer = false;
                    }
                }
                else if (parentObject.tag == "CashRegister")
                {
                    if (parentObject.GetComponent<CashRegister>().nowCheck && !parentObject.GetComponent<CashRegister>().nowWorking && nowGuideCustomer == null)
                    {
                        workCashRegister = true;
                    }
                    else
                    {
                        workCashRegister = false;
                    }
                }
            }
            else if (nowHaving)//�����Ă���ꍇ
            {
                if (parentObject.tag == "FoodDrop" && havingPosition.childCount > 0)
                {
                    if(havingPosition.GetChild(0).gameObject.tag == "CleanupObject" || havingPosition.GetChild(0).gameObject.tag == "Water")
                    {
                        foodDrop = true;
                    }
                }
                else if (parentObject.tag == "Wagon" && nowGuideCustomer == null)
                {
                    if (havingBigFoodTableName != null)
                    {
                        putWagon = true;
                    }
                    else
                    {
                        putWagon = false;
                    }
                }
                else if (parentObject.tag == "Table")
                {
                    //��
                    //if (parentObject.GetComponent<Table>().finishEat && parentObject.GetComponent<Table>().cleanpuTrue && havingNormalFoodTableName == null && nowGuideCustomer == null)//�Еt��
                    //{
                    //    touchTable = parentObject;
                    //    cleanupTable = true;
                    //}
                    //else
                    //{
                    //    cleanupTable = false;
                    //}


                    if (havingNormalFoodTableName != null)
                    {
                        if (parentObject.GetComponent<Table>().FoodPutInformation(havingNormalFoodTableName))
                        {
                            touchTable = parentObject;
                            foodPut = true;
                        }
                    }
                    else if(parentObject.GetComponent<Table>().usedTable && !parentObject.GetComponent<Table>().finishEat && !parentObject.GetComponent<Table>().putWater && nowHavingCleaningTool == null)
                    {
                        touchTable = parentObject;
                        foodPut = true;
                        waterPut = true;
                    }
                    else
                    {
                        foodPut = false;
                        waterPut = false;
                    }
                }
                else if(parentObject.tag == "CleaningTool" && nowHavingCleaningTool != null)
                {
                    touchCleaningToolStand = parentObject;
                    cleaningToolPut = true;
                }
                else if (parentObject.tag == "CustomerList")
                {
                    guideCustomer = false;
                }
            }

            //�����ɑ|���p��������Ă��āA����ɐG�ꂽ��|������������

        }
        onCommandTriggerTrue.Invoke();
        ActionTriggerUpdate(true);
    }

    public void ActionTriggerUpdate(bool flag,int mode = 0)//�A�N�V�����̑I���A�I���I�t
    {
        if(mode == 0 || mode == 1)
        {
            if (flag && nowGuideCustomer == null && !nowJoint && !nowJump)
            {
                actionTrue1 = true;
            }
            else
            {
                actionTrue1 = false;
            }
        }
        if(mode == 0 || mode == 2)
        {
            if (flag && !nowHaving && otherPlayerTrigger.targetObject != null)
            {
                if (!otherPlayerTrigger.playerRPC.nowDown && !action2CoolTime)
                {
                    actionTrue2 = true;
                }
                else
                {
                    actionTrue2 = false;
                }
            }
            else
            {
                actionTrue2 = false;
            }
        }
        /*
        if (flag && !nowHaving)
        {
            if(otherPlayerTrigger.targetObject != null)
            {
                if (!otherPlayerTrigger.playerRPC.nowDown && !action1CoolTime)
                {
                    attackPlayer = true;
                }
                else
                {
                    attackPlayer = false;
                }
            }
            else
            {
                attackPlayer = false;
            }
        }
        else
        {
            attackPlayer = false;
        }
        */
        onActionTriggerUpdate.Invoke();
    }
    
    
    public void Action1CoolTimeSetting(bool flag)
    {
        action1CoolTime = flag;
        if (!action1CoolTime)
        {
            ActionTriggerUpdate(true,1);
        }
        else
        {
            ActionTriggerUpdate(false,1);
        }
    }
    public void Action2CoolTimeSetting(bool flag)
    {
        action2CoolTime = flag;
        if (!action2CoolTime)
        {
            ActionTriggerUpdate(true,2);
        }
        else
        {
            ActionTriggerUpdate(false,2);
        }
    }
    

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.layer != 8)
        {
            Debug.Log("���Z�b�g");
            if (!nowHaving)
            {
                havingOK = false;
            }
            touchStandName = null;//�X�^���h�̖��O�𑗂��ăo�O��Ȃ��悤�ɂ���
            touchNormalFoodTableName = null;
            touchBigFoodTableName = null;
            parentObject = null;
            touchTable = null;
            touchWaterSpot = null;
            touchCleaningToolStand = null;
            havingWagon = false;
            foodDrop = false;
            foodPut = false;
            havingWater = false;
            sitdownCustomer = false;
            orderTrue = false;
            cleaningToolPut = false;
            cleanupTable = false;
            workCashRegister = false;
            guideCustomer = false;
            //onTriggerTrue.Invoke();
            //onCommandTriggerFalse.Invoke();
            onCommandTriggerUpdate.Invoke();
        }
    }

    void LookAt()
    {
        if(parentObject != null)
        {
            Vector3 temp = parentObject.transform.position;
            temp.y = this.transform.position.y;
            this.transform.LookAt(temp);
        }
    }

    public void SetHinge(Rigidbody rigidbody)
    {
        if(rigidbody != null)
        {
            if (playerJoint == null)
            {
                playerJoint = this.gameObject.AddComponent<FixedJoint>();
                playerJoint.enablePreprocessing = false;
                myRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
            playerJoint.connectedBody = rigidbody;
            playerJoint.massScale = myRigidbody.mass;
            playerJoint.connectedMassScale = rigidbody.mass;
            //playerJoint.massScale = 2;
            //playerJoint.connectedMassScale = 2;
            nowJoint = true;
        }
        else
        {
            if(playerJoint != null)
            {
                Destroy(playerJoint);
            }

            myRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            nowJoint = false;
        }
    }

    //Action
    
    public void Action2()
    {
        /*
        nowJump = true;
        //onCommandTriggerFalse.Invoke();
        ActionTriggerUpdate(false);
        miniAudio?.SoundPlay(0, 0.5f);
        //�A�j���[�V�������Đ�
        //myAnimationController.AnimationPlay(PlayerAnimationController.AnimationName.jump);
        myRigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        StartCoroutine(Delay());
        IEnumerator Delay()
        {
            RaycastHit hit;
            yield return new WaitForSeconds(0.2f);
            while (true)
            {
                if (Mathf.Abs(myRigidbody.velocity.y) < 0.1f)
                {
                    Ray ray = new Ray(transform.position, Vector3.down);
                    //if (Physics.Raycast(transform.position, Vector3.down, 0.3f,out hit))
                    if (Physics.Raycast(ray, out hit,0.3f))
                    {
                        //�������W�����v
                        if(hit.collider.gameObject.tag == "Player" || hit.collider.gameObject.tag == "Customer")
                        {
                            miniAudio?.SoundPlay(0, 0.5f);
                            myRigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                            yield return new WaitForSeconds(0.2f);
                        }
                        else
                        {
                            if (hit.collider.gameObject.tag == "Table")
                            {
                                hit.collider.gameObject.GetComponent<Table>().EvaluationAnger();
                            }
                            nowJump = false;
                            ActionTriggerUpdate(true);
                            break;
                        }
                    }
                }
                yield return null;
            }
        }
        */
        //�O��
        if (otherPlayerTrigger.targetObject != null)
        {
            Vector3 force = otherPlayerTrigger.targetObject.transform.position - this.transform.position;
            //force += Vector3.up * force.magnitude;
            MyAttack();
            otherPlayerTrigger.targetObject.GetComponent<PlayerRPC>().OtherAttack(force, playerAttackPower);
        }
        
    }

    IEnumerator LandCheck()//���n����
    {
        RaycastHit hit;
        yield return new WaitForSeconds(0.2f);
        while (true)
        {
            if (Mathf.Abs(myRigidbody.velocity.y) < 0.1f)
            {
                Ray ray = new Ray(transform.position, Vector3.down);
                //if (Physics.Raycast(transform.position, Vector3.down, 0.3f,out hit))

                if (Physics.SphereCast(transform.position + Vector3.up * 0.55f, 0.5f, Vector3.down, out hit, 0.5f))
                {
                    Debug.Log(hit.collider.gameObject.tag);
                    //�������W�����v
                    if (hit.collider.gameObject.tag == "Player" || hit.collider.gameObject.tag == "Customer")
                    {
                        miniAudio?.SoundPlay(0, 0.5f);
                        myRigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                        landEffect?.Play();
                        yield return new WaitForSeconds(0.2f);
                    }
                    else
                    {
                        if (hit.collider.gameObject.tag == "Table")
                        {
                            hit.collider.gameObject.GetComponent<Table>().EvaluationAnger();
                        }
                        walkEffect?.Play();
                        landEffect?.Play();
                        nowJump = false;
                        ActionTriggerUpdate(true, 1);
                        break;
                    }
                    
                }
            }
            yield return null;
        }
    }

    void ChangeWarking(bool flag)
    {
        nowWarking = flag;
        if (flag)
        {
            myRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            myRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }
        

    }

    void SetDown(float time)
    {
        nowDown = true;
        onDown.Invoke();
        StartCoroutine(down());
        IEnumerator down()
        {
            yield return new WaitForSeconds(time);
            nowDown = false;
            onRecover.Invoke();
        }
    }

    //���S��������Ăяo��
    public void UnsetWagon()
    {
        putWagon = false;
        nowHaving = false;
        animationController.BringAnimationStop(PlayerAnimationController.BringAnimationName.bigCake);

        //�����\��
        onDestinationTextChange?.Invoke(null);
        havingBigFoodTableName = null;

        onCommandTriggerUpdate.Invoke();
    }

    //Family�p
    public void EvaluationGuideSetting(float goodFirstTime, float firstTime, float interval)
    {
        goodGuide2Time = goodFirstTime;
        angerGuide2FirstTime = firstTime;
        angerGuide2Interval = interval;
    }

    #region RPC
    //All

    public void SettingOK()
    {
        photonView.RPC(nameof(SettingOKRPC), RpcTarget.AllBufferedViaServer);
    }

    public void GameStart()
    {
        photonView.RPC(nameof(GameStartRPC), RpcTarget.AllViaServer, GameVariable.customerShuffleList.ToArray());
    }

    public void GameFinish(int nowEvaluation)
    {
        photonView.RPC(nameof(GameFinishRPC), RpcTarget.AllViaServer, nowEvaluation);
    }

    public void ChangePoint(int changePoint,bool RPC = false)
    {
        if (!RPC)
        {
            this.playerPoint += changePoint;
            Debug.Log($"PointChange {this.gameObject.GetComponent<PhotonView>().Owner.NickName} {playerPoint}");
        }
        else
        {
            photonView.RPC(nameof(ChangePointRPC), RpcTarget.AllViaServer, changePoint);
        }
    }

    public void ChangeEvaluationGauge(int setValue)
    {
        photonView.RPC(nameof(ChangeEvaluationGaugeRPC), RpcTarget.AllViaServer, setValue);
    }

    public void HelpSet()
    {
        photonView.RPC(nameof(HelpSetRPC), RpcTarget.AllViaServer);
    }

    //Action
    //������

    public void MyJump()
    {
        photonView.RPC(nameof(MyJumpRPC), RpcTarget.AllViaServer);
    }

    public void MyAttack()
    {
        photonView.RPC(nameof(MyAttackRPC), RpcTarget.AllViaServer);
    }

    public void MyRush()
    {
        photonView.RPC(nameof(MyRushRPC), RpcTarget.AllViaServer);
    }

    //���葤
    public void OtherAttack(Vector3 force,float power)
    {
        photonView.RPC(nameof(OtherAttackRPC), RpcTarget.AllViaServer,force,power);
    }

    public void OtherRush()
    {
        photonView.RPC(nameof(OtherRushRPC), RpcTarget.AllViaServer);
    }

    //Player
    public void Having()
    {
        if (touchNormalFoodTableName != null)
        {
            Debug.Log("HavingFood");
            photonView.RPC(nameof(HavingFoodRPC), RpcTarget.AllViaServer, touchNormalFoodTableName,touchStandName);
        }
        else if(touchCleaningToolStand != null)
        {
            Debug.Log("HavingCleaningTool");
            photonView.RPC(nameof(HavingCleaningToolRPC), RpcTarget.AllViaServer, touchCleaningToolStand.name);
        }
        //else if(havingWaterReady)
        //{
            //Debug.Log("HavingWater");
            //photonView.RPC(nameof(HavingWaterRPC), RpcTarget.AllViaServer);
        //}
    }

    public void HavingWagon()
    {
        LookAt();
        photonView.RPC(nameof(HavingWagonRPC), RpcTarget.AllViaServer, touchBigFoodTableName);
    }

    //Water
    public void HavingWater()
    {
        photonView.RPC(nameof(HavingWaterRPC), RpcTarget.AllViaServer, touchWaterSpot.name);
    }
    public void HavingWaterStart()
    {
        photonView.RPC(nameof(HavingWaterStartRPC), RpcTarget.AllViaServer, touchWaterSpot.name, PhotonNetwork.LocalPlayer);
    }
    public void HavingWaterStop()
    {
        photonView.RPC(nameof(HavingWaterStopRPC), RpcTarget.AllViaServer, touchWaterSpot.name, PhotonNetwork.LocalPlayer);
    }

    public void ObjectDestroy()
    {
        photonView.RPC(nameof(ObjectDestroyRPC), RpcTarget.AllViaServer);
    }

    public void FoodPut()
    {
        if(havingNormalFoodTableName != null)
        {
            photonView.RPC(nameof(FoodPutRPC), RpcTarget.AllViaServer, havingNormalFoodTableName, touchTable.name);
        }
        else if (waterPut)
        {
            photonView.RPC(nameof(WaterPutRPC), RpcTarget.AllViaServer, touchTable.name);
        }
    }

    public void PutWagon()
    {
        photonView.RPC(nameof(PutWagonRPC), RpcTarget.AllViaServer, havingBigFoodTableName);
    }

    public void PutWagonTable(string FoodName, string tableName)
    {
        photonView.RPC(nameof(PutWagonTableRPC), RpcTarget.AllViaServer, FoodName, tableName);
    }

    //public void WaterPut()
    //{
    //photonView.RPC(nameof(WaterPutRPC), RpcTarget.AllViaServer, touchTable.name);
    //}

    //Cleanup
    public void CleanupTable()
    {
        photonView.RPC(nameof(CleanupTableRPC), RpcTarget.AllViaServer, touchTable.name);
    }

    public void CleanupTableStart()
    {
        nowWorkingObject = GameVariable.tableList.Find(n => n.name == touchTable.name);
        photonView.RPC(nameof(CleanupTableStartRPC), RpcTarget.AllViaServer, touchTable.name, PhotonNetwork.LocalPlayer);
    }

    public void CleanupTableStop()
    {
        photonView.RPC(nameof(CleanupTableStopRPC), RpcTarget.AllViaServer, nowWorkingObject.name, PhotonNetwork.LocalPlayer);
    }


    //Customer
    public void AddCustomer(int customerNumber = -1)
    {
        //Debug.Log(customerNumber);
        photonView.RPC(nameof(AddCustomerRPC), RpcTarget.AllViaServer, customerNumber);
    }

    public void CustomerListOpen()
    {
        photonView.RPC(nameof(CustomerListOpenRPC), RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer);
    }

    public void CustomerListClose()
    {
        photonView.RPC(nameof(CustomerListCloseRPC), RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer);
    }

    public void GuideCustomer(int tapNumber, CustomerGroup customerGroup)
    {
        photonView.RPC(nameof(GuideCustomerRPC), RpcTarget.AllViaServer, tapNumber, customerGroup.GetCustomerNumber());
    }

    public void SitdownCustomer(int customerNumber)
    {
        photonView.RPC(nameof(SitdownCustomerRPC), RpcTarget.AllViaServer, touchTable.name, customerNumber);
    }

    public void GoHome(int customerNumber, Vector3[] GoHomeRoute)
    {
        photonView.RPC(nameof(GoHomeRPC), RpcTarget.AllViaServer, customerNumber, GoHomeRoute);
    }

    //Order
    public void OrderCreate()
    {
        orderTrue = false;
        photonView.RPC(nameof(OrderCreateRPC), RpcTarget.AllViaServer, touchTable.name);
    }

    public void OrderStart()
    {
        nowWorkingObject = GameVariable.tableList.Find(n => n.name == touchTable.name);
        photonView.RPC(nameof(OrderStartRPC), RpcTarget.AllViaServer, touchTable.name,PhotonNetwork.LocalPlayer);
    }

    public void OrderStop()
    {
        photonView.RPC(nameof(OrderStopRPC), RpcTarget.AllViaServer, nowWorkingObject.name, PhotonNetwork.LocalPlayer);
    }

    //Cleaning
    public void PutCleaningTool()
    {
        photonView.RPC(nameof(PutCleaningToolRPC), RpcTarget.AllViaServer, nowHavingCleaningTool.name);
    }

    //CashRegister

    public void CashRegisterWork()
    {
        photonView.RPC(nameof(CashRegisterWorkRPC), RpcTarget.AllViaServer);
    }

    public void CashRegisterWorkStart()
    {
        photonView.RPC(nameof(CashRegisterWorkStartRPC), RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer);
    }

    public void CashRegisterWorkStop()
    {
        photonView.RPC(nameof(CashRegisterWorkStopRPC), RpcTarget.AllViaServer, PhotonNetwork.LocalPlayer);
    }







    ////////////////////////////////////////////////////////////////////////////////////////////////////







    #region Game

    [PunRPC]
    void SettingOKRPC()
    {
        gameSettingOK = true;
    }

    [PunRPC]
    void GameStartRPC(int[] customerShuffleList)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            foreach(int temp in customerShuffleList)
            {
                GameVariable.customerShuffleList.Add(temp);
            }
        }
        onGameStart.Invoke();
        Debug.Log("GameStart");
    }

    [PunRPC]
    void GameFinishRPC(int nowEvaluation)
    {
        GameVariable.finalEvaluation = nowEvaluation;
        onGameFinish.Invoke();
        Debug.Log("GameFinish");
    }


    [PunRPC]
    void ChangePointRPC(int changePoint)
    {
        this.playerPoint += changePoint;
        Debug.Log($"PointChange {this.gameObject.GetComponent<PhotonView>().Owner.NickName} {playerPoint}");
        //�^�X�N�����������ō��
    }


    [PunRPC]
    void ChangeEvaluationGaugeRPC(int setValue)
    {
        evaluationGauge.ChangeGauge(setValue);
    }

    [PunRPC]
    void HelpSetRPC()
    {
        HelpMark();
        helpPanel.SetHelp(this.gameObject.GetComponent<PhotonView>().Owner);
    }

    #endregion

    #region Action

    [PunRPC]
    void MyJumpRPC()
    {
        nowJump = true;
        //onCommandTriggerFalse.Invoke();
        ActionTriggerUpdate(false,1);
        miniAudio?.SoundPlay(0, 0.5f);
        //�A�j���[�V�������Đ�
        //myAnimationController.AnimationPlay(PlayerAnimationController.AnimationName.jump);
        myRigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        StartCoroutine(LandCheck());
        
    }

    [PunRPC]
    void MyAttackRPC()
    {
        miniAudio?.SoundPlay(0, 0.5f);
        animationController.AnimationPlay(PlayerAnimationController.AnimationName.push);
        pushEffect?.Play();
        if (photonView.IsMine)
        {
            GameVariable.pushResult++;
        }
        ChangePoint(playerMinusPoint[PlayerMinusCategory.Attack]);
    }

    [PunRPC]
    void MyRushRPC()
    {
        //�A�j���[�V�������Đ�

    }

    [PunRPC]
    void OtherAttackRPC(Vector3 force,float power)
    {
        if (!nowWarking && nowGuideCustomer == null)
        {
            walkEffect?.Stop();
            onCommandTriggerFalse.Invoke();
            //�A�j���[�V�������Đ�
            Vector3 temp = force.normalized * 0.7f + Vector3.up * 1.5f;
            myRigidbody.AddForce(temp * power, ForceMode.Impulse);
            SetDown(downTime);
            StartCoroutine(LandCheck());
        }
        
    }

    [PunRPC]
    void OtherRushRPC()
    {
        //�A�j���[�V�������Đ�
        //onCommandTriggerFalse.Invoke();
        //SetDown(downTime);
    }

    #endregion

    #region Customer

    [PunRPC]
    void AddCustomerRPC(int customerNumber)
    {
        GameVariable.nowStandbyCustomerList.Add(customerNumber);
        CustomerGroup temp = GameVariable.customerGroupList.Find(n => n.GetCustomerNumber() == customerNumber);
        customerList.CustomerCreate(temp);
        //��ŏ���

    }

    [PunRPC]
    void CustomerListOpenRPC(Player player)
    {
        if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            if (customerList.isOpenFlag)
            {
                customerList.Close();
                //customerList.isOpenFlag = true;
                
            }
            else
            {
                customerList.isOpenFlag = true;
            }
            guideCustomer = false;
            //onTriggerTrue.Invoke();
            onCommandTriggerUpdate.Invoke();
        }
    }

    [PunRPC]
    void CustomerListCloseRPC(Player player)
    {
        if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            customerList.isOpenFlag = false;
            guideCustomer = customerList.isOpenFlag;
            onCommandTriggerUpdate.Invoke();
        }
        
        
    }

    [PunRPC]
    void GuideCustomerRPC(int tapNumber,int customerGroupNumber)
    {
        CustomerGroup guideCustomerGroup = GameVariable.customerGroupList.Find(n => n.GetCustomerNumber() == customerGroupNumber);
        nowGuideCustomer = guideCustomerGroup;
        customerList.CustomerUpdate(tapNumber);
        GameVariable.nowStandbyCustomerList.Remove(tapNumber);
        guideCustomer = false;
        //���q����̐e�𐶐����āAFamily�����Ĉē����J�n����
        GameObject familyObject = Instantiate(familyPrefab);
        familyObject.transform.position = customerSpawnPosition.position;
        this.gameObject.layer = 6;//���q����Ɠ�����Ȃ��悤�Ƀ��C���[�𕪂���
        familyObject.GetComponent<Family>().playerRPC = this;
        //familyObject.GetComponent<Family>().onGoodGuide2Add.AddListener(() => gameDirector.ChangeEvaluationGauge(gameDirector.goodGaugePoints[GameDirector.EvaluationGaugeValueType.Guide2]));
        //familyObject.GetComponent<Family>().onAngerGuide2Add.AddListener(() => gameDirector.ChangeEvaluationGauge(gameDirector.angerGaugePoints[GameDirector.EvaluationGaugeValueType.Guide2]));
        
        familyObject.GetComponent<Family>().EvaluationGuideSetting(goodGuide2Time,
            angerGuide2FirstTime,
            angerGuide2Interval);

        familyObject.GetComponent<Family>().CreateCustomer(guideCustomerGroup, this.gameObject);
        GameVariable.nowCustomerScriptList.Add(familyObject.GetComponent<Family>());

        onDestinationTextChange?.Invoke("�e�[�u���ֈē�����");

        onCommandTriggerUpdate.Invoke();
    }

    /// <summary>
    /// �q�����点��
    /// </summary>
    /// <param name="tableNumber">�e�[�u���ԍ�</param>
    /// <param name="customerNumber">�c�̊Ǘ��ԍ�</param>
    [PunRPC]
    void SitdownCustomerRPC(string tableNumber, int customerNumber)
    {
        LookAt();
        GameObject table = GameVariable.tableList.Find(n => n.name == tableNumber);

        if (!table.GetComponent<Table>().GetNowSitdownCustomer())
        {
            table.GetComponent<Table>().SitdownCustomers(customerNumber);
            nowGuideCustomer = null;
            sitdownCustomer = false;
            this.gameObject.layer = 9;
            //table.GetComponent<Table>().usedTable = true;
            //onTriggerTrue.Invoke();
            ChangePoint(playerPlusPoint[PlayerPlusCategory.CustomerGuide]);
            if (photonView.IsMine)
            {
                GameVariable.guideCustomerResult++;
            }
            TackCompleteMark();
            onDestinationTextChange?.Invoke(null);
        }
        onCommandTriggerUpdate.Invoke();
    }

    [PunRPC]
    void GoHomeRPC(int customerNumber,Vector3[] GoHomeRoute)
    {
        Family familyScript = GameVariable.nowCustomerScriptList.Find(n => n.gameObject.name == customerNumber.ToString());
        familyScript.GoHome(GoHomeRoute);
    }

    #endregion
    #region Food

    /// <summary>
    /// �����𐶐�
    /// </summary>
    /// <param name="tableNumber">�e�[�u���ԍ�</param>
    [PunRPC]
    void OrderCreateRPC(string tableNumber)
    {
        //GameObject table = GameVariable.tableList.Find(n => n.name == tableNumber);
        ChangeWarking(false);
        animationController.AnimationStop(PlayerAnimationController.AnimationName.order);
        nowWorkingObject.GetComponent<Table>().CreateOrder();
        if (photonView.IsMine)
        {
            GameVariable.orderResult++;
        }
        ChangePoint(playerPlusPoint[PlayerPlusCategory.Order]);
        TackCompleteMark();
        nowWorkingObject = null;
        //onTriggerTrue.Invoke();
        onCommandTriggerUpdate.Invoke();
    }

    [PunRPC]
    void OrderStartRPC(string tableNumber,Player player)
    {
        nowWorkingObject = nowWorkingObject == null ? GameVariable.tableList.Find(n => n.name == tableNumber): nowWorkingObject;
        if (nowWorkingObject.GetComponent<Table>().nowOrder)
        {
            LookAt();
            ChangeWarking(true);
            
            animationController.AnimationPlay(PlayerAnimationController.AnimationName.order);
            nowWorkingObject.GetComponent<Table>().EvaluationStop();
            if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                //GameObject table = GameVariable.tableList.Find(n => n.name == tableNumber);
                //nowWorkingObject = table;
                nowWorkingObject.GetComponent<Table>().nowOrder = false;
                orderTrue = false;
                //onTriggerTrue.Invoke();
                onCommandTriggerUpdate.Invoke();
            }
        }
        else
        {
            onCommandTriggerFalse.Invoke();
        }
    }

    [PunRPC]
    void OrderStopRPC(string tableNumber, Player player)
    {
        ChangeWarking(false);
        nowWorkingObject.GetComponent<Table>().EvaluationAnger();
        if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            //GameObject table = GameVariable.tableList.Find(n => n.name == tableNumber);
            nowWorkingObject.GetComponent<Table>().nowOrder = true;
            orderTrue = nowWorkingObject.GetComponent<Table>().nowOrder;
            //onTriggerTrue.Invoke();
            onCommandTriggerUpdate.Invoke();
        }
        animationController.AnimationStop(PlayerAnimationController.AnimationName.order);
        nowWorkingObject = null;
    }


    [PunRPC]
    void HavingFoodRPC(string foodName,string standName)
    {
        LookAt();
        GameObject foodObject = GameVariable.foodObjectList.Find(n => n.name == foodName);
        GameObject foodStand = GameVariable.foodStandList.Find(n => n.name == standName);
        
        if (!nowHaving && foodObject != null && foodStand != null)
        {
            if (foodStand.GetComponent<FoodStand>().FoodGetInformation() != null)
            {
                Debug.Log("����");
                foodStand.GetComponent<FoodStand>().GetFood();
                havingNormalFoodTableName = foodName;
                nowHaving = true;
                havingOK = false;
                havingScale = foodObject.transform.localScale;
                foodObject.transform.SetParent(havingPosition, true);
                foodObject.transform.localPosition = Vector3.zero;
                foodObject.transform.rotation = transform.rotation;
                animationController.BringAnimationPlay(PlayerAnimationController.BringAnimationName.normal);
                //onTriggerTrue.Invoke();

                //�����\��
                var temp = foodName.Split(",");

                onDestinationTextChange?.Invoke(temp[0] + "�ԃe�[�u���։^��");
            }
            /*
            if(null != foodStand.GetComponent<FoodStand>().FoodGetInformation())
            {
                Debug.Log("����");
                havingNormalFoodTableName = foodName;
                nowHaving = true;
                havingOK = false;
                havingScale = foodObject.transform.localScale;
                foodObject.transform.SetParent(havingPosition, true);
                foodObject.transform.localPosition = Vector3.zero;
                foodObject.transform.rotation = transform.rotation;
                //onTriggerTrue.Invoke();

                //�����\��
                var temp = foodName.Split(",");

                onDestinationTextChange?.Invoke(temp[0] + "�ԃe�[�u���։^��");


            }
            */
            onCommandTriggerUpdate.Invoke();
        }
    }

    [PunRPC]
    void HavingWagonRPC(string wagonName)
    {
        LookAt();
        GameObject wagonObject = GameVariable.foodObjectList.Find(n => n.name == wagonName);
        if (!nowHaving && wagonObject != null)
        {
            Debug.Log("����");
            havingBigFoodTableName = wagonName;
            nowHaving = true;
            havingWagon = false;
            wagonObject.GetComponent<Wagon>().SetJointPlayer(this);
            animationController.BringAnimationPlay(PlayerAnimationController.BringAnimationName.bigCake);
            //havingScale = wagonObject.transform.localScale;
            //wagonObject.transform.SetParent(havingPosition, true);
            //wagonObject.transform.localPosition = Vector3.zero;
            //wagonObject.transform.rotation = transform.rotation;
            //onTriggerTrue.Invoke();

            //�����\��
            var temp = wagonName.Split(",");

            onDestinationTextChange?.Invoke("���͂��āA" + temp[0] + "�ԃe�[�u���։^��");


            onCommandTriggerUpdate.Invoke();
        }
    }

    [PunRPC]
    void HavingWaterRPC(string waterSpotNumber)
    {
        if (!nowHaving)
        {
            //GameObject waterSpot = GameVariable.waterSpotList.Find(n => n.name == waterSpotNumber);
            nowWorkingObject.GetComponent<WaterSpot>().nowUse = false;
            Debug.Log("��������");
            GameObject waterObject = Instantiate(waterPrefab);//�񐄏������g��
            waterObject.transform.SetParent(havingPosition, true);
            waterObject.transform.localPosition = Vector3.zero;
            waterObject.transform.rotation = transform.rotation;
            animationController.BringAnimationPlay(PlayerAnimationController.BringAnimationName.normal);
            nowHaving = true;
            havingWater = false;
            nowWorkingObject = null;
            onCommandTriggerUpdate.Invoke();
        }
    }
    [PunRPC]
    void HavingWaterStartRPC(string waterSpotNumber, Player player)
    {
        LookAt();
        nowWorkingObject = GameVariable.waterSpotList.Find(n => n.name == waterSpotNumber);
        if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            //GameObject waterSpot = GameVariable.waterSpotList.Find(n => n.name == waterSpotNumber);
            
            //nowWorkingObject = waterSpot;
            nowWorkingObject.GetComponent<WaterSpot>().nowUse = true;
            onCommandTriggerUpdate.Invoke();
        }

    }

    [PunRPC]
    void HavingWaterStopRPC(string waterSpotNumber, Player player)
    {
        if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            //GameObject waterSpot = GameVariable.waterSpotList.Find(n => n.name == waterSpotNumber);
            nowWorkingObject.GetComponent<WaterSpot>().nowUse = false;
            
            onCommandTriggerUpdate.Invoke();
        }
        nowWorkingObject = null;
    }



    /// <summary>
    /// �����Ă��镨����������
    /// </summary>
    /// <param name="objectName">�t�[�h�̖��O</param>
    [PunRPC]
    void ObjectDestroyRPC()
    {
        LookAt();
        if (nowHaving && havingPosition.childCount > 0 )
        {
            onDestinationTextChange?.Invoke(null);
            animationController.BringAnimationStop(PlayerAnimationController.BringAnimationName.normal);
            foreach (Transform temp in havingPosition.transform)
            {
                if(temp.gameObject.tag == cleanupObjectPrefab.tag)
                {
                    ChangePoint(playerPlusPoint[PlayerPlusCategory.CleanupTable]);
                    TackCompleteMark();
                    Destroy(temp.gameObject);
                }
            }
            /*
            if(havingPosition.GetChild(0).gameObject.tag == cleanupObjectPrefab.tag)
            {
                ChangePoint(PlayerPlusPoint.CleanupTableAfter);
            }
            Destroy(havingPosition.GetChild(0).gameObject);
            */
        }

        nowHaving = false;
        onCommandTriggerUpdate.Invoke();
        /*
        GameObject foodObject = GameVariable.foodObjectList.Find(n => n.name == objectName);
        if(foodObject != null)
        {
            GameVariable.foodObjectList.Remove(foodObject);
            Destroy(foodObject);
            foodDrop = false;
            foodPut = false;
            nowHaving = false;
            //onTriggerTrue.Invoke();
            onTriggerUpdate.Invoke();
        }
        */
    }

    //�e�[�u��

    /// <summary>
    /// �����Ă���H�ו���u��
    /// </summary>
    /// <param name="foodName">�t�[�h�̖��O</param>
    /// <param name="tableName">�e�[�u���̖��O</param>
    [PunRPC]
    void FoodPutRPC(string foodName,string tableName)
    {
        LookAt();
        GameObject foodObject = GameVariable.foodObjectList.Find(n => n.name == foodName);
        GameObject tableObject = GameVariable.tableList.Find(n => n.name == tableName);
        if (foodObject != null)
        {
            //foodObject.transform.SetParent(tableObject.transform,true);
            //foodObject.transform.localPosition = tableObject.transform.GetChild(1).transform.localPosition + Vector3.up * ((foodObject.transform.localScale.y / 2) + (tableObject.transform.localScale.y / 2));
            //foodObject.transform.rotation = Quaternion.identity;
            //foodObject.transform.localScale = GameVariable.foodDataBase.GetFood(int.Parse(foodInformation[0])).GetFoodModel().transform.localScale;//���̃T�C�Y���Q��
            if (tableObject.GetComponent<Table>().SetFood())
            {
                animationController.BringAnimationStop(PlayerAnimationController.BringAnimationName.normal);
                GameVariable.foodObjectList.Remove(foodObject);
                Destroy(foodObject);
                foodDrop = false;
                foodPut = false;
                nowHaving = false;
                if (photonView.IsMine)
                {
                    GameVariable.carryfoodResult++;
                }
                ChangePoint(playerPlusPoint[PlayerPlusCategory.PutFood]);
                TackCompleteMark();
                //onTriggerTrue.Invoke();

                //�����\��
                onDestinationTextChange?.Invoke(null);
                havingNormalFoodTableName = null;

                onCommandTriggerUpdate.Invoke();
            }
        }
    }

    /// <summary>
    /// ���̏�Ƀ��S����u��
    /// </summary>
    /// <param name="wagonName"></param>
    [PunRPC]
    void PutWagonRPC(string wagonName)
    {
        LookAt();
        GameObject wagonObject = GameVariable.foodObjectList.Find(n => n.name == wagonName);
        if (wagonObject != null)
        {
            animationController.BringAnimationStop(PlayerAnimationController.BringAnimationName.bigCake);
            wagonObject.GetComponent<Wagon>().UnsetJointPlayer(this);
            foodDrop = false;
            putWagon = false;
            nowHaving = false;
            //onTriggerTrue.Invoke();

            onDestinationTextChange?.Invoke(null);
            havingBigFoodTableName = null;

            onCommandTriggerUpdate.Invoke();
        }
    }

    /// <summary>
    /// �e�[�u���Ƀ��S����u��
    /// </summary>
    /// <param name="wagonName"></param>
    [PunRPC]
    void PutWagonTableRPC(string wagonName,string tableName)
    {
        LookAt();
        GameObject wagonObject = GameVariable.foodObjectList.Find(n => n.name == wagonName);
        GameObject tableObject = GameVariable.tableList.Find(n => n.name == tableName);
        if (wagonObject != null)
        {
            StartCoroutine(Delay());
            IEnumerator Delay()
            {
                //yield return new WaitUntil(() => wagonObject.GetComponent<Wagon>().Complete());
                yield return new WaitUntil(() => tableObject.GetComponent<Table>().SetFood());
                yield return wagonObject.GetComponent<Wagon>().Complete();

                GameVariable.foodObjectList.Remove(wagonObject);
                Destroy(wagonObject);
                //onTriggerTrue.Invoke();

                if (photonView.IsMine)
                {
                    GameVariable.carryfoodResult++;
                }
                onDestinationTextChange?.Invoke(null);
                havingBigFoodTableName = null;

                onCommandTriggerUpdate.Invoke();
            }
        }
    }


    [PunRPC]
    void WaterPutRPC(string tableName)
    {
        LookAt();
        animationController.BringAnimationStop(PlayerAnimationController.BringAnimationName.normal);
        GameObject tableObject = GameVariable.tableList.Find(n => n.name == tableName);
        tableObject.GetComponent<Table>().SetWater();
        if (nowHaving && havingPosition.childCount > 0)
        {
            Destroy(havingPosition.GetChild(0).gameObject);
        }
        nowHaving = false;
        foodPut = false;
        waterPut = false;
        ChangePoint(playerPlusPoint[PlayerPlusCategory.PutWater]);
        TackCompleteMark();
        onCommandTriggerUpdate.Invoke();

    }

    [PunRPC]
    void CleanupTableRPC(string tableName)
    {
        //GameObject tableObject = GameVariable.tableList.Find(n => n.name == tableName);
        //if (!nowHaving)
        //{
        Debug.Log("�Еt�� ����");
        animationController.AnimationStop(PlayerAnimationController.AnimationName.bussing);
        animationController.BringAnimationPlay(PlayerAnimationController.BringAnimationName.normal);
        nowHaving = true;
        cleanupTable = false;
        if (havingPosition.childCount <= 0)
        {
            onDestinationTextChange?.Invoke("�ԋp���Ɏ����Ă���");
        }
        ChangeWarking(false);
        nowWorkingObject.GetComponent<Table>().CleanupTable();
        GameObject cleanObject = Instantiate(cleanupObjectPrefab);//�񐄏������g��
        cleanObject.tag = "CleanupObject";
        cleanObject.transform.SetParent(havingPosition, true);
        cleanObject.transform.localPosition = Vector3.zero;
        cleanObject.transform.rotation = transform.rotation;
        cleanObject.transform.localScale = cleanupObjectPrefab.transform.localScale / 2;
        if (photonView.IsMine)
        {
            GameVariable.cleanupTableResult++;
        }
        nowWorkingObject = null;
        onCommandTriggerUpdate.Invoke();
        //}

    }

    [PunRPC]
    void CleanupTableStartRPC(string tableNumber, Player player)
    {
        LookAt();

        ChangeWarking(true);
        nowWorkingObject = nowWorkingObject == null ? GameVariable.tableList.Find(n => n.name == tableNumber) : nowWorkingObject;
        animationController.AnimationPlay(PlayerAnimationController.AnimationName.bussing);
        if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            //GameObject table = GameVariable.tableList.Find(n => n.name == tableNumber);
            //nowWorkingObject = table;
            nowWorkingObject.GetComponent<Table>().cleanpuTrue = false;
            onCommandTriggerUpdate.Invoke();
        }

    }

    [PunRPC]
    void CleanupTableStopRPC(string tableNumber, Player player)
    {
        if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            //GameObject table = GameVariable.tableList.Find(n => n.name == tableNumber);
            nowWorkingObject.GetComponent<Table>().cleanpuTrue = true;
            
            onCommandTriggerUpdate.Invoke();
        }
        ChangeWarking(false);
        animationController.AnimationStop(PlayerAnimationController.AnimationName.bussing);
        nowWorkingObject = null;
    }

    #endregion
    #region CleaningTool
    [PunRPC]
    void HavingCleaningToolRPC(string toolStandName)
    {
        LookAt();
        GameObject toolObject = GameVariable.cleaningToolList.Find(n => n.name == toolStandName);
        if (!nowHaving && toolStandName != null)
        {
            GameObject cleaningTool = toolObject.GetComponent<CleaningToolStand>().GetCleaningTool();
            if(cleaningTool != null)
            {
                Debug.Log("����");
                nowHaving = true;
                havingOK = false;
                havingScale = cleaningTool.transform.localScale;
                cleaningTool.transform.SetParent(havingPosition, true);
                cleaningTool.transform.localPosition = Vector3.zero;
                cleaningTool.transform.rotation = transform.rotation;
                nowHavingCleaningTool = cleaningTool;
                onCommandTriggerUpdate.Invoke();
            }
        }
    }

    [PunRPC]
    void PutCleaningToolRPC(string toolName)
    {
        LookAt();
        GameObject toolObject = GameVariable.cleaningToolList.Find(n => n.name == toolName);
        if (nowHaving && toolName != null)
        {
            CleaningToolStand toolObjectScript = toolObject.GetComponent<CleaningToolStand>();
            if (!toolObjectScript.nowCleaningTool)
            {
                Debug.Log("�u��");
                toolObjectScript.SetCleaningTool(nowHavingCleaningTool);
                nowHaving = false;
                havingOK = true;
                nowHavingCleaningTool = null;
                onCommandTriggerUpdate.Invoke();
            }
        }
    }
    #endregion
    #region CashRegister

    [PunRPC]
    void CashRegisterWorkRPC()
    {
        animationController.AnimationStop(PlayerAnimationController.AnimationName.register,false);
        //Invoke(nameof(CashRegisterDelay), 1.45f);
        CashRegisterDelay();
    }

    void CashRegisterDelay()
    {
        GameVariable.cashRegister.CompleteCash();
        ChangeWarking(false);
        if (photonView.IsMine)
        {
            GameVariable.cashResult++;
        }
        ChangePoint(playerPlusPoint[PlayerPlusCategory.CashRegister]);
        TackCompleteMark();
        onCommandTriggerUpdate.Invoke();
    }

    [PunRPC]
    void CashRegisterWorkStartRPC(Player player)
    {
        LookAt();
        animationController.AnimationPlay(PlayerAnimationController.AnimationName.register);
        GameVariable.cashRegister.EvaluationStop();
        ChangeWarking(true);
        if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            GameVariable.cashRegister.nowWorking = true;
            workCashRegister = false;
            onCommandTriggerUpdate.Invoke();
        }

    }

    [PunRPC]
    void CashRegisterWorkStopRPC(Player player)
    {
        animationController.AnimationStop(PlayerAnimationController.AnimationName.register,false);
        GameVariable.cashRegister.EvaluationAnger();
        ChangeWarking(false);
        if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            GameVariable.cashRegister.nowWorking = false;
            onCommandTriggerUpdate.Invoke();
        }
    }

    #endregion

    #endregion
}
