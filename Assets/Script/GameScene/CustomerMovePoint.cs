using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerMovePoint : MonoBehaviour
{
    [SerializeField] Transform firstTargetPosition;//最初にお客さんが目指す場所
    [SerializeField] Transform registerRoute;//レジに並ぶルート
    [SerializeField] Transform exitRoute;//出口に進むルート

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
