using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPunCallbacks
{
    //////////////////////////////////////////////////////////////

    //パラメーター
    //移動系
    float walkingSpeed = 6.0f;//歩き速度
    float dashPlusSpeed = 2.0f;//ダッシュスペードの追加分

    //時間系(プレイヤー)
    float attackCoolTime = 5f;//殴った後のクールタイム
    float stealCoolTime = 5f;//奪った後のクールタイム
    float rushCoolTime = 10f;//突進した後のクールタイム

    //時間系(長押し系)
    float orderTime = 2;//オーダー受注にかかる時間
    float cleanupTime = 2f;//片付けにかかる時間
    float waterTime = 2f;//水汲みにかかる時間
    float cashRegister = 2f;//会計にかかる時間

    

    //カメラの場所
    Vector3 playerCameraPosition = new Vector3(-15f,19.5f, -15f);////プレイヤーカメラのプレイヤーからの距離 斜めからの場合

    //入力系
    float gamepadStickDeadzone = 0.2f;//スティックの閾値
    float reinputInterval = 0.5f;//再入力が可能になる時間

    //////////////////////////////////////////////////////////////
    //Setting(Director経由で代入)
    [System.NonSerialized] public GameDirector gameDirector;
    [System.NonSerialized] public GameObject targetObject;
    [System.NonSerialized] public PlayerRPC targetObjectScript;//RPC
    [System.NonSerialized] public Menu menu;
    [System.NonSerialized] public CustomerList customerList;
    [System.NonSerialized] public ProgressBar progressBar;//進捗バー
    [System.NonSerialized] public Camera playerCamera;

    [System.NonSerialized] public UnityEvent onDown = new UnityEvent();//ダウンしたとき

    [System.NonSerialized] public GameObject dontTapPanel;//Directorから入れる

    ///////////////////
    //操作方法
    //New
    //[SerializeField] InputActionAsset inputActionAsset;//InputActionAsset
    GameActions gameActions; //ラッパークラスの作成必須
    GameActions.PlayerActions playerActions = new GameActions.PlayerActions();
    GameActions.UIActions uiActions = new GameActions.UIActions();

    //[SerializeField] Gamepad nowGamepad;//現在のゲームパッド

    //Action
    //コマンド
    Action commandButtonEnterAction = null;//単押し
    Action commandButtonDownAction = null;//長押し(押した瞬間)
    Action commandButtonUpAction = null;//長押し(離した瞬間)
    //攻撃
    Action actionButton1Action = null;
    Action actionButton2Action = null;

    enum InputPossibleDevice//入力可能なデバイス
    {
        Null,
        Gamepad,
        Keyboard,
        TouchScreen
    };

    enum ButtonMode//特定のボタンの入力方法
    {
        Null,
        Press,
        LongPress
    }

    public enum InputScene//現在の入力場面
    {
        Null,
        Game,
        CustomerList,
        Menu
    };

    //現在の入力方法
    InputPossibleDevice nowInputPossibleDevice = InputPossibleDevice.Null;
    //コマンドボタンの入力方法
    ButtonMode commandButtonMode = ButtonMode.Null;
    //現在入力している場面
    [System.NonSerialized] public InputScene nowSelectScene = InputScene.Null;

    bool nowCommandPressInterval = false;//コマンド入力がされた後、インターバルの最中か
    bool nowAction1PressInterval = false;//アクション1
    bool nowAction2PressInterval = false;//アクション2
    bool nowHelpPressInterval = false;//ヘルプ

    ///////////////////

    //操作
    [SerializeField] GameObject tapArea;//移動操作に割り当てる範囲
    [SerializeField] GameObject startTapPoint;//最初にタップしたポイント
    [SerializeField] GameObject nowTapPoint;//現在タップしているポイント
    [SerializeField] ButtonManager buttonManager; 
    [SerializeField] Button menuButton;//メニューボタン
    [SerializeField] GameObject dashButton;//走るボタン
    //[SerializeField] GameObject enterButton;//決定ボタン
    //[SerializeField] GameObject actionButton1;//アクションボタン1
    //[SerializeField] GameObject helpButton;//ヘルプボタン
    //[SerializeField] Text HavingText;//何を持っているかテキスト
    //表示
    [SerializeField] RectTransform destinationPanel;//行き先パネル
    [SerializeField] Text destinationText;//行き先用テキスト

    bool settingOK = false;//プレイヤーの準備ができたか
    public bool moveControlTrue = false;//プレイヤーを動かせるか
    public bool buttonControlTrue = false;//ボタンをタップできるか

    float pushButtonTime = 0f;//長押ししている時間
    float pushButtonTimeLimit = 0f;//長押しなければならない時間(都度変更)
    bool nowEnterButton = false;//現在押しているか
    int buttonMode = 0;//現在の長押しボタンモード

    //Move
    Transform transform;
    Rigidbody rigidbody;
    bool moveFlag = false;
    public float speed;//速度
    public bool move, rotation;//お客さんから参照
    private Vector2 startPos, currentPos, differenceDisVector2;
    [SerializeField] private float radian, differenceDisFloat;

    //移動
    bool dashFlag = false;

    float movePanelRadius;
    float movePointRadius;

    //タップ関係
    bool screenTap = false;//現在タップしているか(Mouse)
    bool nowTap = false;//現在タップしているか(Tap)
    Touch moveTouch;//移動に使うtouch
    Vector3[] tapAreaCorner = new Vector3[4];
    int touchCount;

    //パネル関係
    float destinationMoveSpeed = 0.3f;
    float destinationPanelOpenPosition = 0;
    float destinationPanelClosePosition = 0;

    public void TargetSetting()
    {
        targetObjectScript = targetObject.GetComponent<PlayerRPC>();
        progressBar = targetObject.GetComponent<ProgressBar>();
        transform = targetObject.GetComponent<Transform>();
        rigidbody = targetObject.GetComponent<Rigidbody>();

        targetObjectScript.onDown.AddListener(() => PlayerDown());
        targetObjectScript.onRecover.AddListener(() => PlayerRecover());
        buttonManager.action1Object.GetComponent<ButtonCoolTime>().onStartCoolTime.AddListener(() => targetObjectScript.Action1CoolTimeSetting(true));
        buttonManager.action1Object.GetComponent<ButtonCoolTime>().onFinishCoolTime.AddListener(() => targetObjectScript.Action1CoolTimeSetting(false));
        buttonManager.action2Object.GetComponent<ButtonCoolTime>().onStartCoolTime.AddListener(() => targetObjectScript.Action2CoolTimeSetting(true));
        buttonManager.action2Object.GetComponent<ButtonCoolTime>().onFinishCoolTime.AddListener(() => targetObjectScript.Action2CoolTimeSetting(false));

        //消去予定
        targetObjectScript.onDestinationTextChange += ChangeDestinationText;


        settingOK = true;
    }

    #region InputSetting

    void ChangeInputDevice(InputDevice device, InputDeviceChange change)//コールバック用
    {
        switch (change)
        {
            case InputDeviceChange.Added:
            case InputDeviceChange.Removed:
            case InputDeviceChange.Disconnected:
                InputSetting();
                break;
        }
    }

    void InputSetting()//入力方法の検知&変更
    {
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                if (Gamepad.current != null)
                {
                    Debug.Log("ゲームパッドを有効化");
                    ChangeInputSetting(InputPossibleDevice.Gamepad);
                }
                else if (Touchscreen.current != null)
                {
                    Debug.Log("タッチスクリーンを有効化");
                    ChangeInputSetting(InputPossibleDevice.TouchScreen);
                }
                else
                {
                    Debug.Log("対応しているデバイスがないため操作ができません");
                    ChangeInputSetting(InputPossibleDevice.Null);
                }
                break;
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                if (Gamepad.current != null)
                {
                    Debug.Log("ゲームパッドを有効化");
                    ChangeInputSetting(InputPossibleDevice.Gamepad);
                }
                else if (Keyboard.current != null)
                {
                    Debug.Log("キーボードを有効化");
                    ChangeInputSetting(InputPossibleDevice.Keyboard);
                }
                else
                {
                    Debug.Log("対応しているデバイスがないため操作ができません");
                    ChangeInputSetting(InputPossibleDevice.Null);
                }
                break;
        }
    }

    void ChangeInputSetting(InputPossibleDevice inputPossibleDevice)
    {
        if(nowInputPossibleDevice != inputPossibleDevice)
        {
            switch (inputPossibleDevice)
            {
                case InputPossibleDevice.Gamepad:
                    nowInputPossibleDevice = InputPossibleDevice.Gamepad;
                    break;
                case InputPossibleDevice.TouchScreen:
                    nowInputPossibleDevice = InputPossibleDevice.TouchScreen;
                    break;
                case InputPossibleDevice.Keyboard:
                    nowInputPossibleDevice = InputPossibleDevice.Keyboard;
                    break;
                case InputPossibleDevice.Null:
                    nowInputPossibleDevice = InputPossibleDevice.Null;
                    break;
            }
        }
    }

    #endregion

    void Start()
    {
        gameActions = new GameActions();
        gameActions?.Enable();//Actionを有効化
        playerActions = gameActions.Player;
        uiActions = gameActions.UI;

        InputSystem.onDeviceChange += ChangeInputDevice;
        InputSetting();

        //menu.onMenuOpen.AddListener(() => nowSelectScene = InputScene.Menu);
        //menu.onMenuClose.AddListener(() => nowSelectScene = InputScene.Game);
        customerList.onCustomerListOpen.AddListener(() => CustomerListOpen());
        customerList.onCustomerListClose.AddListener(() => CustomerListClose());

        progressBar.BarActive(false);

        //行き先パネル設定
        destinationPanelOpenPosition = destinationPanel.anchoredPosition.y;
        destinationPanelClosePosition = destinationPanel.rect.height;
        destinationText.text = "";
        destinationPanel.DOAnchorPosY(destinationPanelClosePosition, 0);

        //入力系
        pushButtonTime = 0f;
        //角「左下，左上，右上，右下」
        tapArea.GetComponent<RectTransform>().GetWorldCorners(tapAreaCorner);
        speed = 0;
        startTapPoint.SetActive(false);
        nowTapPoint.SetActive(false);
        movePointRadius = startTapPoint.GetComponent<CircleCollider2D>().radius - nowTapPoint.GetComponent<CircleCollider2D>().radius;//スライドパッド用
        startTapPoint.GetComponent<CircleCollider2D>().enabled = false;
        nowTapPoint.GetComponent<CircleCollider2D>().enabled = false;

        Invoke(nameof(DelayStart), 0.1f);

    }

    void DelayStart()
    {
        buttonManager.EnterButtonOff();
        buttonManager.onActionButtonUpdate.AddListener(() => ActionTrigger());

        //actionButton1.GetComponent<Button>().enabled = false;
        //actionButton1.GetComponent<Image>().color = buttonEnableOff;

        if (buttonManager.helpButton != null)
        {
            buttonManager.helpButton.onClick.AddListener(() => HelpPress());//ヘルプボタン設定
        }

        buttonManager.HelpButtonOn();
    }

    void Update()
    {
        if (settingOK)
        {
            if (!targetObjectScript.nowDown && moveControlTrue && gameDirector.gameStarted && GameVariable.nowApplicationFocus && nowSelectScene == InputScene.Game)//前面の時だけ操作可能
            {
                //ゲームパッド&キーボード
                if(nowInputPossibleDevice == InputPossibleDevice.Gamepad || nowInputPossibleDevice == InputPossibleDevice.Keyboard)
                {
                    MovementActionsControll();
                }
                else if (nowInputPossibleDevice == InputPossibleDevice.TouchScreen)//タッチ操作
                {
                    MovementTouchScreenControll();
                }
                
            }
            if (!targetObjectScript.nowDown && buttonControlTrue && gameDirector.gameStarted && GameVariable.nowApplicationFocus)//前面の時だけ操作可能
            {
                ButtonControl();

                if (nowEnterButton)
                {
                    pushButtonTime += Time.deltaTime;
                    progressBar.SetBar(pushButtonTime, pushButtonTimeLimit);
                    if (pushButtonTime >= pushButtonTimeLimit)
                    {
                        ButtonExit();
                    }
                }
            }
            //if (moveControlTrue && gameDirector.gameStarted)
            //{
            //    Movement();
            //}
        }

    }
    void LateUpdate()
    {
        if (settingOK)
        {
            if (playerCamera != null && transform != null)
            {
                //playerCamera.transform.position = transform.position + playerCameraPosition;
                playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, transform.position + playerCameraPosition, 5.0f * Time.deltaTime);
            }
            if (moveControlTrue && gameDirector.gameStarted)
            {
                Movement();
            }
        }    
    }

    //void FixedUpdate()
    //{
    //    if (settingOK)
    //    {
    //        
    //    }
    //}

    public void ControllableChange(bool changeMoveStatus,bool changeButtonStatus = true)
    {
        if (changeMoveStatus)
        {
            moveControlTrue = true;
            if (changeButtonStatus)
            {
                buttonControlTrue = true;
            }
        }
        else
        {
            MoveStop();
            moveControlTrue = false;
            if (changeButtonStatus)
            {
                buttonControlTrue = false;
            }
        }
    }

    bool FloatBetween(float target,float betA,float betB )
    {
        if(betA > betB)
        {
            return (target <= betA && target >= betB);
        }
        else
        {
            return (target <= betB && target >= betA);
        }
    }


    void PlayerDown()
    {
        onDown.Invoke();
        dontTapPanel.SetActive(true);
        CommandTriggerFalse();
        buttonManager.Action1ButtonOff();
        ControllableChange(false);
    }

    void PlayerRecover()
    {
        dontTapPanel.SetActive(false);
        CommandTriggerUpdate();
        ControllableChange(true);
    }


    void ChangeDestinationText(string text = null)
    {
        if(text == null || text == "")
        {
            destinationPanel.DOAnchorPosY(destinationPanelClosePosition, destinationMoveSpeed).
                SetEase(Ease.OutCubic).OnComplete(() => destinationText.text = "");
        }
        else
        {
            destinationText.text = text;
            destinationPanel.DOAnchorPosY(destinationPanelOpenPosition, destinationMoveSpeed).
                SetEase(Ease.OutCubic);
        }
        
    }


    public void MenuButton()
    {
        //MoveStop();
        //menu.MenuButton();
    }


    #region Move

    void MovementActionsControll()//ゲームパッド
    {
        if (playerActions.Move.ReadValue<Vector2>().magnitude > gamepadStickDeadzone)
        {
            if (!moveFlag)
            {
                moveFlag = true;
                startPos = Vector2.zero;
            }
            if (moveFlag)
            {
                currentPos = playerActions.Move.ReadValue<Vector2>();
                differenceDisVector2 = currentPos - startPos;//2点間の距離(x,y)
                //スワイプ量によってSpeedを変化させる.この時、絶対値にする。
                differenceDisFloat = Mathf.Abs(differenceDisVector2.x) + Mathf.Abs(differenceDisVector2.y);
                float tempX = differenceDisVector2.normalized.x * Mathf.Clamp(differenceDisFloat, -movePointRadius, movePointRadius);
                float tempY = differenceDisVector2.normalized.y * Mathf.Clamp(differenceDisFloat, -movePointRadius, movePointRadius);
                move = true;
                //回転する角度計算
                radian = (Mathf.Atan2(differenceDisVector2.x, differenceDisVector2.y) * Mathf.Rad2Deg) + playerCamera.transform.localRotation.eulerAngles.y;
                speed = walkingSpeed;
            }
        }
        else if (moveFlag)
        {
            move = false;
            rigidbody.velocity = Vector3.zero;
            MoveStop();
        }
    }


    void MovementTouchScreenControll()//タッチスクリーン
    {
        touchCount = Input.touchCount;
        if (touchCount > 0)
        {
            if (!nowTap)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (FloatBetween(touch.position.x, tapAreaCorner[1].x, tapAreaCorner[2].x) &&
                        FloatBetween(touch.position.y, tapAreaCorner[2].y, tapAreaCorner[3].y))
                    {
                        moveTouch = touch;
                        break;
                    }
                }
            }
            else
            {
                foreach (Touch touch in Input.touches)
                {
                    if (touch.fingerId == moveTouch.fingerId)
                    {
                        moveTouch = touch;//同じ指だったら更新
                    }
                }
            }


            if (moveTouch.phase == UnityEngine.TouchPhase.Began)
            {
                nowTap = true;
                TapDown(moveTouch.position);
            }
            if (moveTouch.phase == UnityEngine.TouchPhase.Moved)
            {
                TapStay(moveTouch.position);
            }
            if (moveTouch.phase == UnityEngine.TouchPhase.Ended)
            {
                nowTap = false;
                MoveStop();
            }
        }
    }
    /*
    void MovementKeyboardMouseControll()//マウス
    {
        
        //移動
        if (Input.GetMouseButtonDown(0) && screenTap && touchCount <= 1)
        {
            TapDown(Input.mousePosition);
        }

        if (Input.GetMouseButton(0) && screenTap && touchCount <= 1)
        {
            TapStay(Input.mousePosition);
        }
        else if(touchCount > 1)
        {
            MoveStop();
        }

        if (Input.GetMouseButtonUp(0))
        {
            MoveStop();
        }
        
    }

    */

    void TapDown(Vector2 tapPosition)
    {
        if (gameDirector.gameStarted)
        {
            moveFlag = true;
            startPos = new Vector2(tapPosition.x, tapPosition.y);
            startTapPoint.transform.position = startPos;
            nowTapPoint.transform.localPosition = Vector3.zero;
            startTapPoint.SetActive(true);
            nowTapPoint.SetActive(true);
        }
        
    }

    void TapStay(Vector2 tapPosition)
    {
        if (moveFlag && gameDirector.gameStarted)
        {
            startTapPoint.transform.position = startPos;
            //押している最中に今の座標を代入
            currentPos = new Vector2(tapPosition.x, tapPosition.y);
            differenceDisVector2 = currentPos - startPos;//2点間の距離(x,y)

            //スワイプ量によってSpeedを変化させる.この時、絶対値にする。
            differenceDisFloat = Mathf.Abs(differenceDisVector2.x) + Mathf.Abs(differenceDisVector2.y);

            float tempX = differenceDisVector2.normalized.x * Mathf.Clamp(differenceDisFloat, -movePointRadius, movePointRadius);
            float tempY = differenceDisVector2.normalized.y * Mathf.Clamp(differenceDisFloat, -movePointRadius, movePointRadius);
            nowTapPoint.transform.localPosition = new Vector2(tempX, tempY);

            if (differenceDisFloat > 10)
            {

                move = true;
                //回転する角度計算
                radian = (Mathf.Atan2(differenceDisVector2.x, differenceDisVector2.y) * Mathf.Rad2Deg) + playerCamera.transform.localRotation.eulerAngles.y;

                speed = walkingSpeed;
            }
            else
            {
                move = false;
                rigidbody.velocity = Vector3.zero;
            }
        }
    }

    void MoveStop()
    {
        moveFlag = false;
        if (startTapPoint.activeSelf || nowTapPoint.activeSelf)
        {
            startTapPoint.SetActive(false);
            nowTapPoint.SetActive(false);
        }
        speed = 0;
        move = false;
        rigidbody.transform.DOKill();
        if (rigidbody != null)//ゲーム終了時にバグる可能性があるため
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
    }

    void Movement()//移動
    {
        if (move == true && transform != null)
        {
            Vector3 move;
            if (!targetObjectScript.nowJoint)
            {
                //Vector3 move;
                if (targetObjectScript.nowJump)
                {
                    move = transform.forward * speed/2;
                }
                else if (targetObjectScript.nowHaving)
                {
                    move = transform.forward * speed * 0.75f;
                }
                else
                {
                    move = transform.forward * speed;
                }
                rigidbody.transform.DORotateQuaternion(Quaternion.Euler(0, radian, 0), 0.3f);
                //rigidbody.velocity = new Vector3(move.x, rigidbody.velocity.y, move.z);//y成分はそのまま
            }
            else//ジョイントされてたらこっち
            {
                move = new Vector3(differenceDisVector2.x, 0, differenceDisVector2.y).normalized * speed;
                
            }
            rigidbody.velocity = new Vector3(move.x, rigidbody.velocity.y, move.z);//y成分はそのまま
        }
    }
    #endregion

    //一定時間入力出来ないようにする
    public void PressInterval(int mode)
    {
        StartCoroutine(PressDelay(mode));
    }
    
    IEnumerator PressDelay(int mode)
    {
        switch (mode)
        {
            case 1:
                nowCommandPressInterval = true;
                yield return new WaitForSeconds(reinputInterval);
                nowCommandPressInterval = false;
                break;
            case 2:
                nowAction1PressInterval = true;
                yield return new WaitForSeconds(reinputInterval);
                nowAction1PressInterval = false;
                break;
            case 3:
                nowAction2PressInterval = true;
                yield return new WaitForSeconds(reinputInterval);
                nowAction2PressInterval = false;
                break;
            case 4:
                nowHelpPressInterval = true;
                yield return new WaitForSeconds(reinputInterval);
                nowHelpPressInterval = false;
                break;

        }
    }

    //ボタンコントロール
    void ButtonControl()
    {
        switch (nowSelectScene)
        {
            case InputScene.Game:
                //メニュー
                if (playerActions.Menu.WasPressedThisFrame())
                {
                    //MenuButton();
                }

                //コマンド
                if (playerActions.Command.WasPressedThisFrame())
                {
                    if (commandButtonMode == ButtonMode.Press)
                    {
                        commandPress();
                    }
                    else if (commandButtonMode == ButtonMode.LongPress)
                    {
                        CommandDown();
                    }
                }
                if (playerActions.Command.WasReleasedThisFrame())
                {
                    if (commandButtonMode == ButtonMode.LongPress)
                    {
                        CommandUp();
                    }
                }

                //攻撃
                if (playerActions.Action1.WasPressedThisFrame())
                {
                    //Action1Press();
                }
                if (playerActions.Action2.WasPressedThisFrame())
                {
                    Action2Press();
                }
                if (playerActions.Help.WasPressedThisFrame())
                {
                    HelpPress();
                }

                break;
            case InputScene.CustomerList:
                if (uiActions.Up.WasPressedThisFrame())
                {
                    customerList.UpDown(true);
                }
                if (uiActions.Down.WasPressedThisFrame())
                {
                    customerList.UpDown(false);
                }
                if (uiActions.Enter.WasPressedThisFrame())
                {
                    customerList.ControllerGuideOK();
                }
                if (uiActions.Back.WasPressedThisFrame())
                {
                    customerList.Close();
                }
                break;
            case InputScene.Menu:
                if (uiActions.Menu.WasPressedThisFrame())
                {
                    //MenuButton();
                }
                break;
        }
    }

    #region Command
    public void CommandTriggerUpdate()
    {
        targetObjectScript.CommandTriggerUpdate();
    }

    public void CommandTriggerTrue()
    {
        if (!targetObjectScript.nowDown &&
            (targetObjectScript.havingOK || targetObjectScript.havingWater || targetObjectScript.foodDrop || 
            targetObjectScript.foodPut || targetObjectScript.orderTrue || targetObjectScript.sitdownCustomer || 
            targetObjectScript.cleaningToolPut || targetObjectScript.cleanupTable || targetObjectScript.guideCustomer || 
            targetObjectScript.workCashRegister || targetObjectScript.havingWagon || targetObjectScript.putWagon))
        {
            if (targetObjectScript.havingOK)//持つ
            {
                commandSetting(() => targetObjectScript.Having(), "持つ");
            }
            else if (targetObjectScript.havingWagon)//ワゴン 持つ
            {
                commandSetting(() => targetObjectScript.HavingWagon(), "持つ");
            }
            else if (targetObjectScript.foodDrop)//捨てる
            {
                commandSetting(() => targetObjectScript.ObjectDestroy(), "返却");
            }
            else if (targetObjectScript.foodPut)//置く
            {
                commandSetting(() => targetObjectScript.FoodPut(), "置く");
            }
            else if (targetObjectScript.putWagon)//ワゴン 置く
            {
                commandSetting(() => targetObjectScript.PutWagon(), "置く");
            }
            else if (targetObjectScript.guideCustomer)//案内を開始する
            {
                commandSetting(() => CustomerList(), "案内");
            }
            else if (targetObjectScript.sitdownCustomer)//席に案内する
            {
                commandSetting(() => targetObjectScript.SitdownCustomer(targetObjectScript.nowGuideCustomer.GetCustomerNumber()), "案内");
            }
            else if (targetObjectScript.cleaningToolPut)//掃除用具を置く
            {
                commandSetting(() => targetObjectScript.PutCleaningTool(), "置く");
            }
            else if (targetObjectScript.orderTrue)//注文を取る
            {
                commandSetting(orderTime, 1,"注文");}
            else if (targetObjectScript.cleanupTable)//テーブルを掃除する
            {
                commandSetting(cleanupTime, 2, "片付け");}
            else if (targetObjectScript.havingWater)//水汲み
            {
                commandSetting(waterTime, 3, "水汲み");}
            else if (targetObjectScript.workCashRegister)//会計
            {
                commandSetting(cashRegister, 4, "会計");
            }
        }
        else
        {
            CommandTriggerFalse();
        }
    }

    public void CommandTriggerFalse()
    {
        targetObjectScript.animationController.AnimationStop(targetObjectScript.animationController.NowAnimationPlayed(), false);
        commandSetting(null, "");
        ButtonExit();

    }

    /// <summary>
    /// コマンド設定(単押し)
    /// </summary>
    /// <param name="call"></param>
    /// <param name="displayText"></param>
    void commandSetting(Action call,string displayText)
    {

        if (call != null)
        {
            commandButtonEnterAction = call;
            buttonManager.EnterButtonOn();

            buttonManager.EnterButtonReset();
            //enterButton.GetComponent<Button>().onClick.RemoveAllListeners();
            //enterButton.GetComponent<EventTrigger>().triggers.RemoveRange(0, enterButton.GetComponent<EventTrigger>().triggers.Count);
            //buttonManager.enterButton.onClick.AddListener(() => MoveStop());
            if (nowInputPossibleDevice == InputPossibleDevice.Gamepad || nowInputPossibleDevice == InputPossibleDevice.Keyboard)
            {
                commandButtonMode = ButtonMode.Press;
                
                //enterButton.transform.GetChild(0).GetComponent<Text>().text = displayText;
            }
            else if (nowInputPossibleDevice == InputPossibleDevice.TouchScreen)
            {
                buttonManager.enterButton?.onClick.AddListener(() => commandPress());
                //enterButton.transform.GetChild(0).GetComponent<Text>().text = displayText;
            }
            //else if (nowInputPossibleDevice == InputPossibleDevice.Keyboard)
            //{
            //    enterButton.GetComponent<Button>().onClick.AddListener(() => commandPress());
            //    enterButton.transform.GetChild(0).GetComponent<Text>().text = displayText;
            //}
            buttonManager.EnterButtonTextChange(displayText);
            progressBar.BarActive(true, displayText, false);
        }
        else
        {
            commandButtonMode = ButtonMode.Null;
            commandButtonEnterAction = null;
            commandButtonDownAction = null;
            commandButtonUpAction = null;
            //enterButton.SetActive(false);
            buttonManager.EnterButtonOff();
            //enterButton.transform.GetChild(0).GetComponent<Text>().text = displayText;
            buttonManager.EnterButtonTextChange(null);
            progressBar.BarActive(false);
        }
        //buttonManager.EnterButtonTextChange(displayText);
    }

    void commandPress()
    {
        if (!nowCommandPressInterval)
        {
            PressInterval(1);
            MoveStop();
            commandButtonEnterAction?.Invoke();
        }
    }

    /// <summary>
    /// コマンド設定(長押し)
    /// </summary>
    /// <param name="time">時間</param>
    /// <param name="mode">モード</param>
    void commandSetting(float time,int mode, string displayText)
    {
        commandButtonDownAction = () => ButtonEnter(time,mode);
        commandButtonUpAction = () => ButtonExit();
        commandButtonEnterAction = null;

        //enterButton.SetActive(true);
        buttonManager.EnterButtonOn();

        buttonManager.EnterButtonReset();
        //buttonManager.enterButton.onClick.RemoveAllListeners();
        //buttonManager.enterButton.GetComponent<EventTrigger>()?.triggers.RemoveRange(0, enterButton.GetComponent<EventTrigger>().triggers.Count);
        //enterButton.GetComponent<Button>().onClick.AddListener(() => MoveStop());

        if (nowInputPossibleDevice == InputPossibleDevice.Gamepad || nowInputPossibleDevice == InputPossibleDevice.Keyboard)
        {
            commandButtonMode = ButtonMode.LongPress;
            //enterButton.transform.GetChild(0).GetComponent<Text>().text = displayText;
        }
        else if(nowInputPossibleDevice == InputPossibleDevice.TouchScreen)
        {
            EventTrigger.Entry entry1 = new EventTrigger.Entry();
            EventTrigger.Entry entry2 = new EventTrigger.Entry();
            EventTrigger.Entry entry3 = new EventTrigger.Entry();
            entry1.eventID = EventTriggerType.PointerDown;
            entry2.eventID = EventTriggerType.PointerUp;
            entry3.eventID = EventTriggerType.PointerExit;
            entry1.callback.AddListener((eventDate) => { CommandDown(); });
            entry2.callback.AddListener((eventDate) => { CommandUp(); });
            entry3.callback = entry2.callback;
            buttonManager.enterButton?.GetComponent<EventTrigger>().triggers.Add(entry1);
            buttonManager.enterButton?.GetComponent<EventTrigger>().triggers.Add(entry2);
            buttonManager.enterButton?.GetComponent<EventTrigger>().triggers.Add(entry3);
            //enterButton.transform.GetChild(0).GetComponent<Text>().text = displayText;
        }
        buttonManager.EnterButtonTextChange(displayText);
        progressBar.BarActive(true, displayText, true);
        /*
        else if (nowInputPossibleDevice == InputPossibleDevice.Keyboard)
        {
            EventTrigger.Entry entry1 = new EventTrigger.Entry();
            EventTrigger.Entry entry2 = new EventTrigger.Entry();
            EventTrigger.Entry entry3 = new EventTrigger.Entry();
            entry1.eventID = EventTriggerType.PointerDown;
            entry2.eventID = EventTriggerType.PointerUp;
            entry3.eventID = EventTriggerType.PointerExit;
            entry1.callback.AddListener((eventDate) => { CommandDown(); });
            entry2.callback.AddListener((eventDate) => { CommandUp(); });
            entry3.callback = entry2.callback;
            enterButton.GetComponent<EventTrigger>().triggers.Add(entry1);
            enterButton.GetComponent<EventTrigger>().triggers.Add(entry2);
            enterButton.GetComponent<EventTrigger>().triggers.Add(entry3);
            enterButton.transform.GetChild(0).GetComponent<Text>().text = displayText;
        }
        */
    }

    void CommandDown()
    {
        if (!nowCommandPressInterval)
        {
            PressInterval(1);
            MoveStop();
            targetObjectScript.ActionTriggerUpdate(false);
            commandButtonDownAction?.Invoke();
        }
    }

    void CommandUp()
    {
        //PressInterval(1);
        commandButtonUpAction?.Invoke();
    }

    #endregion

    #region Action

    public void ActionTrigger()
    {
        //if (!targetObjectScript.nowDown && targetObjectScript.actionTrue1 && !nowEnterButton)
        //{
        //    Action1Setting(() => Action1(), "ジャンプ");
        //}
        //else
        //{
        //    Action1Setting(null);
        //}
        if(!targetObjectScript.nowDown && targetObjectScript.actionTrue2 && !nowEnterButton)
        {
            Action2Setting(() => Action2(),"押す");
        }
        else
        {
            Action2Setting(null);
        }
    }

    void Action1Setting(Action call, string displayText = "")
    {
        if (call != null)
        {
            actionButton1Action = call;

            if (nowInputPossibleDevice == InputPossibleDevice.Gamepad || nowInputPossibleDevice == InputPossibleDevice.Keyboard)
            {
                //actionButton1.transform.GetChild(1).GetComponent<Text>().text = displayText;
            }
            else if (nowInputPossibleDevice == InputPossibleDevice.TouchScreen)
            {
                buttonManager.Action1ButtonOn();

                buttonManager.Action1ButtonReset();
                //buttonManager.action1Button?.onClick.AddListener(() => Action1Press());
                //actionButton1.transform.GetChild(1).GetComponent<Text>().text = displayText;
            }
            buttonManager.Action1ButtonTextChange(displayText);
            /*
            else if (nowInputPossibleDevice == InputPossibleDevice.Keyboard)
            {
                actionButton1.GetComponent<Button>().onClick.AddListener(() => Action1Press());
                actionButton1.transform.GetChild(1).GetComponent<Text>().text = displayText;
            }
            */
        }
        else
        {
            actionButton1Action = null;
            buttonManager.Action1ButtonOff();

        }
    }

    void Action1Press()
    {
        if (!nowAction1PressInterval)
        {
            PressInterval(2);
            //MoveStop();
            actionButton1Action?.Invoke();
        }
    }

    void Action1()
    {
        //actionButton1.GetComponent<ButtonCoolTime>().CoolTimeSetting(attackCoolTime,buttonEnableOn,buttonEnableOff);
        targetObjectScript.MyJump();
    }

    void Action2Setting(Action call, string displayText = "")
    {
        if (call != null)
        {
            actionButton2Action = call;

            if (nowInputPossibleDevice == InputPossibleDevice.Gamepad || nowInputPossibleDevice == InputPossibleDevice.Keyboard)
            {
                //actionButton1.transform.GetChild(1).GetComponent<Text>().text = displayText;
            }
            else if (nowInputPossibleDevice == InputPossibleDevice.TouchScreen)
            {
                buttonManager.Action2ButtonOn();

                buttonManager.Action2ButtonReset();
                buttonManager.action2Button?.onClick.AddListener(() => Action2Press());
                //actionButton1.transform.GetChild(1).GetComponent<Text>().text = displayText;
            }
            buttonManager.Action2ButtonTextChange(displayText);
            /*
            else if (nowInputPossibleDevice == InputPossibleDevice.Keyboard)
            {
                actionButton1.GetComponent<Button>().onClick.AddListener(() => Action1Press());
                actionButton1.transform.GetChild(1).GetComponent<Text>().text = displayText;
            }
            */
        }
        else
        {
            actionButton2Action = null;
            buttonManager.Action1ButtonOff();

        }
    }

    void Action2Press()
    {
        if (!nowAction2PressInterval)
        {
            PressInterval(3);
            //MoveStop();
            actionButton2Action?.Invoke();
        }
    }

    void Action2()
    {
        buttonManager.Action2ButtonCoolTime(attackCoolTime);
        targetObjectScript.Action2();
    }

    #endregion

    #region Help

    void HelpPress()
    {
        if (!nowHelpPressInterval)
        {
            PressInterval(4);
            targetObjectScript.HelpSet();
        }
    }

    #endregion
    void CustomerList()
    {
        //ここでお客さんリストを表示させる
        MoveStop();
        customerList.Open();
    }

    public void HavingButton()
    {
        targetObjectScript.Having();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time">time</param>
    /// <param name="mode">1.Order 2.CleanupTable</param>
    public void ButtonEnter(float time,int mode)
    {
        if(mode > 0 && mode < 5)
        {
            ControllableChange(false, false);
            nowEnterButton = true;
            pushButtonTime = 0f;
            pushButtonTimeLimit = time;
            //progressBar.BarActive(true);
            buttonMode = mode;
            if (mode == 1)
            {
                targetObjectScript.OrderStart();
            }
            else if (mode == 2)
            {
                targetObjectScript.CleanupTableStart();
            }
            else if (mode == 3)
            {
                targetObjectScript.HavingWaterStart();
            }
            else if (mode == 4)
            {
                targetObjectScript.CashRegisterWorkStart();
            }
        }
        
    }

    public void ButtonExit()
    {
        
        if (nowEnterButton)
        {
            ButtonEnd();
            //progressBar.BarActive(false);
            nowEnterButton = false;
            int select = 0;
            if (pushButtonTime >= pushButtonTimeLimit)
            {
                select = 1;//成功
                progressBar.BarActive(false);
                //targetObjectScript.OrderCreate();
                //TriggerUpdate();
            }
            else
            {
                select = 2;//失敗
                progressBar.BarActive(true,longPress:true);
                //targetObjectScript.OrderStop();
            }

            if(select == 1)
            {
                if(buttonMode == 1)
                {
                    targetObjectScript.OrderCreate();
                    //ButtonEnd();
                }
                else if(buttonMode == 2)
                {
                    targetObjectScript.CleanupTable();
                    //ButtonEnd();
                }
                else if (buttonMode == 3)
                {
                    targetObjectScript.HavingWater();
                    //ButtonEnd();
                }
                else if (buttonMode == 4)
                {
                    targetObjectScript.CashRegisterWork();
                    //ControllableChange(false, false);
                    //Invoke(nameof(ButtonEnd), 1.45f);
                    //ButtonEnd();


                }
            }
            else if(select == 2)
            {
                //ButtonEnd();
                
                if (buttonMode == 1)
                {
                    targetObjectScript.OrderStop();
                }
                else if (buttonMode == 2)
                {
                    targetObjectScript.CleanupTableStop();
                }
                else if (buttonMode == 3)
                {
                    targetObjectScript.HavingWaterStop();
                }
                else if (buttonMode == 4)
                {
                    targetObjectScript.CashRegisterWorkStop();
                }
                targetObjectScript.animationController.AnimationStop(targetObjectScript.animationController.NowAnimationPlayed(), false);
            }
            pushButtonTime = 0f;
            targetObjectScript.ActionTriggerUpdate(true);
        }
    }
    void ButtonEnd()
    {
        ControllableChange(true, true);
    }

    public void DashButton()
    {
        if (!dashFlag)
        {
            Debug.Log("ダッシュオン");
            dashButton.GetComponent<Image>().color = Color.cyan;
            walkingSpeed += dashPlusSpeed;
            dashFlag = true;
        }
        else
        {
            Debug.Log("ダッシュオフ");
            dashButton.GetComponent<Image>().color = Color.white;
            walkingSpeed -= dashPlusSpeed;
            dashFlag = false;
        }
    }
    public void ScreenTapDown()
    {
        screenTap = true;
    }

    public void ScreenTapUp()
    {
        screenTap = false;
    }

    #region CustomerList

    void CustomerListOpen()
    {
        nowSelectScene = InputScene.CustomerList;
        //playerController.ControllableChange(false);
        targetObjectScript.CustomerListOpen();
    }

    void CustomerListClose()
    {
        nowSelectScene = InputScene.Game;
        //playerController.ControllableChange(true);
        targetObjectScript.CustomerListClose();
    }

    #endregion

}
