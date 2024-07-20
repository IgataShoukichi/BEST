using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MatchingDirector : MonoBehaviourPunCallbacks
{
    //[SerializeField][Header("InputActions")] InputAction enterAction;
    //[SerializeField] InputAction backAction;
    //[SerializeField] InputAction leftAction;
    //[SerializeField] InputAction rightAction;

    [SerializeField] MatchingSelect matchingSelect;

    [SerializeField] Text roomCodeText;//プレイヤーの参加状況
    [SerializeField] GameObject playerPanelPrefab;//プレイヤー表のprefab
    [SerializeField] Transform PlayerListContent;//プレイヤー表が入る親オブジェクト
    [SerializeField]
    [Header("PlayerListText")] Text playerListText;//プレイヤーの参加状況
    [SerializeField] Text gameStartText;//ゲーム開始テキスト
    [SerializeField] GameObject readyButton;//ゲーム開始ボタン
    [SerializeField] GameObject skinButton;//スキンボタン

    [SerializeField][Header("Skin")] CanvasGroup skinChangePanel;
    [SerializeField] SwipeSelect skinChangeSwipe;
    [SerializeField] GameObject playerObject;

    //入力
    enum InputMode
    {
        Null,
        Matching,
        SkinChange
    }

    InputMode nowInputMode = InputMode.Null;

    GameActions gameActions;
    GameActions.UIActions uiActions = new GameActions.UIActions();


    //設定
    byte networkMaxPlayer = 4;//プレイ人数
    string gameSceneName = "GameScene";//プレイシーン名
    string roomNumber = "";

    float connectErrorTime = 10f;
    Coroutine ConnectErrorCoroutine = null;

    MatchingRPC masterMatchingRPC;//マスターRPC用スクリプト
    MatchingRPC myMatchingRPC;//自分用
    List<MatchingRPC> matchingRPCs = new List<MatchingRPC>();
    List<GameObject> playerPanelList = new List<GameObject>();
    //int masterClientActorNumber = -1;

    int selectSkinNumber = 0;
    int playerRotationDirection = 0;
    float playerRotationSpeed = 100f;//プレイヤーの回転速度

    bool isControl = false;
    bool isStandby = false;
    int matchingMode = -1;
    float gameStartDelay = 1.5f;

    void Awake()
    {
        GameVariable.Reset();
    }

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        //２回目以降エラーが発生しないように
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        // PhotonServerSettingsの設定内容でマスターサーバーへ接続
        PhotonNetwork.ConnectUsingSettings();
        //PhotonNetwork.LocalPlayer.NickName = "Player" + Random.Range(0000,9999).ToString("0000");
        PhotonNetwork.LocalPlayer.NickName = SaveData.GameSaveData.playerName;
        PhotonNetwork.SendRate = 40; // 1秒間にメッセージ送信を行う回数
        PhotonNetwork.SerializationRate = 20; // 1秒間にオブジェクト同期を行う回数
        GameVariable.masterClientActorNumber = -1;

        gameStartText.text = "";
        readyButton.SetActive(false);
        skinButton.SetActive(false);
        roomCodeText.text = "";

        //InputActions
        gameActions = new GameActions();
        gameActions?.Enable();
        uiActions = gameActions.UI;
        //matchingSelect.enterAction = enterAction;
        //matchingSelect.backAction = backAction;
        //matchingSelect.leftAction = leftAction;
        //matchingSelect.rightAction = rightAction;
        matchingSelect.onModeSelect += MatchingMode;

        selectSkinNumber = SaveData.GameSaveData.playerSkin;
        //表示されないことがあるためこの後表示
        //playerObject.GetComponent<PlayerSelector>().SetSkin(selectSkinNumber);
        skinChangeSwipe.SwipeSetting(GameVariable.skinName, selectSkinNumber);
        skinChangeSwipe.onUpdateSelect += ((number) => skinUpdata(number));
        skinChangePanel.alpha = 0f;
        skinChangePanel.interactable = false;
        skinChangePanel.blocksRaycasts = false;

        isControl = false;
        ConnectErrorCoroutine = StartCoroutine(ConnectError());
    }

    void Update()
    {
        if (isControl && GameVariable.nowApplicationFocus)
        {
            switch (nowInputMode)
            {
                case InputMode.Matching:
                    if (uiActions.Enter.WasPressedThisFrame())
                    {
                        if (isStandby)
                        {
                            ReadySetting(true);
                        }
                    }
                    if (uiActions.Back.WasPressedThisFrame())
                    {
                        if (matchingMode != 1)
                        {
                            MatchingMode(0, "");
                        }
                    }
                    if (uiActions.Menu.WasPressedThisFrame())
                    {
                        SkinButton();
                    }
                    break;
                case InputMode.SkinChange:
                    if (uiActions.Enter.WasPressedThisFrame() || uiActions.Menu.WasPressedThisFrame())
                    {
                        SkinClose();
                    }
                    if (uiActions.Left.WasPressedThisFrame())
                    {
                        skinChangeSwipe.BackNext(false);
                    }
                    if (uiActions.Right.WasPressedThisFrame())
                    {
                        skinChangeSwipe.BackNext(true);
                    }

                    //方向転換
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
                    if (playerRotationDirection != 0)
                    {
                        playerObject.transform.Rotate(Vector3.up * playerRotationSpeed * playerRotationDirection * Time.deltaTime);
                    }
                    break;
            }
            
        }
    }

    IEnumerator ConnectError()
    {
        yield return new WaitForSeconds(connectErrorTime);
        Debug.Log("ネットワークに接続できませんでした");
        ErrorPanel.Instance.ErrorOpen("ネットワークに接続できませんでした", 3f);
        PhotonNetwork.Disconnect();
        SoundList.Instance.BGM(SoundList.PlayMode.Stop);
        yield return new WaitForSeconds(2.5f);
        FadePanel.Instance.AutoSceneFadeMode("TitleScene");
    }

    void MatchingMode(int number,string roomText)
    {
        matchingMode = number;
        switch (matchingMode)
        {
            case 0://タイトルに戻る
                PhotonNetwork.Disconnect();
                SoundList.Instance.BGM(SoundList.PlayMode.Stop);
                FadePanel.Instance.AutoSceneFadeMode("TitleScene");
                break;
            case 1://ランダム
                PhotonNetwork.JoinRandomRoom();
                break;
            case 2://フレンド 作成
                RoomCreate();
                break;
            case 3://フレンド　参加
                roomNumber = roomText;
                PhotonNetwork.JoinRoom(roomText);
                break;
        }
    }

    #region server

    public override void OnConnectedToMaster()
    {
        if(ConnectErrorCoroutine != null)
        {
            StopCoroutine(ConnectErrorCoroutine);
            ConnectErrorCoroutine = null;
        }
        FadePanel.Instance.ManualSceneFadeMode(false);
        SoundList.Instance.BGM(SoundList.PlayMode.Play, 1, 0.5f);
        if (!SaveData.SystemSaveData.firstTutorial)
        {
            SaveData.SystemSaveData.firstTutorial = true;
            SaveData.SystemSaveData.Save();
            Tutorial.Instance.Open();
            Tutorial.Instance.PanelBackSetting(() => SelectOpen());
        }
        else
        {
            SelectOpen();
        }
        
    }

    void SelectOpen()
    {
        matchingSelect.SelectOpen();//設定を開く
    }

    #region Random

    // ランダムで参加できるルームが存在しないなら、新規でルームを作成する
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // ルームの参加人数を設定する
        var roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = networkMaxPlayer;
        roomOptions.IsVisible = true;//ロビーに公開するか
        roomOptions.IsOpen = true;//ルームに参加可能か
        Debug.Log("参加できるルームがないため新しく作成します");
        PhotonNetwork.CreateRoom("", roomOptions);

    }

    #endregion

    #region CreateRoom

    public void RoomCreate()
    {
        //ルーム設定
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;//最大人数
        roomOptions.IsVisible = false;//ロビーに公開するか
        roomOptions.IsOpen = true;//ルームに参加可能か
        roomNumber = Random.Range(0, 999999).ToString("000000");
        //ルームを作成
        PhotonNetwork.CreateRoom(roomNumber, roomOptions);
    }
    //ルームの作成が失敗したとき
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("ルームの作成に失敗しました");
        RoomCreate();
    }
    //ルームの作成が成功したとき
    public override void OnCreatedRoom()
    {
        Debug.Log($"ルームの作成に成功しました\nルーム番号 {roomNumber}");
    }

    #endregion

    //ルームに参加したら
    public override void OnJoinedRoom()
    {
        if(matchingMode != 1)
        {
            roomCodeText.text = $"<size=50>ルームコード</size> [{roomNumber}]";
        }
        UndarText(matchingMode);
        matchingSelect.SelectClose();
        Debug.Log("サーバーに接続しました");
        Debug.Log(PhotonNetwork.LocalPlayer.ActorNumber);
        GameVariable.masterClientActorNumber = PhotonNetwork.MasterClient.ActorNumber;
        
        //自分の同期するオブジェクトを生成
        myMatchingRPC = PhotonNetwork.Instantiate("Matching", Vector3.zero, Quaternion.identity).GetComponent<MatchingRPC>();//resourceフォルダに入れる

        StartCoroutine(GetDelay());
        IEnumerator GetDelay()
        {
            /*
            GameObject[] allObject;
            do
            {
                yield return new WaitForSeconds(0.5f);
                allObject = GameObject.FindGameObjectsWithTag("RPC");
            } while (allObject.Length <= 0);
            foreach (GameObject temp in allObject)
            {
                MatchingRPC tempRPC = temp.GetComponent<MatchingRPC>();
                if (PhotonNetwork.MasterClient.ActorNumber == tempRPC.gameObject.GetComponent<PhotonView>().Owner.ActorNumber)
                {
                    masterMatchingRPC = tempRPC;
                    masterMatchingRPC.onGameStart.AddListener(() => GameStartText());
                    //matchingRPC.onReadyOK.AddListener(() => CheckReady());
                    break;
                }
            }
            */
            //masterMatchingRPC.onGameStart.AddListener(() => GameStartText());
            UpdatePlayerList(true);
            skinButton.SetActive(true);
            playerObject.GetComponent<PlayerSelector>().SetSkin(selectSkinNumber);//表示がされない問題解決のため
            isControl = true;
            nowInputMode = InputMode.Matching;
            yield return null;
        }

    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if(matchingMode == 3)
        {
            StartCoroutine(Delay());
            IEnumerator Delay()
            {
                yield return new WaitForSeconds(2f);
                matchingSelect.ErrorInput();
            }
        }
    }

    // 他プレイヤーがルームへ参加した時に呼ばれるコールバック
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName}が参加しました");
        UpdatePlayerList(true);
        //if (PhotonNetwork.PlayerList.Length >= networkMaxPlayer && PhotonNetwork.LocalPlayer.IsMasterClient)
        //{
            

        //}
    }

    // 他プレイヤーが退出した時に呼ばれるコールバック
    public override void OnPlayerLeftRoom(Player exitPlayer)
    {
        UpdatePlayerList(false,exitPlayer);
        if(GameVariable.masterClientActorNumber == exitPlayer.ActorNumber)
        {
            PhotonNetwork.Disconnect();
            FadePanel.Instance.AutoSceneFadeMode("TitleScene");
        }
    }

    #endregion


    public void ReadySettingButton()
    {
        if (isControl)
        {
            ReadySetting(true);
        }
    }

    void ReadySetting(bool flag)
    {
        if (flag)
        {
            SoundList.Instance.SoundEffectPlay(4);
            readyButton.SetActive(!flag);
            skinButton.SetActive(!flag);
        }
        isControl = !flag;
        myMatchingRPC.ReadyOK(flag, selectSkinNumber);
    }

    void GameStart()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;//以降の入室を禁止する
        //readyButton.SetActive(false);
        GameVariable.isMasterClient = PhotonNetwork.LocalPlayer.IsMasterClient;
        masterMatchingRPC.GameStart(gameSceneName, 2f);
    }

    void GameStartAll()
    {
        SoundList.Instance.BGM(SoundList.PlayMode.Stop);
        SoundList.Instance.SoundEffectPlay(5);//開始音
        UndarText(4);
    }

    void UndarText(int mode = 0)
    {
        switch (mode)
        {
            case 0://null
                gameStartText.text = "";
                break;
            case 1://ランダム
                gameStartText.text = "マッチング中...";
                break;
            case 2://フレンド
            case 3:
                gameStartText.text = "待機中...";
                break;
            case 4:
                gameStartText.text = "開店時間になりました!";
                break;
        }
        //gameStartText.text = "開店時間になりました!";
    }

    void CheckReady()
    {
        Debug.Log("CheckReady");
        int count = 0;
        foreach(MatchingRPC rpc in matchingRPCs)
        {
            Player player = rpc.GetComponent<PhotonView>().Owner;
            GameObject temp = playerPanelList.Find(n => n.name == $"{player.NickName},{player.ActorNumber}");
            if (rpc.gameReady)
            {
                if(temp != null && !temp.transform.GetChild(1).gameObject.activeSelf)
                {
                    temp.transform.GetChild(1).gameObject.SetActive(true);
                }
                GameVariable.allPlayerSkins.Add(player.ActorNumber, rpc.skinNumber);
                count++;
            }
            else
            {
                if (temp != null && temp.transform.GetChild(1).gameObject.activeSelf)
                {
                    temp.transform.GetChild(1).gameObject.SetActive(false);
                }
            }
        }
        if(count >= networkMaxPlayer)
        {
            Invoke(nameof(GameStart), gameStartDelay);
        }
        else
        {
            GameVariable.allPlayerSkins.Clear();
        }
    }

    void UpdatePlayerList(bool join,Player exitPlayer = null)
    {
        if (join)
        {
            StartCoroutine(Delay(1f));
        }
        else
        {
            StartCoroutine(Delay(0f));
        }
        IEnumerator Delay(float delayTime)
        {
            GameObject[] allObject;
            do//検索
            {
                yield return new WaitForSeconds(0.2f);
                allObject = GameObject.FindGameObjectsWithTag("RPC");
                Debug.Log(allObject.Length);
            } while (allObject.Length <= 0);
            matchingRPCs.Clear();
            foreach (GameObject tmepObject in allObject)
            {
                var rpc = tmepObject.GetComponent<MatchingRPC>();
                rpc.onReadyOK.RemoveAllListeners();
                rpc.onReadyOK.AddListener(() => CheckReady());
                matchingRPCs.Add(rpc);
                if (PhotonNetwork.MasterClient.ActorNumber == tmepObject.gameObject.GetComponent<PhotonView>().Owner.ActorNumber)
                {
                    masterMatchingRPC = rpc;
                    masterMatchingRPC.onGameStart.RemoveAllListeners();
                    masterMatchingRPC.onGameStart.AddListener(() => GameStartAll());
                }
                matchingRPCs.Sort((a, b) => a.gameObject.GetComponent<PhotonView>().Owner.ActorNumber - b.gameObject.GetComponent<PhotonView>().Owner.ActorNumber);
            }
            if (join && exitPlayer == null)//入室
            {
                yield return new WaitForSeconds(delayTime);
                foreach(Player player in PhotonNetwork.PlayerList)
                {
                    if(playerPanelList.Find(n => n.name == $"{player.NickName},{player.ActorNumber}") == null)
                    {
                        //パネル生成
                        GameObject playerPanel = Instantiate(playerPanelPrefab);
                        playerPanel.name = $"{player.NickName},{player.ActorNumber}";
                        playerPanel.transform.SetParent(PlayerListContent.transform, false);
                        playerPanel.transform.localScale = playerPanelPrefab.transform.localScale;
                        playerPanel.GetComponent<RectTransform>().sizeDelta = playerPanelPrefab.GetComponent<RectTransform>().sizeDelta;

                        playerPanel.transform.GetChild(0).gameObject.GetComponent<Text>().text = player.NickName;
                        if(player == PhotonNetwork.LocalPlayer)
                        {
                            playerPanel.transform.GetChild(0).gameObject.GetComponent<Text>().text += "(あなた)";
                        }
                        playerPanel.transform.GetChild(1).gameObject.SetActive(false);
                        playerPanelList.Add(playerPanel);
                    }
                }
                if (matchingRPCs.Count >= networkMaxPlayer)
                {
                    if(matchingMode == 1)//ランダムなら
                    {
                        Invoke(nameof(GameStart), gameStartDelay);
                    }
                    else if (!readyButton.activeSelf)
                    {
                        UndarText(0);
                        readyButton.SetActive(true);
                        isStandby = true;
                    }
                }
                else if(readyButton.activeSelf)
                {
                    UndarText(matchingMode);
                    readyButton.SetActive(false);
                    isStandby = false;
                }
                CheckReady();
                /*
                //playerListText.text = "参加状況";
                if (playerListText != null)
                {
                    foreach (Player player in PhotonNetwork.PlayerList)
                    {
                        playerListText.text += "\n" + player.NickName;
                        if (player == PhotonNetwork.LocalPlayer)
                        {
                            playerListText.text += "(あなた)";
                        }
                    }
                }
                */
            }
            else
            {
                GameObject temp = playerPanelList.Find(n => n.name == $"{exitPlayer.NickName},{exitPlayer.ActorNumber}");
                if(temp != null)
                {
                    Destroy(temp);
                    playerPanelList.Remove(temp);
                }
                GameVariable.allPlayerSkins.Clear();
                UndarText(matchingMode);
                ReadySetting(false);
                readyButton.SetActive(false);
                isStandby = false;
                CheckReady();
            }
           
        }
        
    }

    #region Skin
    
    public void SkinButton()
    {
        nowInputMode = InputMode.SkinChange;
        SoundList.Instance.SoundEffectPlay(2, 0.3f);
        skinChangePanel.DOFade(1f,0.2f);
        skinChangePanel.interactable = true;
        skinChangePanel.blocksRaycasts = true;
    }

    public void SkinClose()
    {
        nowInputMode = InputMode.Matching;
        SoundList.Instance.SoundEffectPlay(2, 0.3f);
        selectSkinNumber = skinChangeSwipe.ReturnSelect();
        SaveData.GameSaveData.playerSkin = selectSkinNumber;
        SaveData.GameSaveData.Save();
        playerObject.transform.DORotate(Vector3.up * 160f, 0.2f);
        skinChangePanel.DOFade(0f, 0.2f);
        skinChangePanel.interactable = false;
        skinChangePanel.blocksRaycasts = false;
    }

    void skinUpdata(int number)
    {
        playerObject.GetComponent<PlayerSelector>().SetSkin(number);
    }
    #endregion

}
