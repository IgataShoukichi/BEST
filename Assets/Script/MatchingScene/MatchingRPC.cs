using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

public class MatchingRPC : MonoBehaviourPunCallbacks
{
    public UnityEvent onGameStart = new UnityEvent();
    public UnityEvent onReadyOK = new UnityEvent();

    public bool gameReady = false;//開始の準備が出来たか
    public int skinNumber = 0;//スキン

    public void GameStart(string gameSceneName, float delay)
    {
        photonView.RPC(nameof(GameStartRPC), RpcTarget.AllViaServer,gameSceneName,delay);
    }

    public void ReadyOK(bool flag,int playerSelectNumber)
    {
        photonView.RPC(nameof(ReadyOKRPC), RpcTarget.AllViaServer,flag, playerSelectNumber);
    }

    #region RPC
    [PunRPC]
    void GameStartRPC(string gameSceneName,float delay)
    {
        onGameStart.Invoke();
        PhotonNetwork.IsMessageQueueRunning = false;//同期を一時停止
        //FadePanel.Instance.AutoSceneFadeMode(gameSceneName, delay);
        FadePanel.Instance.ManualSceneFadeMode(true, gameSceneName, delay, true);
    }

    [PunRPC]
    void ReadyOKRPC(bool flag,int playerSelectNumber)
    {
        skinNumber = playerSelectNumber;
        gameReady = flag;
        onReadyOK.Invoke();
    }
    #endregion
}
