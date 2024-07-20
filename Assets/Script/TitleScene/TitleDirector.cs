using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

public class TitleDirector : MonoBehaviour
{
    //Warning
    [SerializeField] CanvasGroup titlePanel;
    [SerializeField] CanvasGroup warningPanel;
    //Title
    [SerializeField]
    [Header("バージョン")] Text versionText;
    [SerializeField]
    [Header("Pressテキスト")] Text anyPressText;
    [SerializeField] GameObject mobileTapPanel;
    //[SerializeField]
    //[Header("ChangeWindowテキスト")] Text changeWindowsText;
    //Setting
    [SerializeField] GameSetting gameSetting;
    //Exit
    [SerializeField] GameObject backPanel;

    enum InputMode
    {
        Null,
        Title,
        Back,
        Setting
    }

    InputMode nowInputMode = InputMode.Null;

    GameActions gameActions;
    GameActions.UIActions uiActions = new GameActions.UIActions();

    bool flag = false;
    string pressText;

    Tween warningTween = null;
    int screenMode = 0;

    bool controlTrue = false;
    float panelMoveSpeed = 0.25f;
    float warningPanelDisplayTime = 3f;

    void Awake()
    {

    }

    void Start()
    {
        //カーソル
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        //QualitySettings.vSyncCount = 1;//画面同期
        //Application.targetFrameRate = 60;//FPS
        //Time.fixedDeltaTime = 1f / Screen.currentResolution.refreshRate;//RigidBodyカクツキ防止用
        versionText.text = "Ver " + Application.version;

        SaveData.BootCheck();
        SaveData.SystemSaveData.Load();
        SaveData.GameSaveData.Load();
        ChangeWindow(SaveData.SystemSaveData.screenSize, SaveData.SystemSaveData.fullScreen);

        if(SaveData.GameSaveData.playerName == "")
        {
            SaveData.GameSaveData.playerName = "Player" + Random.Range(0000, 9999).ToString("0000");
            SaveData.GameSaveData.Save();
        }

        titlePanel.alpha = 0;
        warningPanel.alpha = 0;
        controlTrue = false;

        gameSetting.onSettingClose.AddListener((flag) => SettingClose(flag));

        backPanel.GetComponent<CanvasGroup>().alpha = 0;
        backPanel.SetActive(false);

        gameActions = new GameActions();
        gameActions?.Enable();
        uiActions = gameActions.UI;

        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:

                QualitySettings.vSyncCount = 1;//画面同期
                //Application.targetFrameRate = 60;//FPS
                Time.fixedDeltaTime = 1f / Screen.currentResolution.refreshRate;//RigidBodyカクツキ防止用

                mobileTapPanel.SetActive(false);
                if (Gamepad.current != null)
                {
                    pressText = "Press A To Start";
                }
                else if(Keyboard.current != null)
                {
                    pressText = "Press Space To Start";
                }
                break;
            case RuntimePlatform.Android:

                QualitySettings.vSyncCount = 1;//画面同期
                Application.targetFrameRate = 60;//FPS
                Time.fixedDeltaTime = 1f / Screen.currentResolution.refreshRate;//RigidBodyカクツキ防止用

                mobileTapPanel.SetActive(true);
                pressText = "Touch To Start";
                break;
        }
        anyPressText.text = pressText;
        if (!GameVariable.gameStarted)
        {
            //ChangeWindow(2);//提出時はON
            warningPanel.DOFade(1, panelMoveSpeed).SetEase(Ease.Linear).SetDelay(panelMoveSpeed * 2);
            warningPanel.DOFade(0, panelMoveSpeed).SetEase(Ease.Linear).SetDelay(warningPanelDisplayTime);
            titlePanel.DOFade(1, panelMoveSpeed).SetEase(Ease.Linear).SetDelay(warningPanelDisplayTime + panelMoveSpeed * 3).OnComplete(() => TitleStart());
            GameVariable.gameStarted = true;
        }
        else
        {
            titlePanel.alpha = 1;
            Invoke(nameof(TitleStart), 1.0f);
            //TitleStart();
        }
        
    }

    void TitleStart()
    {
        warningPanel.gameObject.SetActive(false);
        flag = false;
        SoundList.Instance.BGM(SoundList.PlayMode.Play, 0, 0.5f);
        controlTrue = true;
        nowInputMode = InputMode.Title;
    }

    void Update()
    {
        if (controlTrue && GameVariable.nowApplicationFocus)
        {
            switch (nowInputMode)
            {
                case InputMode.Title:
                    
                    if (uiActions.Enter.WasPressedThisFrame())
                    {
                        StartGame();
                    }
                    else if (uiActions.Back.WasPressedThisFrame())
                    {
                        OpenBack();
                    }
                    else if (uiActions.Menu.WasPressedThisFrame())
                    {
                        //ChangeWindow();
                        SettingOpen();
                    }
                    break;
                case InputMode.Back:
                    
                    if (uiActions.Enter.WasPressedThisFrame())
                    {
                        ApplicationClose();
                    }
                    else if (uiActions.Back.WasPressedThisFrame())
                    {
                        CloseBack();
                    }
                    break;
            }

            
        }
    }

    public void StartGame()
    {
        if (!flag)//ネットが繋がってたら
        {
            mobileTapPanel.SetActive(false);
            anyPressText.transform.DOScale(Vector3.one * 0.9f, 0.15f).SetEase(Ease.InCubic).SetLoops(2, LoopType.Yoyo);
            //anyPressText.transform.DOScale(Vector3.one * 0.9f, 0.2f).SetEase(Ease.InCubic);
            //anyPressText.DOFade(0,0.2f).SetEase(Ease.Linear);

            flag = true;
            controlTrue = false;
            //FadePanel.Instance.AutoSceneFadeMode("MatchingScene");
            SoundList.Instance.BGM(SoundList.PlayMode.Stop);
            SoundList.Instance.SoundEffectPlay(6, 0.6f);//入店音
            FadePanel.Instance.ManualSceneFadeMode(true, "MatchingScene",2,true);
        }
    }

    public void SettingOpen()
    {
        SoundList.Instance.SoundEffectPlay(3, 1.0f);
        gameSetting.Open();
        nowInputMode = InputMode.Setting;
    }

    public void SettingClose(bool changeWindows)
    {
        nowInputMode = InputMode.Title;
        if (changeWindows)
        {
            ChangeWindow(SaveData.SystemSaveData.screenSize, SaveData.SystemSaveData.fullScreen);
        }
    }

    public void OpenBack()
    {
        if (!backPanel.activeSelf)
        {
            SoundList.Instance.SoundEffectPlay(3, 1.0f);
            backPanel.SetActive(true);
            backPanel.GetComponent<CanvasGroup>().DOFade(1f, 0.2f);
            nowInputMode = InputMode.Back;
        }
        
    }

    public void CloseBack()
    {
        SoundList.Instance.SoundEffectPlay(3, 1.0f);
        nowInputMode = InputMode.Title;
        backPanel.GetComponent<CanvasGroup>().DOFade(0f, 0.2f).
            OnComplete(() => backPanel.SetActive(false));
    }

    public void ApplicationClose()
    {
        SoundList.Instance.SoundEffectPlay(3, 1.0f);
        backPanel.GetComponent<CanvasGroup>().DOFade(0f, 0.2f).
            OnComplete(() => Application.Quit());
    }

    void ChangeWindow(int screenMode,bool fullscreen)
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            switch (screenMode)
            {
                case 0:
                    Screen.SetResolution(1280, 720, fullscreen, 60);
                    break;
                case 1:
                    Screen.SetResolution(1366, 768, fullscreen, 60);
                    break;
                case 2:
                    Screen.SetResolution(1920, 1080, fullscreen, 60);
                    break;

            }
            //if(Screen.fullScreen != fullScreen)
            //{
            //    Screen.fullScreen = fullScreen;
            //    this.fullScreen = fullScreen;
            //}
        }
    }
}
