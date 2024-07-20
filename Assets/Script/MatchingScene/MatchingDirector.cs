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

    [SerializeField] Text roomCodeText;//�v���C���[�̎Q����
    [SerializeField] GameObject playerPanelPrefab;//�v���C���[�\��prefab
    [SerializeField] Transform PlayerListContent;//�v���C���[�\������e�I�u�W�F�N�g
    [SerializeField]
    [Header("PlayerListText")] Text playerListText;//�v���C���[�̎Q����
    [SerializeField] Text gameStartText;//�Q�[���J�n�e�L�X�g
    [SerializeField] GameObject readyButton;//�Q�[���J�n�{�^��
    [SerializeField] GameObject skinButton;//�X�L���{�^��

    [SerializeField][Header("Skin")] CanvasGroup skinChangePanel;
    [SerializeField] SwipeSelect skinChangeSwipe;
    [SerializeField] GameObject playerObject;

    //����
    enum InputMode
    {
        Null,
        Matching,
        SkinChange
    }

    InputMode nowInputMode = InputMode.Null;

    GameActions gameActions;
    GameActions.UIActions uiActions = new GameActions.UIActions();


    //�ݒ�
    byte networkMaxPlayer = 4;//�v���C�l��
    string gameSceneName = "GameScene";//�v���C�V�[����
    string roomNumber = "";

    float connectErrorTime = 10f;
    Coroutine ConnectErrorCoroutine = null;

    MatchingRPC masterMatchingRPC;//�}�X�^�[RPC�p�X�N���v�g
    MatchingRPC myMatchingRPC;//�����p
    List<MatchingRPC> matchingRPCs = new List<MatchingRPC>();
    List<GameObject> playerPanelList = new List<GameObject>();
    //int masterClientActorNumber = -1;

    int selectSkinNumber = 0;
    int playerRotationDirection = 0;
    float playerRotationSpeed = 100f;//�v���C���[�̉�]���x

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
        //�Q��ڈȍ~�G���[���������Ȃ��悤��
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        // PhotonServerSettings�̐ݒ���e�Ń}�X�^�[�T�[�o�[�֐ڑ�
        PhotonNetwork.ConnectUsingSettings();
        //PhotonNetwork.LocalPlayer.NickName = "Player" + Random.Range(0000,9999).ToString("0000");
        PhotonNetwork.LocalPlayer.NickName = SaveData.GameSaveData.playerName;
        PhotonNetwork.SendRate = 40; // 1�b�ԂɃ��b�Z�[�W���M���s����
        PhotonNetwork.SerializationRate = 20; // 1�b�ԂɃI�u�W�F�N�g�������s����
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
        //�\������Ȃ����Ƃ����邽�߂��̌�\��
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

                    //�����]��
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
        Debug.Log("�l�b�g���[�N�ɐڑ��ł��܂���ł���");
        ErrorPanel.Instance.ErrorOpen("�l�b�g���[�N�ɐڑ��ł��܂���ł���", 3f);
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
            case 0://�^�C�g���ɖ߂�
                PhotonNetwork.Disconnect();
                SoundList.Instance.BGM(SoundList.PlayMode.Stop);
                FadePanel.Instance.AutoSceneFadeMode("TitleScene");
                break;
            case 1://�����_��
                PhotonNetwork.JoinRandomRoom();
                break;
            case 2://�t�����h �쐬
                RoomCreate();
                break;
            case 3://�t�����h�@�Q��
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
        matchingSelect.SelectOpen();//�ݒ���J��
    }

    #region Random

    // �����_���ŎQ���ł��郋�[�������݂��Ȃ��Ȃ�A�V�K�Ń��[�����쐬����
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // ���[���̎Q���l����ݒ肷��
        var roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = networkMaxPlayer;
        roomOptions.IsVisible = true;//���r�[�Ɍ��J���邩
        roomOptions.IsOpen = true;//���[���ɎQ���\��
        Debug.Log("�Q���ł��郋�[�����Ȃ����ߐV�����쐬���܂�");
        PhotonNetwork.CreateRoom("", roomOptions);

    }

    #endregion

    #region CreateRoom

    public void RoomCreate()
    {
        //���[���ݒ�
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;//�ő�l��
        roomOptions.IsVisible = false;//���r�[�Ɍ��J���邩
        roomOptions.IsOpen = true;//���[���ɎQ���\��
        roomNumber = Random.Range(0, 999999).ToString("000000");
        //���[�����쐬
        PhotonNetwork.CreateRoom(roomNumber, roomOptions);
    }
    //���[���̍쐬�����s�����Ƃ�
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("���[���̍쐬�Ɏ��s���܂���");
        RoomCreate();
    }
    //���[���̍쐬�����������Ƃ�
    public override void OnCreatedRoom()
    {
        Debug.Log($"���[���̍쐬�ɐ������܂���\n���[���ԍ� {roomNumber}");
    }

    #endregion

    //���[���ɎQ��������
    public override void OnJoinedRoom()
    {
        if(matchingMode != 1)
        {
            roomCodeText.text = $"<size=50>���[���R�[�h</size> [{roomNumber}]";
        }
        UndarText(matchingMode);
        matchingSelect.SelectClose();
        Debug.Log("�T�[�o�[�ɐڑ����܂���");
        Debug.Log(PhotonNetwork.LocalPlayer.ActorNumber);
        GameVariable.masterClientActorNumber = PhotonNetwork.MasterClient.ActorNumber;
        
        //�����̓�������I�u�W�F�N�g�𐶐�
        myMatchingRPC = PhotonNetwork.Instantiate("Matching", Vector3.zero, Quaternion.identity).GetComponent<MatchingRPC>();//resource�t�H���_�ɓ����

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
            playerObject.GetComponent<PlayerSelector>().SetSkin(selectSkinNumber);//�\��������Ȃ��������̂���
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

    // ���v���C���[�����[���֎Q���������ɌĂ΂��R�[���o�b�N
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName}���Q�����܂���");
        UpdatePlayerList(true);
        //if (PhotonNetwork.PlayerList.Length >= networkMaxPlayer && PhotonNetwork.LocalPlayer.IsMasterClient)
        //{
            

        //}
    }

    // ���v���C���[���ޏo�������ɌĂ΂��R�[���o�b�N
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
        PhotonNetwork.CurrentRoom.IsOpen = false;//�ȍ~�̓������֎~����
        //readyButton.SetActive(false);
        GameVariable.isMasterClient = PhotonNetwork.LocalPlayer.IsMasterClient;
        masterMatchingRPC.GameStart(gameSceneName, 2f);
    }

    void GameStartAll()
    {
        SoundList.Instance.BGM(SoundList.PlayMode.Stop);
        SoundList.Instance.SoundEffectPlay(5);//�J�n��
        UndarText(4);
    }

    void UndarText(int mode = 0)
    {
        switch (mode)
        {
            case 0://null
                gameStartText.text = "";
                break;
            case 1://�����_��
                gameStartText.text = "�}�b�`���O��...";
                break;
            case 2://�t�����h
            case 3:
                gameStartText.text = "�ҋ@��...";
                break;
            case 4:
                gameStartText.text = "�J�X���ԂɂȂ�܂���!";
                break;
        }
        //gameStartText.text = "�J�X���ԂɂȂ�܂���!";
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
            do//����
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
            if (join && exitPlayer == null)//����
            {
                yield return new WaitForSeconds(delayTime);
                foreach(Player player in PhotonNetwork.PlayerList)
                {
                    if(playerPanelList.Find(n => n.name == $"{player.NickName},{player.ActorNumber}") == null)
                    {
                        //�p�l������
                        GameObject playerPanel = Instantiate(playerPanelPrefab);
                        playerPanel.name = $"{player.NickName},{player.ActorNumber}";
                        playerPanel.transform.SetParent(PlayerListContent.transform, false);
                        playerPanel.transform.localScale = playerPanelPrefab.transform.localScale;
                        playerPanel.GetComponent<RectTransform>().sizeDelta = playerPanelPrefab.GetComponent<RectTransform>().sizeDelta;

                        playerPanel.transform.GetChild(0).gameObject.GetComponent<Text>().text = player.NickName;
                        if(player == PhotonNetwork.LocalPlayer)
                        {
                            playerPanel.transform.GetChild(0).gameObject.GetComponent<Text>().text += "(���Ȃ�)";
                        }
                        playerPanel.transform.GetChild(1).gameObject.SetActive(false);
                        playerPanelList.Add(playerPanel);
                    }
                }
                if (matchingRPCs.Count >= networkMaxPlayer)
                {
                    if(matchingMode == 1)//�����_���Ȃ�
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
                //playerListText.text = "�Q����";
                if (playerListText != null)
                {
                    foreach (Player player in PhotonNetwork.PlayerList)
                    {
                        playerListText.text += "\n" + player.NickName;
                        if (player == PhotonNetwork.LocalPlayer)
                        {
                            playerListText.text += "(���Ȃ�)";
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
