using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MatchingSelect : MonoBehaviour
{

    [System.NonSerialized] public Action<int, string> onModeSelect;
    [System.NonSerialized] public UnityEvent onCloseSelect = new UnityEvent();

    [SerializeField] GameObject dontTapPanel;

    [SerializeField][Header("モードセレクト")] GameObject modeSelect;
    [SerializeField] SwipeSelect modeSelectSwipe;
    [SerializeField] Text modeInformationText;

    [SerializeField][Header("フレンドセレクト")] GameObject friendSelect;
    [SerializeField] SwipeSelect friendSelectSwipe;
    [SerializeField] Text friendInformationText;

    [SerializeField][Header("キー入力")] NumericKeypad numericKeypad;

    //InputActions

    GameActions gameActions;
    GameActions.UIActions uiActions = new GameActions.UIActions();
    //[System.NonSerialized] public InputAction enterAction;
    //[System.NonSerialized] public InputAction backAction;
    //[System.NonSerialized] public InputAction leftAction;
    //[System.NonSerialized] public InputAction rightAction;

    string[] modeNames = new string[] { "ランダム戦", "フレンド戦" };
    string[] friendNames = new string[] { "ルームを作成する", "ルームに参加する" };

    bool isControl = false;//操作できるか
    bool isSelected = false;//選択したか
    bool inputSetting = false;//設定が完了しているか

    float panelFadeTime = 0.2f;
    float PanelMoveRange = 15f;

    int roomCodeMaxCount = 6;//ルームコードの最大桁数
    int roomCodeMinCount = 6;//ルームコードの最小桁数
    string tempRoomCode = null;

    enum Select
    {
        Null,
        ModeSelect,
        FriendSetting,
        InputNumber
    }

    Select selectMode = Select.ModeSelect;

    public void SelectOpen()
    {
        //enterAction?.Enable();
        //backAction?.Enable();
        //leftAction?.Enable();
        //rightAction?.Enable();
        gameActions = new GameActions();
        gameActions?.Enable();
        uiActions = gameActions.UI;

        modeSelectSwipe.SwipeSetting(modeNames);
        ChangeModeInformation(0);
        modeSelectSwipe.onUpdateSelect += ChangeModeInformation;

        friendSelectSwipe.SwipeSetting(friendNames);
        ChangeFriendInformation(0);
        friendSelectSwipe.onUpdateSelect += ChangeFriendInformation;

        numericKeypad.onCloseKeyboard.AddListener(() => ChangePanel(3));
        numericKeypad.onReturnString += EndInput;

        inputSetting = true;
        isSelected = false;
        ChangePanel(1,1);//フレンド前提で起動
    }

    public void SelectClose()
    {
        ChangePanel(5);
    }

    void Start()
    {
        ChangePanel(0);
    }

    
    void Update()
    {
        InputSetting();
    }

    void InputSetting()
    {
        if (isControl && GameVariable.nowApplicationFocus)
        {
            switch (selectMode)
            {
                case Select.ModeSelect:
                    if (uiActions.Enter.WasPressedThisFrame())
                    {
                        ModeChange(modeSelectSwipe.ReturnSelect());
                    }
                    if (uiActions.Right.WasPressedThisFrame())
                    {
                        modeSelectSwipe.BackNext(true);
                    }
                    if (uiActions.Left.WasPressedThisFrame())
                    {
                        modeSelectSwipe.BackNext(false);
                    }
                    if (uiActions.Back.WasPressedThisFrame())
                    {
                        onModeSelect?.Invoke(0, null);
                    }
                    break;
                case Select.FriendSetting:
                    if (uiActions.Enter.WasPressedThisFrame())
                    {
                        FriendChange(friendSelectSwipe.ReturnSelect());
                    }
                    if (uiActions.Right.WasPressedThisFrame())
                    {
                        friendSelectSwipe.BackNext(true);
                    }
                    if (uiActions.Left.WasPressedThisFrame())
                    {
                        friendSelectSwipe.BackNext(false);
                    }
                    if (uiActions.Back.WasPressedThisFrame())
                    {
                        //ChangePanel(2);
                        onModeSelect?.Invoke(0, null);
                    }
                    break;
            }
        }
    }

    void ChangePanel(int panelNumber,int subNumber = 0)//パネルを切り替える
    {
        if(panelNumber == 0)//非表示
        {
            this.gameObject.GetComponent<CanvasGroup>().DOFade(0, 0);
            modeSelect.GetComponent<CanvasGroup>().DOFade(0, 0);
            friendSelect.GetComponent<CanvasGroup>().DOFade(0, 0);
            modeSelect.SetActive(false);
            friendSelect.SetActive(false);
            selectMode = Select.Null;
            isControl = false;
            this.gameObject.SetActive(false);
        }
        else
        {
            if (!this.gameObject.activeSelf)
            {
                this.gameObject.SetActive(true);
            }
            StartCoroutine(Change());
        }
        IEnumerator Change()
        {
            switch (panelNumber)
            {
                case 1://表示
                    if (subNumber == 0)//mode
                    {
                        modeSelect.GetComponent<CanvasGroup>().DOFade(1, 0);
                        friendSelect.GetComponent<CanvasGroup>().DOFade(0, 0);
                        modeSelect.SetActive(true);
                        friendSelect.SetActive(false);
                        this.gameObject.GetComponent<CanvasGroup>().DOFade(1, panelFadeTime);
                        yield return new WaitForSeconds(panelFadeTime);
                        selectMode = Select.ModeSelect;
                        dontTapPanel.SetActive(false);
                        isControl = true;
                    }
                    else if(subNumber == 1)//friend
                    {
                        modeSelect.GetComponent<CanvasGroup>().DOFade(0, 0);
                        friendSelect.GetComponent<CanvasGroup>().DOFade(1, 0);
                        modeSelect.SetActive(false);
                        friendSelect.SetActive(true);
                        this.gameObject.GetComponent<CanvasGroup>().DOFade(1, panelFadeTime);
                        yield return new WaitForSeconds(panelFadeTime * 2);
                        selectMode = Select.FriendSetting;
                        dontTapPanel.SetActive(false);
                        isControl = true;
                    }
                    break;
                case 2://mode
                    isControl = false;
                    modeSelect.SetActive(true);
                    friendSelect.GetComponent<RectTransform>().DOLocalMoveY(-PanelMoveRange, panelFadeTime);
                    friendSelect.GetComponent<CanvasGroup>().DOFade(0, panelFadeTime).OnComplete(() => friendSelect.SetActive(false));
                    yield return new WaitForSeconds(panelFadeTime);
                    modeSelect.GetComponent<RectTransform>().DOLocalMoveY(PanelMoveRange, 0);
                    modeSelect.GetComponent<RectTransform>().DOLocalMoveY(0, panelFadeTime);
                    modeSelect.GetComponent<CanvasGroup>().DOFade(1, panelFadeTime);
                    yield return new WaitForSeconds(panelFadeTime);
                    selectMode = Select.ModeSelect;
                    isControl = true;
                    break;
                case 3://friend
                    if (!friendSelect.activeSelf)
                    {
                        isControl = false;
                        friendSelect.SetActive(true);
                        modeSelect.GetComponent<RectTransform>().DOLocalMoveY(PanelMoveRange, panelFadeTime);
                        modeSelect.GetComponent<CanvasGroup>().DOFade(0, panelFadeTime).OnComplete(() => modeSelect.SetActive(false));
                        yield return new WaitForSeconds(panelFadeTime);
                        friendSelect.GetComponent<RectTransform>().DOLocalMoveY(-PanelMoveRange, 0);
                        friendSelect.GetComponent<RectTransform>().DOLocalMoveY(0, panelFadeTime);
                        friendSelect.GetComponent<CanvasGroup>().DOFade(1, panelFadeTime);
                        yield return new WaitForSeconds(panelFadeTime);
                    }
                    if(tempRoomCode == null)//ルームコードが入ってたら操作できなくする
                    {
                        dontTapPanel.SetActive(false);
                        selectMode = Select.FriendSetting;
                        isControl = true;
                    }
                    break;
                case 4://一時的に停止
                    selectMode = Select.Null;
                    isControl = false;
                    break;
                case 5://設定完了
                    isControl = false;
                    dontTapPanel.SetActive(true);
                    this.gameObject.GetComponent<CanvasGroup>().DOFade(0, panelFadeTime);
                    yield return new WaitForSeconds(panelFadeTime);
                    selectMode = Select.Null;
                    onCloseSelect.Invoke();
                    this.gameObject.SetActive(false);
                    break;
            }
            yield return null;
        }
    }
    #region DisplayInformation

    void ChangeModeInformation(int number)
    {
        switch (number)
        {
            case 0:
                modeInformationText.text =
                    "ランダムで誰かとマッチングします。\n(デバッグ用)";
                break;
            case 1:
                modeInformationText.text =
                    "ルームコードを使ってマッチングします。\nルームコードの入力が必要です。";
                break;
        }
    }

    void ChangeFriendInformation(int number)
    {
        switch (number)
        {
            case 0:
                friendInformationText.text =
                    "ルームを作成します。\n表示されたルームコードを相手に伝えてください。";
                break;
            case 1:
                friendInformationText.text =
                    "ルームに参加します。\nルームコードを入力してください。";
                break;
        }
    }

    #endregion


    #region ModeSelect



    public void ModeEnterButton()
    {
        if (isControl)
        {
            ModeChange(modeSelectSwipe.ReturnSelect());
        }
    }

    public void ModeBackButton()
    {
        if (isControl)
        {
            SoundList.Instance.SoundEffectPlay(3);
            onModeSelect?.Invoke(0, null);
        }
    }

    void ModeChange(int mode)
    {
        SoundList.Instance.SoundEffectPlay(3);
        switch (mode)
        {
            case 0://ランダム
                onModeSelect?.Invoke(1, null);
                break;
            case 1://フレンド
                ChangePanel(3);
                break;
        }
    }

    #endregion

    #region FriendSelect



    public void FriendEnterButton()
    {
        if (isControl)
        {
            FriendChange(friendSelectSwipe.ReturnSelect());
        }
    }

    public void FriendBackButton()
    {
        if (isControl)
        {
            SoundList.Instance.SoundEffectPlay(3);
            onModeSelect?.Invoke(0, null);
            //ChangePanel(2);
        }
    }

    void FriendChange(int mode)
    {
        if (!isSelected)
        {
            isSelected = true;
            SoundList.Instance.SoundEffectPlay(3);
            switch (mode)
            {
                case 0://ルームの作成
                    onModeSelect?.Invoke(2, null);
                    break;
                case 1://ルームに参加
                    ChangePanel(4);
                    numericKeypad.KeyboardOpen(roomCodeMinCount, roomCodeMaxCount);
                    break;
            }
        }
    }

    void EndInput(string text)
    {
        tempRoomCode = text;
        dontTapPanel.SetActive(true);
        onModeSelect?.Invoke(3, text);
    }

    public void ErrorInput()//部屋に入れなかったとき
    {
        ChangePanel(4);
        dontTapPanel.SetActive(false);
        tempRoomCode = null;
        numericKeypad.KeyboardOpen(roomCodeMinCount, roomCodeMaxCount);
    }

    #endregion
}
