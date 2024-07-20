using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerSelector : MonoBehaviour
{
    [System.NonSerialized] public UnityEvent<PlayerAnimationController> onSetSkin = new UnityEvent<PlayerAnimationController>();
    [System.NonSerialized] public PlayerAnimationController animationController;

    [SerializeField] GameObject objectSkins;//プレイヤーが入ってるオブジェクト
    [SerializeField] bool uiVer = false;//3DUIかどうか

    List<GameObject> playerObjects = new List<GameObject>();
    List<Animator> playerAnimators = new List<Animator>();
    List<PlayerAnimationController> playerAnimationControllers = new List<PlayerAnimationController>();

    int nowSelectNumber = 0;

    void Awake()
    {
        if (uiVer)
        {
            Transform[] temp = objectSkins.GetComponentsInChildren<Transform>();
            foreach (Transform childtemp in temp)
            {
                childtemp.gameObject.layer = GameVariable.set3DUILayer;
            }
        }
        foreach (Transform gObject in objectSkins.transform)
        {
            if (gObject.GetComponent<Animator>() && gObject.GetComponent<PlayerAnimationController>())
            {
                playerObjects.Add(gObject.gameObject);
                playerAnimators.Add(gObject.GetComponent<Animator>());
                playerAnimationControllers.Add(gObject.GetComponent<PlayerAnimationController>());
                if (playerObjects.Count - 1 != nowSelectNumber)
                {
                    gObject.gameObject.SetActive(false);
                }
            }
        }
    }

    void Start()
    {
        
    }

    public void SetSkin(int selectNumber)
    {
        nowSelectNumber = selectNumber;

        if(nowSelectNumber >= playerObjects.Count)
        {
            nowSelectNumber = playerObjects.Count - 1;
        }
        else if(nowSelectNumber < 0)
        {
            nowSelectNumber = 0;
        }

        for(int i = 0; i < playerObjects.Count; i++)
        {
            if(i == nowSelectNumber)
            {
                playerObjects[i].SetActive(true);
                animationController = playerAnimationControllers[i];
            }
            else
            {
                playerObjects[i].SetActive(false);
            }
        }
        onSetSkin.Invoke(animationController);
    }
}
