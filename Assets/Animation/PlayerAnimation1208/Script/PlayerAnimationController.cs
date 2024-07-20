using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] public  Animator myAnimator;//�����̃A�j���[�^�[
    [SerializeField] Rigidbody myRigidBody;//�����̃��W�b�h�{�f�B

    ///////////////////////////////

    float playerMoveStartSpeed = 1f;

    ///////////////////////////////




    //Animator anim;//�����̃A�j���[�^�[
    //Rigidbody rb;//�����̃��W�b�h�{�f�B
    bool move = false;//�ړ�OK�ȏ�Ԃ�
    bool command = false;//�R�}���h�����͂���Ă��邩
    bool nowHit = false;//���܉�����Ă��邩

    void Start()
    {
        /*
        if(myRigidBody == null)
        {
            myRigidBody = GetComponent<Rigidbody>();
        }
        if(myAnimator == null)
        {
            myAnimator = GetComponent<Animator>();
        }
        */
    }
    public enum AnimationName//enum
    {
        neutral,    //�ҋ@
        walk,       //����
        run,        //����
        order,      //����
        register,   //���W�ł�
        bussing,    //�o�b�V���O�i�Еt���j
        push,       //����
        surprise,   //����
        hit,        //�������
        Null        //�����Ȃ�
    }
    AnimationName aniName = AnimationName.neutral;

    public enum BringAnimationName//���̂����A�j���[�V������enum
    {
        neutral,    //�����Ȃ�
        normal,     //�ʏ�
        bigCake     //���`���f�J�P�[�L�p
    }

    BringAnimationName briName = BringAnimationName.neutral;


    #region Check
    public AnimationName NowAnimationPlayed()//����m�F(�R�}���h����)
    {
        if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Order"))
        {
            return AnimationName.order;
        }
        else if(myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Cash_Register1"))
        {
            return AnimationName.register;
        }
        else if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Bussing"))
        {
            return AnimationName.bussing;
        }
        else
        {
            return AnimationName.Null;
        }
        /*
        switch (name)
        {
                
            case AnimationName.order://����
                if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Order"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

            case AnimationName.register://���W
                if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Cash_Register1"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

            case AnimationName.bussing://�o�b�V���O
                if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Bussing"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

            case AnimationName.push://����
                if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Push"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

            case AnimationName.surprise://���̂𗎂Ƃ��ċ���
                if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Surprise"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

            case AnimationName.hit://�������
                if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;



        }
        return false;
        */
    }

    //���̂����A�j���[�V�����̌���m�F
    public bool nowBringAnimationPlayed(BringAnimationName briName)
    {
        switch (briName)
        {
            case BringAnimationName.normal://�ʏ�
                if (myAnimator.GetCurrentAnimatorStateInfo(1).IsName("Bring_Hand"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

            case BringAnimationName.bigCake://���`���f�J�P�[�L
                if (myAnimator.GetCurrentAnimatorStateInfo(1).IsName("Bring_Hand1"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;
        }
        return false;
    }

    #endregion

    #region Play
    public bool AnimationPlay(AnimationName name)//�Đ�
    {
        switch (name)
        {
            case AnimationName.walk://����
                myAnimator.SetBool("Run", false);
                myAnimator.SetBool("Walk", true);//�����OFF
                return true;

            case AnimationName.run://����
                myAnimator.SetBool("Walk", false);//������OFF
                myAnimator.SetBool("Run", true);
                return true;

            case AnimationName.neutral://�ҋ@
                myAnimator.SetBool("Walk", false);
                myAnimator.SetBool("Run", false);
                return true;

            case AnimationName.order://�I�[�_�[
                myAnimator.SetBool("Order", true);
                move = true;//�ړ��n�A�j���[�V����OFF
                command = true;//�R�}���hON
                return true;

            case AnimationName.register://���W
                myAnimator.SetBool("Cash_Register", true);
                move = true;//�ړ��n�A�j���[�V����OFF
                command = true;//�R�}���hON
                return true;

            case AnimationName.bussing://�o�b�V���O
                myAnimator.SetBool("Bussing", true);
                move = true;//�ړ��n�A�j���[�V����OFF
                command = true;//�R�}���hON
                return true;

            case AnimationName.push://����
                if (!command)//��������Ȃ��ƃg���K�[���Q��Đ�����Ă��܂�
                {
                    myAnimator.SetTrigger("Push");
                    move = true;//�ړ��n�A�j���[�V����OFF
                    command = true;//�R�}���hON
                }
                return true;

            case AnimationName.surprise://���̂𗎂Ƃ��ċ���
                if (!nowHit)
                {
                    myAnimator.SetTrigger("Surprise");
                    move = true;//�ړ��n�A�j���[�V����OFF
                    command = false;//�R�}���hOFF
                    nowHit = true;//������ON
                }
                return true;

            case AnimationName.hit://�������
                if (!nowHit)
                {
                    myAnimator.SetTrigger("Hit");
                    move = true;//�ړ��n�A�j���[�V����OFF
                    command = false;//�R�}���hOFF
                    nowHit = true;//������ON
                }
                return true;
        }
        return false;

    }

    //���̂����Ƃ��̃A�j���[�V�����Đ�
    public bool BringAnimationPlay(BringAnimationName briName)
    {
        switch (briName)
        {
            case BringAnimationName.normal://�ʏ�
                myAnimator.SetBool("Bring1", false);
                myAnimator.SetBool("Bring2", true);
                return true;

            case BringAnimationName.bigCake://�r�b�O�P�[�L
                
                myAnimator.SetBool("Bring1", true);
                myAnimator.SetBool("Bring2", false);
                return true;
        }
        return false;
    }

    #endregion

    #region Stop

    public bool AnimationStop(AnimationName name, bool finish = true)//��~
    {
        switch (name)
        {
            case AnimationName.order://�I�[�_�[
                myAnimator.SetBool("Order", false);
                command = false;//�R�}���hOFF
                move = false;//�ړ��n�A�j���[�V����ON
                return false;

            case AnimationName.register://���W
                myAnimator.SetBool("Cash_Register", false);
                if (finish)
                {
                    myAnimator.SetTrigger("Cash_Register_F");//�g���K�[�ŃI�W�M����
                }
                command = false;//�R�}���hOFF
                move = false;//�ړ��n�A�j���[�V����ON
                return true;

            case AnimationName.bussing://�o�b�V���O
                myAnimator.SetBool("Bussing", false);
                command = false;//�R�}���hOFF
                move = false;//�ړ��n�A�j���[�V����ON
                return true;
        }
        return false;
    }

    //
    public bool BringAnimationStop(BringAnimationName briName)
    {
        switch(briName)
        {
            case BringAnimationName.normal:
                myAnimator.SetBool("Bring1", false);
                myAnimator.SetBool("Bring2", false);
                return false;

            case BringAnimationName.bigCake:
                myAnimator.SetBool("Bring1", false);
                myAnimator.SetBool("Bring2", false);
                return false;
        }
        return false;

    }

    #endregion

    //�g���K�[�A�j���[�V�����p
    public void AnimationFinish()
    {
        command = false;//�R�}���hOFF
        nowHit = false;//������OFF
        move = false;//�ړ��n�A�j���[�V����ON
    }

    //�����ꂽ�� �Ȃǂ������ɁA��ƒ��̃A�j���[�V�������~����
    public void WorkAnimationOFF()
    {
        myAnimator.SetBool("Order", false);
        myAnimator.SetBool("Cash_Register", false);
        myAnimator.SetBool("Bussing", false);
    }


    void Update()
    {

        if (!move && myRigidBody != null && myAnimator != null)//������Ƃ�
        {
            //��������
            if (myRigidBody.velocity.magnitude > playerMoveStartSpeed)
            {
                if(aniName != AnimationName.walk)
                {
                    aniName = AnimationName.walk;
                    AnimationPlay(aniName);
                }
                
            }
            //���锻��(�T�u)
            /*else if (rb.velocity.magnitude > 0   && ����p�t���O )
            {
                aniName = AnimationName.run;
            }*/
            //�ҋ@����
            else if (myRigidBody.velocity.magnitude <= playerMoveStartSpeed)
            {
                if(aniName != AnimationName.neutral)
                {
                    aniName = AnimationName.neutral;
                    AnimationPlay(aniName);
                }
            }
        }
        

        /*
        if (!command)//�R�}���h���ł͂Ȃ��Ƃ�
        {

            //�I�[�_�[�Ƃ�u��
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                aniName = AnimationName.order;
            }

            //���W����u��
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                aniName = AnimationName.register;
            }

            //�o�b�V���O����u��
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                aniName = AnimationName.bussing;
            }

            //�����u��
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                aniName = AnimationName.push;
            }


        }

        if (!nowHit)//�����Ă��Ȃ��Ƃ�
        {
            //���̂𗎂Ƃ��ċ���
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                aniName = AnimationName.surprise;
                WorkAnimationOFF();
            }

            //�������
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                aniName = AnimationName.hit;
                WorkAnimationOFF();
            }

            
            //�ːi�͍��͂Ȃ�(�ꉞ�c���Ƃ��܂�)
            if (Input.GetKey(KeyCode.LeftShift))
            {
                anim.SetBool("Rush", true);
            }
            else
            {
                anim.SetBool("Rush", false);
            }


            //�ːi�����
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                anim.SetTrigger("Rush_Receive");
            }
            

        }

        //���������Ă��Ȃ��Ƃ�
        if(briName == BringAnimationName.neutral)
        {
            //�������u��(�ʏ�)
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                briName = BringAnimationName.normal;
            }

            //�������u��(���`���f�J�P�[�L)
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                briName = BringAnimationName.bigCake;
            }

        }

        //�Đ�
        AnimationPlay(aniName);

        //��~����u��
        if (Input.GetKeyDown(KeyCode.M) && NowAnimationPlayed(aniName))
        {
            //����m�F������true��������~�߂�
            AnimationStop(aniName);
        }

        //�������A�j���[�V�����Đ�
        BringAnimaitonPlay(briName);

        //���������~����
        if (Input.GetKeyDown(KeyCode.N) && nowBringAnimationPlayed(briName))
        {
            BringAnimationStop(briName);
            briName = BringAnimationName.neutral;
        }
        */
    }
}
