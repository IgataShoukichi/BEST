using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NewCustomerController : MonoBehaviour
{
    [System.NonSerialized] public CustomerAnimation customerAnimation;//�A�j���[�V����

    GameObject followTarget = null;//�ǂ�������ΏۃI�u�W�F�N�g
    Transform followTargetTransform;

    Transform myTransform;
    CapsuleCollider capsuleCollider;

    float targetDistanceTrigger = 2.0f;//�^�[�Q�b�g���炱��ȏ㗣�ꂽ��ǂ�������
    float otherDistanceTrigger = 1.0f;//�O�̐l���痣�ꂽ�炱��ȏ㗣�ꂽ��ǂ�������

    [System.NonSerialized] public float speed = 5.4f;

    ////////////////�ȉ��̕ϐ��͂�����Ȃ�
    float distanceTrigger = 0.0f;//�ݒ�̋���
    bool nowTracking = false;//�ǂ������邩

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
