using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField][Header("�^�[�Q�b�g(None : MainCamera)")] public GameObject targetPoint;
    [SerializeField][Header("�^�[�Q�b�g���W���]")] bool reverse;
    [SerializeField][Header("�^�[�Q�b�g���W�Œ�")] bool x;
    [SerializeField] bool y;
    [SerializeField] bool z;


    Vector3 initialPosition = Vector3.zero;
    void Awake()
    {
        if (targetPoint == null)
        {
            targetPoint = Camera.main.gameObject;
        }
        initialPosition = targetPoint.transform.position;
    }

    void Update()
    {
        Vector3 tempTargetPos = targetPoint.transform.position;
        if (reverse)
        {
            tempTargetPos = tempTargetPos * -1;
        }
        if (x)
        {
            tempTargetPos.x = initialPosition.x;
        }
        if (y)
        {
            tempTargetPos.y = initialPosition.y;
        }
        if (z)
        {
            tempTargetPos.z = initialPosition.z;
        }
        transform.LookAt(tempTargetPos);
    }
}
