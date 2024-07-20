using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerMovePoint : MonoBehaviour
{
    [SerializeField] Transform firstTargetPosition;//�ŏ��ɂ��q���񂪖ڎw���ꏊ
    [SerializeField] Transform registerRoute;//���W�ɕ��ԃ��[�g
    [SerializeField] Transform exitRoute;//�o���ɐi�ރ��[�g

    public Vector3 GetFirstPosition()
    {
        return firstTargetPosition.position;
    }

    public List<Vector3> GetRegisterRoute()
    {
        List<Vector3> temp = new List<Vector3>();
        foreach (Transform i in registerRoute.transform)
        {
            temp.Add(i.position);
        }
        return temp;
    }

    public List<Vector3> GetExitRoute()
    {
        List<Vector3> temp = new List<Vector3>();
        foreach(Transform i in exitRoute.transform)
        {
            temp.Insert(0, i.position);
        }
        return temp;
    }

}
