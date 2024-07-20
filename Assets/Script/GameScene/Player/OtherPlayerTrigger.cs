using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OtherPlayerTrigger : MonoBehaviour
{
    [System.NonSerialized] public UnityEvent onOtherPlayerTriggerEnter = new UnityEvent();
    [System.NonSerialized] public UnityEvent onOtherPlayerTriggerExit = new UnityEvent();
    [System.NonSerialized] public GameObject targetObject;
    [System.NonSerialized] public PlayerRPC playerRPC;

    void Start()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if(targetObject != this.transform.parent.gameObject && other.gameObject.GetComponent<PlayerRPC>())
        {
            targetObject = other.gameObject;
            playerRPC = targetObject.GetComponent<PlayerRPC>();
            onOtherPlayerTriggerEnter.Invoke();
        }
    }

    void OnTriggerExit(Collider other)
    {
        targetObject = null;
        onOtherPlayerTriggerExit.Invoke();
    }
}
