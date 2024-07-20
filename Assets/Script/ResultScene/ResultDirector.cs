using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;

public class ResultDirector : MonoBehaviour
{
    ///////////////////////////////////////////////////

    float firstWaitTime = 1f;//���ʔ��\���n�܂�܂ł̎���
    float panelChangeTime = 0.5f;//�p�l����؂�ւ��鎞��
    float titleMoveTime = 0.5f;//�^�C�g������������
    float starMoveTime = 0.3f;//������������
    float starMoveInterval = 0.5f;//���������Ԋu

    float playerRotationSpeed = 100f;//�v���C���[�̉�]���x
    
    ///////////////////////////////////////////////////

    //�p�l��
    [SerializeField] GameObject resultPanel;
    [SerializeField] GameObject personalResultPanel;
    [SerializeField] GameObject selectPanel;

    [SerializeField] GameObject title;

    [SerializeField][Header("Result")] GameObject starPanel;
    [SerializeField] GameObject starBase;
    [SerializeField] GameObject sampleStar;
    [SerializeField] GameObject customerSatisfactionText;
    [SerializeField] GameObject nextButton;

    [SerializeField][Header("PersonalResult")] Transform dataContent;
    [SerializeField] GameObject personalNextButton;
    [SerializeField] GameObject playerObject;
    [SerializeField] GameObject playerPrefab;

    [SerializeField][Header("Select")] List<GameObject> buttonList;
    [SerializeField] GameObject backPointer;


    int customerSatisfactionPercent = 0;
    int starCount = 0;//���̐�
    bool isControl = false;//����o���邩
    int nowSelectButtonList = 0;//���ݑI�����Ă���{�^���̐�
    List<Text> dataCount = new List<Text>();//�J�E���g�p�e�L�X�g
    int playerRotationDirection = 0;

    GameActions gameActions;
    GameActions.UIActions uiActions = new GameActions.UIActions();

    enum InputMode
    {
        Null,
        Result,
        PersonalResult,
        ModeSelect
    }
    InputMode nowInputMode = InputMode.Null;

    void Awake()
    {
        gameActions = new GameActions();
        gameActions?.Enable();//Action��L����
        uiActions = gameActions.UI;
    }

    void Start()
    {
        //�J�[�\��
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        isControl = false;
        resultPanel.GetComponent<CanvasGroup>().alpha = 0f;
        personalResultPanel.GetComponent<CanvasGroup>().alpha = 0f;
        selectPanel.GetComponent<CanvasGroup>().alpha = 0f;
        resultPanel.SetActive(false);
        personalResultPanel.SetActive(false);
        selectPanel.SetActive(false);
        title.SetActive(true);
        starPanel.GetComponent<CanvasGroup>().alpha = 0f;
        sampleStar.SetActive(false);
        customerSatisfactionText.SetActive(false);
        nextButton.GetComponent<Button>().onClick.AddListener(() => ResultNext(1));
        nextButton.SetActive(false);
        personalNextButton.GetComponent<Button>().onClick.AddListener(() => SelectModeEnter(0));
        personalNextButton.SetActive(false);
        foreach(Transform temp in dataContent)
        {
            dataCount.Add(temp.GetChild(temp.childCount - 1).GetComponent<Text>());
        }
        SetPersonalResultData();
        for (int i = 0;i < buttonList.Count;i++)
        {
            int temp = i;
            buttonList[i].GetComponent<Button>().onClick.AddListener(() => SelectModeEnter(temp));
        }

        customerSatisfactionPercent = GameVariable.finalEvaluation;
        if (customerSatisfactionPercent >= 80)
        {
            starCount = 5;
        }
        else if(customerSatisfactionPercent >= 60)
        {
            starCount = 4;
        }
        else if(customerSatisfactionPercent >= 40)
        {
            starCount = 3;
        }
        else if (customerSatisfactionPercent >= 20)
        {
            starCount = 2;
        }
        else if (customerSatisfactionPercent >= 0)
        {
            starCount = 1;
        }

        StartCoroutine(ResultFlow());
    }

    
    void Update()
    {
        if (isControl && GameVariable.nowApplicationFocus)
        {
            switch (nowInputMode)
            {
                case InputMode.Result:
                    if (uiActions.Enter.WasPressedThisFrame())
                    {
                        ResultNext(1);
                    }
                    break;
                case InputMode.PersonalResult:
                    if (uiActions.Enter.WasPressedThisFrame())
                    {
                        SelectModeEnter(0);
                    }
                    if (uiActions.RightStick_Left.IsPressed())
                    {
                        playerRotationDirection = 1;
                    }
                    else if (uiActions.RightStick_Right.IsPressed())
                    {
                        playerRotationDirection = -1;
                    }
                    else
                    {
                        playerRotationDirection = 0;
                    }
                    break;
                case InputMode.ModeSelect:
                    if(uiActions.Up.WasPressedThisFrame())
                    {
                        SelectUpDown(true);
                    }
                    if (uiActions.Down.WasPressedThisFrame())
                    {
                        SelectUpDown(false);
                    }
                    if (uiActions.Enter.WasPressedThisFrame())
                    {
                        SelectModeEnter(nowSelectButtonList);
                    }
                    break;
            }
            if(playerRotationDirection != 0)
            {
                playerObject.transform.Rotate(Vector3.up * playerRotationSpeed * playerRotationDirection * Time.deltaTime);
            }
        }
        
    }

    void SetPersonalResultData()
    {
        dataCount[0].text = $"�~{GameVariable.guideCustomerResult.ToString("00")}";
        dataCount[1].text = $"�~{GameVariable.orderResult.ToString("00")}";
        dataCount[2].text = $"�~{GameVariable.carryfoodResult.ToString("00")}";
        dataCount[3].text = $"�~{GameVariable.cashResult.ToString("00")}";
        dataCount[4].text = $"�~{GameVariable.cleanupTableResult.ToString("00")}";
        dataCount[5].text = $"�~{GameVariable.pushResult.ToString("00")}";

        GameObject temp = Instantiate(playerPrefab, playerObject.transform);
        temp.transform.localScale = Vector3.one;
        temp.transform.localPosition = Vector3.zero;
        //�X�L���ݒ�
        temp.GetComponent<PlayerSelector>()
            .SetSkin(SaveData.GameSaveData.playerSkin);
        //temp.GetComponent<PlayerSelector>()
        //    .SetSkin(GameVariable.allPlayerSkins[PhotonNetwork.LocalPlayer.ActorNumber]);
    }



    IEnumerator ResultFlow()//���ʕ\��
    {
        //�^�C�g���\��
        yield return new WaitForSeconds(firstWaitTime);
        SoundList.Instance.SoundEffectPlay(9, 0.5f);
        yield return new WaitForSeconds(2f);
        title.transform.DOLocalMoveY(100f, titleMoveTime).SetEase(Ease.OutCubic);
        title.GetComponent<CanvasGroup>().DOFade(0f, titleMoveTime/2).SetEase(Ease.OutCubic).
            OnComplete(() => title.SetActive(false));
        //resultTitle.transform.DOScale(Vector3.one * 0.6f, resultTitleMoveTime).SetEase(Ease.InSine);
        yield return new WaitForSeconds(titleMoveTime);

        //�p�l���\��
        resultPanel.SetActive(true);
        resultPanel.transform.localPosition += Vector3.down * 100;
        resultPanel.transform.DOLocalMove(Vector3.zero, panelChangeTime).SetEase(Ease.OutCubic);
        resultPanel.GetComponent<CanvasGroup>().DOFade(1f,panelChangeTime).SetEase(Ease.OutCubic).
            OnComplete(() => SoundList.Instance.BGM(SoundList.PlayMode.Play, 3, 0.3f));
        yield return new WaitForSeconds(panelChangeTime);

        //���\��
        starPanel.SetActive(true);
        Vector3 oldPosition = starPanel.transform.localPosition;
        starPanel.transform.localPosition += Vector3.up * 100;
        starPanel.transform.DOLocalMove(oldPosition, panelChangeTime).SetEase(Ease.OutCubic);
        starPanel.GetComponent<CanvasGroup>().DOFade(1f, panelChangeTime).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(panelChangeTime * 2);
        for (int i = 0;i < starCount; i++)//�����w�萔�\��
        {
            Transform targetStar = starBase.transform.GetChild(i);
            GameObject tempStar = Instantiate(sampleStar);
            tempStar.transform.SetParent(starPanel.transform);
            tempStar.transform.position = targetStar.position;
            tempStar.transform.localScale = targetStar.localScale * 1.5f;
            tempStar.GetComponent<CanvasGroup>().alpha = 0f;
            tempStar.SetActive(true);
            tempStar.transform.DOScale(targetStar.localScale, starMoveTime)
                .SetEase(Ease.InCubic).OnComplete(() => SoundList.Instance.SoundEffectPlay(10, 1f));
            tempStar.GetComponent<CanvasGroup>().DOFade(1f,starMoveTime * 0.6f).SetEase(Ease.OutCubic);
            yield return new WaitForSeconds(starMoveInterval);
        }
        yield return new WaitForSeconds(starMoveInterval);
        customerSatisfactionText.GetComponent<Text>().text = "���q�l�����x " + customerSatisfactionPercent + "%";
        customerSatisfactionText.SetActive(true);
        nextButton.SetActive(true);
        SoundList.Instance.SoundEffectPlay(11, 0.6f);
        nowInputMode = InputMode.Result;
        isControl = true;
        //SoundList.Instance.BGM(SoundList.PlayMode.Play, 3, 0.5f);
    }

    IEnumerator PersonalResultFlow()
    {
        resultPanel.SetActive(true);
        resultPanel.transform.DOLocalMove(Vector3.up * 100, panelChangeTime).SetEase(Ease.OutCubic);
        resultPanel.GetComponent<CanvasGroup>().DOFade(0f, panelChangeTime).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(panelChangeTime);
        personalResultPanel.SetActive(true);
        personalResultPanel.transform.localPosition += Vector3.down * 100;
        personalResultPanel.transform.DOLocalMove(Vector3.zero, panelChangeTime).SetEase(Ease.OutCubic);
        personalResultPanel.GetComponent<CanvasGroup>().DOFade(1f, panelChangeTime).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(panelChangeTime);
        personalNextButton.SetActive(true);
        isControl = true;
        nowInputMode = InputMode.PersonalResult;


    }

    IEnumerator SelectFlow()//�I��\��
    {
        resultPanel.SetActive(true);
        resultPanel.transform.DOLocalMove(Vector3.up * 100, panelChangeTime).SetEase(Ease.OutCubic);
        resultPanel.GetComponent<CanvasGroup>().DOFade(0f, panelChangeTime).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(panelChangeTime);
        selectPanel.SetActive(true);
        nowSelectButtonList = 0;
        SetSelectButton(nowSelectButtonList);
        selectPanel.transform.localPosition += Vector3.down * 100;
        selectPanel.transform.DOLocalMove(Vector3.zero, panelChangeTime).SetEase(Ease.OutCubic);
        selectPanel.GetComponent<CanvasGroup>().DOFade(1f, panelChangeTime).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(panelChangeTime);
        isControl = true;
        nowInputMode = InputMode.ModeSelect;
    }


    #region Control

    public void ResultNext(int mode)
    {
        if(isControl)
        {
            isControl = false;
            nowInputMode = InputMode.Null;
            SoundList.Instance.SoundEffectPlay(3, 0.5f);
            switch (mode)
            {
                case 1:
                    StartCoroutine(PersonalResultFlow());
                    break;
            }
            
        }
    }

    public void SelectModeEnter(int selectMode)
    {
        if(isControl)
        {
            isControl = false;
            nowInputMode = InputMode.Null;
            SoundList.Instance.SoundEffectPlay(3, 0.5f);
            switch(selectMode)
            {
                case 0:
                    PhotonNetwork.Disconnect();
                    SoundList.Instance.BGM(SoundList.PlayMode.Stop);
                    FadePanel.Instance.AutoSceneFadeMode("TitleScene", 0.5f);
                    break;
            }
        }
        
    }

    public void SelectUpDown(bool up)
    {
        if (up)
        {
            nowSelectButtonList--;
            if(nowSelectButtonList < 0)
            {
                nowSelectButtonList = buttonList.Count - 1;
            }
        }
        else
        {
            nowSelectButtonList++;
            if(nowSelectButtonList >= buttonList.Count)
            {
                nowSelectButtonList = 0;
            }
        }
        SetSelectButton(nowSelectButtonList);
    }

    void SetSelectButton(int selectMode)
    {
        if(backPointer.transform.position != buttonList[selectMode].transform.position)
        {
            backPointer.transform.position = buttonList[selectMode].transform.position;
            SoundList.Instance.SoundEffectPlay(2, 0.5f);
        }
        
    }

    #endregion
}
