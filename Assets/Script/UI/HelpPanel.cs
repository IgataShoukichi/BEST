using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using DG.Tweening;

public class HelpPanel : MonoBehaviour
{
    [SerializeField] GameObject helpContent;//ヘルプが入る場所
    [SerializeField] GameObject helpSample;//ヘルプのサンプル

    Dictionary<Player,GameObject> playerHelpPanel  = new Dictionary<Player,GameObject>();
    Dictionary<GameObject,Coroutine> playerHelpCoroutine = new Dictionary<GameObject,Coroutine>();

    float panelFadeTime = 0.2f;//フェード時間
    float panelDisplayTime = 2f;//表示時間

    void Start()
    {
        helpSample.SetActive(false);
        playerHelpPanel.Clear();
        playerHelpCoroutine.Clear();
    }

    
    public void SetHelp(Player sendPlayer)
    {
        if (playerHelpPanel.ContainsKey(sendPlayer))
        {
            if (!playerHelpCoroutine.ContainsKey(playerHelpPanel[sendPlayer]))
            {
                playerHelpPanel[sendPlayer].transform.SetAsLastSibling();
            }
            else
            {
                StopCoroutine(playerHelpCoroutine[playerHelpPanel[sendPlayer]]);
                playerHelpCoroutine.Remove(playerHelpPanel[sendPlayer]);
            }
            
        }
        else
        {
            //GameObject tempHelpPanel = Instantiate(helpSample);
            GameObject tempHelpPanel = Instantiate(helpSample, helpContent.transform);
            //tempHelpPanel.transform.SetParent(helpContent.transform);
            tempHelpPanel.transform.GetChild(0).GetComponent<Text>().text = sendPlayer.NickName;
            playerHelpPanel.Add(sendPlayer, tempHelpPanel);
        }
        playerHelpCoroutine.Add(playerHelpPanel[sendPlayer], StartCoroutine(HelpDisplay(playerHelpPanel[sendPlayer])));
    }

    IEnumerator HelpDisplay(GameObject playerPanel)
    {
        playerPanel.SetActive(true);
        playerPanel.GetComponent<CanvasGroup>().alpha = 0f;
        playerPanel.GetComponent<CanvasGroup>().DOFade(1f, panelFadeTime).SetEase(Ease.OutCubic);
        SoundList.Instance.SoundEffectPlay(12, 1.0f);
        yield return new WaitForSeconds(panelFadeTime);
        yield return new WaitForSeconds(panelDisplayTime);
        playerPanel.GetComponent<CanvasGroup>().DOFade(0f, panelFadeTime).SetEase(Ease.InCubic);
        yield return new WaitForSeconds(panelFadeTime);
        playerHelpCoroutine.Remove(playerPanel);
        playerPanel.SetActive(false);
    }
}
