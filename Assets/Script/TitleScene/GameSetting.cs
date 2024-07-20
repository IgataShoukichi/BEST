using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using Unity.VisualScripting;

public class GameSetting : MonoBehaviour
{
    [System.NonSerialized] public UnityEvent<bool> onSettingClose = new UnityEvent<bool>();//閉じた時

    [SerializeField] GameObject settingBasePanel;//全体
    [SerializeField] Transform settingContent;//設定パネルが入っているやつ
    List<GameObject> settingPanels = new List<GameObject>();
    List<SettingParts> settingPanelParts = new List<SettingParts>();

    int nowSelectPanel = 0;

    GameActions gameActions;
    GameActions.UIActions uiActions = new GameActions.UIActions();

    enum InputMode
    {
        Null,
        Select
    }
    InputMode nowInputMode = InputMode.Null;

    void Awake()
    {
        gameActions = new GameActions();
        gameActions?.Enable();//Actionを有効化
        uiActions = gameActions.UI;
    }


    void Start()
    {
        settingBasePanel.SetActive(true);
        settingBasePanel.GetComponent<CanvasGroup>().alpha = 0;
        settingBasePanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
        foreach (Transform panel in settingContent)
        {
            if (panel.childCount > 1 && panel.GetChild(1).gameObject.GetComponent<SettingParts>())
            {
                SettingParts settingParts = panel.GetChild(1).gameObject.GetComponent<SettingParts>();
                settingPanels.Add(panel.gameObject);
                settingPanelParts.Add(settingParts);
                settingParts.onInputTextStart.AddListener(() => InputTextChange(true));
                settingParts.onInputTextEnd.AddListener(() => InputTextChange(false));
            }
        }
        PanelSetting();
        
        //settingBasePanel.SetActive(false);
        nowInputMode = InputMode.Null;
        //SelectPanel(nowSelectPanel);
        //Open();
    }


    void Update()
    {
        if (GameVariable.nowApplicationFocus && nowInputMode == InputMode.Select)
        {
            if (uiActions.Up.WasPressedThisFrame())
            {
                MovePanels(false);
            }
            if (uiActions.Down.WasPressedThisFrame())
            {
                MovePanels(true);
            }
            if (uiActions.Enter.WasPressedThisFrame())
            {
                EnterPanel();
            }
            if (uiActions.Left.WasPressedThisFrame())
            {
                BackNext(false);
            }
            if (uiActions.Right.WasPressedThisFrame())
            {
                BackNext(true);
            }
            if (uiActions.Menu.WasPressedThisFrame() || uiActions.Back.WasPressedThisFrame())
            {
                Close();
            }
        }
    }

    void PanelSetting()
    {
        SaveData.SystemSaveData.Load();
        SaveData.GameSaveData.Load();

        settingPanelParts[0].NameChangeSetting(SaveData.GameSaveData.playerName);//プレイヤー名
        settingPanelParts[1].SwipeSetting(new string[] { "ON", "OFF" }, SaveData.SystemSaveData.fullScreen == true ? 0:1);//フルスクリーン
        settingPanelParts[2].SwipeSetting(new string[] { "1280x720", "1366x768", "1920x1080" },SaveData.SystemSaveData.screenSize);//解像度
        settingPanelParts[3].ScrollSetting(SaveData.SystemSaveData.bgmVolume);//BGM
        settingPanelParts[4].ScrollSetting(SaveData.SystemSaveData.seVolume);//SE
        settingPanelParts[5].ButtonSetting("開く", () => TutorialOpen());//tutorial
    }

    bool SettingSave()
    {
        bool windowsSizeChange = false;
        SaveData.SystemSaveData.Load();
        SaveData.GameSaveData.Load();

        SaveData.GameSaveData.playerName = settingPanelParts[0].ReturnString();

        bool fullScreenTemp;
        int screenSize;
        if (settingPanelParts[1].ReturnNumber() == 0)
        {
            fullScreenTemp = true;
        }
        else
        {
            fullScreenTemp = false;
        }
        screenSize = settingPanelParts[2].ReturnNumber();
        if(SaveData.SystemSaveData.fullScreen != fullScreenTemp 
            || SaveData.SystemSaveData.screenSize != screenSize)
        {
            windowsSizeChange = true;
        }
        else
        {
            windowsSizeChange = false;
        }

        SaveData.SystemSaveData.fullScreen = fullScreenTemp;
        SaveData.SystemSaveData.screenSize = screenSize;
        SaveData.SystemSaveData.bgmVolume = settingPanelParts[3].ReturnVolume();
        SaveData.SystemSaveData.seVolume = settingPanelParts[4].ReturnVolume();

        SaveData.SystemSaveData.Save();
        SaveData.GameSaveData.Save();
        return windowsSizeChange;
    }


    public void Open()
    {
        settingBasePanel.GetComponent<CanvasGroup>().alpha = 0f;
        settingBasePanel.GetComponent<CanvasGroup>().blocksRaycasts = true;
        settingBasePanel.GetComponent<CanvasGroup>().DOKill();
        settingBasePanel.GetComponent<CanvasGroup>().DOFade(1,0.3f).OnComplete(() => nowInputMode = InputMode.Select);
        nowSelectPanel = 0;
        settingContent.GetComponent<RectTransform>().DOLocalMoveY(0f, 0f);
        SelectPanel(nowSelectPanel);
    }

    public void Close()
    {
        nowInputMode = InputMode.Null;
        //SettingSave();
        SoundList.Instance.SoundEffectPlay(3, 1.0f);
        settingBasePanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
        settingBasePanel.GetComponent<CanvasGroup>().DOKill();
        settingBasePanel.GetComponent<CanvasGroup>().DOFade(0, 0.3f);
        onSettingClose.Invoke(SettingSave());
    }

    void MovePanels(bool down)
    {
        if (down)//下
        {
            if (nowSelectPanel < settingPanels.Count-1)
            {
                nowSelectPanel++;
            }
        }
        else//上
        {
            if (nowSelectPanel > 0)
            {
                nowSelectPanel--;
            }
        }

        float contentPosition = settingContent.GetComponent<RectTransform>().localPosition.y;
        float contentSize = settingContent.transform.parent.parent.GetComponent<RectTransform>().rect.height;
        float childPosition = -settingPanels[nowSelectPanel].GetComponent<RectTransform>().localPosition.y;
        float childHalfSize = settingPanels[nowSelectPanel].GetComponent<RectTransform>().rect.height / 2;
        //下がる
        if (childPosition + childHalfSize * 2 > contentPosition + contentSize)
        {
            float temp = (childPosition + childHalfSize * 2) - (contentPosition + contentSize);
            //Debug.Log(contentPosition + temp);
            settingContent.GetComponent<RectTransform>().DOLocalMoveY(contentPosition + temp, 0.2f);
        }
        //上がる
        else if (childPosition < contentPosition)
        {
            float temp = (contentPosition) - (childPosition);
            settingContent.GetComponent<RectTransform>().DOLocalMoveY(contentPosition - temp, 0.2f);
        }
        SelectPanel(nowSelectPanel);

    }

    void SelectPanel(int panelNumber)
    {
        for(int i = 0; i < settingPanels.Count; i++)
        {
            if(i == panelNumber)
            {
                settingPanels[i].GetComponent<Image>().DOFade(0.4f, 0.1f);
            }
            else
            {
                settingPanels[i].GetComponent<Image>().DOFade(0f, 0.1f);
            }
        }
    }

    void BackNext(bool next)
    {
        settingPanelParts[nowSelectPanel].BackNext(next);
    }
    void EnterPanel()
    {
        settingPanelParts[nowSelectPanel].Enter();
    }

    void InputTextChange(bool flag)//文字入力状態の切り替え
    {
        if (settingBasePanel.GetComponent<CanvasGroup>().blocksRaycasts)
        {
            StartCoroutine(Delay());
            IEnumerator Delay()//Enter連発防止
            {
                if (flag)
                {
                    nowInputMode = InputMode.Null;
                }
                else
                {
                    yield return null;
                    nowInputMode = InputMode.Select;
                }
            }
        }
    }

    void TutorialOpen()
    {
        nowInputMode = InputMode.Null;
        Tutorial.Instance.Open();
        Tutorial.Instance.PanelBackSetting(() => TutorialClose());
    }

    void TutorialClose()
    {
        StartCoroutine(Delay());
        IEnumerator Delay()//Enter連発防止
        {
            yield return null;
            nowInputMode = InputMode.Select;
        }
    }
}
