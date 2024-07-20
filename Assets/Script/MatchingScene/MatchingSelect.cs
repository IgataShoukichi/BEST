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

    [SerializeField][Header("���[�h�Z���N�g")] GameObject modeSelect;
    [SerializeField] SwipeSelect modeSelectSwipe;
    [SerializeField] Text modeInformationText;

    [SerializeField][Header("�t�����h�Z���N�g")] GameObject friendSelect;
    [SerializeField] SwipeSelect friendSelectSwipe;
    [SerializeField] Text friendInformationText;

    [SerializeField][Header("�L�[����")] NumericKeypad numericKeypad;

    //InputActions

    GameActions gameActions;
    GameActions.UIActions uiActions = new GameActions.UIActions();
    //[System.NonSerialized] public InputAction enterAction;
    //[System.NonSerialized] public InputAction backAction;
    //[System.NonSerialized] public InputAction leftAction;
    //[System.NonSerialized] public InputAction rightAction;

    string[] modeNames = new string[] { "�����_����", "�t�����h��" };
    string[] friendNames = new string[] { "���[�����쐬����", "���[���ɎQ������" };

    bool isControl = false;//����ł��邩
    bool isSelected = false;//�I��������
    bool inputSetting = false;//�ݒ肪�������Ă��邩

    float panelFadeTime = 0.2f;
    float PanelMoveRange = 15f;

    int roomCodeMaxCount = 6;//���[���R�[�h�̍ő包��
    int roomCodeMinCount = 6;//���[���R�[�h�̍ŏ�����
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
        ChangePanel(1,1);//�t�����h�O��ŋN��
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

    void ChangePanel(int panelNumber,int subNumber = 0)//�p�l����؂�ւ���
    {
        if(panelNumber == 0)//��\��
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
                case 1://�\��
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
                    if(tempRoomCode == null)//���[���R�[�h�������Ă��瑀��ł��Ȃ�����
                    {
                        dontTapPanel.SetActive(false);
                        selectMode = Select.FriendSetting;
                        isControl = true;
                    }
                    break;
                case 4://�ꎞ�I�ɒ�~
                    selectMode = Select.Null;
                    isControl = false;
                    break;
                case 5://�ݒ芮��
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
                    "�����_���ŒN���ƃ}�b�`���O���܂��B\n(�f�o�b�O�p)";
                break;
            case 1:
                modeInformationText.text =
                    "���[���R�[�h���g���ă}�b�`���O���܂��B\n���[���R�[�h�̓��͂��K�v�ł��B";
                break;
        }
    }

    void ChangeFriendInformation(int number)
    {
        switch (number)
        {
            case 0:
                friendInformationText.text =
                    "���[�����쐬���܂��B\n�\�����ꂽ���[���R�[�h�𑊎�ɓ`���Ă��������B";
                break;
            case 1:
                friendInformationText.text =
                    "���[���ɎQ�����܂��B\n���[���R�[�h����͂��Ă��������B";
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
            case 0://�����_��
                onModeSelect?.Invoke(1, null);
                break;
            case 1://�t�����h
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
                case 0://���[���̍쐬
                    onModeSelect?.Invoke(2, null);
                    break;
                case 1://���[���ɎQ��
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

    public void ErrorInput()//�����ɓ���Ȃ������Ƃ�
    {
        ChangePanel(4);
        dontTapPanel.SetActive(false);
        tempRoomCode = null;
        numericKeypad.KeyboardOpen(roomCodeMinCount, roomCodeMaxCount);
    }

    #endregion
}
