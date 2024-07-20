using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NewCustomerController : MonoBehaviour
{
    [System.NonSerialized] public CustomerAnimation customerAnimation;//アニメーション

    GameObject followTarget = null;//追いかける対象オブジェクト
    Transform followTargetTransform;

    Transform myTransform;
    CapsuleCollider capsuleCollider;

    float targetDistanceTrigger = 2.0f;//ターゲットからこれ以上離れたら追いかける
    float otherDistanceTrigger = 1.0f;//前の人から離れたらこれ以上離れたら追いかける

    [System.NonSerialized] public float speed = 5.4f;

    ////////////////以下の変数はいじらない
    float distanceTrigger = 0.0f;//設定の距離
    bool nowTracking = false;//追いかけるか

    void Start()
    {
        //capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
        //myRigidbody.constraints = RigidbodyConstraints.FreezeRotationX;
        //myRigidbody.constraints = RigidbodyConstraints.FreezeRotationZ;

        myTransform = this.transform;

    }

    public void AnimationSetting()
    {
        customerAnimation = this.gameObject.GetComponent<CustomerAnimation>() ? 
            this.gameObject.GetComponent<CustomerAnimation>() : this.gameObject.AddComponent<CustomerAnimation>();
    }

    public void SetFollow(GameObject targetObject,bool first)
    {
        followTarget = targetObject;
        followTargetTransform = targetObject.transform;
        if (first)
        {
            distanceTrigger = targetDistanceTrigger;
        }
        else
        {
            distanceTrigger = otherDistanceTrigger;
        }
    }

    public void StopFollow()
    {
        followTarget = null;
        followTargetTransform = null;
        nowTracking = false;
        customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.neutralCus);
    }

    
    void Update()
    {
        if(followTarget != null)
        {
            if(distanceTrigger < Vector3.Distance(followTargetTransform.position,myTransform.position))
            {
                nowTracking = true;
                customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.walkCus);
            }
            else if(distanceTrigger * 0.7f > Vector3.Distance(followTargetTransform.position, myTransform.position))
            {
                nowTracking = false;
                customerAnimation.AnimationPlay(CustomerAnimation.AnimationName.neutralCus);
            }

            Vector3 temp = followTargetTransform.position;
            temp.y = myTransform.position.y;
            myTransform.LookAt(temp);
            if (nowTracking)
            {
                myTransform.position += myTransform.forward * speed * Time.deltaTime;
            }
        }
    }
}
