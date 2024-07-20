using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CockController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("èÑâÒÇ∑ÇÈínì_ÇÃîzóÒ")]
    private Transform[] wayPoints;

    Vector3 rotation;

    void Start()
    {
        this.transform.position = wayPoints[0].position;
        List<Vector3> tempVector3 = new List<Vector3> ();
        foreach(Transform temp in wayPoints)
        {
            tempVector3.Add (temp.position);
        }
        this.transform.DOPath(tempVector3.ToArray(), 3f).SetSpeedBased().SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear).SetLookAt(0.02f);
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        rotation = this.transform.rotation.eulerAngles;
        rotation += Vector3.up * 90f;
        this.transform.rotation = Quaternion.Euler(rotation);
    }
}