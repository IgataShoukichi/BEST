using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerAnimation : MonoBehaviour
{
    Animator animCus;//�����̃A�j���[�^�[

    bool nowSit = false;//�������Ă��邩

    void Start()
    {
        animCus = GetComponent<Animator>();
    }

    public enum AnimationName//enum
    {
        neutralCus,    //�ҋ@
        walkCus,       //����
        sitCus,        //����
        look_menuCus,  //���j���[������
        eat_foodCus,   //������H�ׂ�
        registerCus    //���W�ł�
    }

    //�ŏ���Neutral���Z�b�g
    AnimationName aniName = AnimationName.neutralCus;

    #region Check
    public bool NowAnimationPlayed(AnimationName name)//����m�F(�R�}���h����)
    {
        switch (name)
        {
            /*�ꉞ�c���Ă܂�
            case AnimationName.walkCus:
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("WalkCus"))
                //���O��Animator�̃X�e�[�g���ƈ�v������
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;
            case AnimationName.neutralCus:
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("NeutralCus"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;
            */
            case AnimationName.sitCus://����
                if (animCus.GetCurrentAnimatorStateInfo(0).IsName("Sit_Chair"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

            case AnimationName.look_menuCus://���j���[������
                if (animCus.GetCurrentAnimatorStateInfo(0).IsName("Look_menu"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

            case AnimationName.eat_foodCus://������H�ׂ�
                if (animCus.GetCurrentAnimatorStateInfo(0).IsName("Eat_food"))
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
            case AnimationName.walkCus://����
                animCus.SetBool("WalkCus", true);
                return true;

            case AnimationName.neutralCus://�ҋ@
                animCus.SetBool("WalkCus", false);
                return true;

            case AnimationName.sitCus://����
                animCus.SetBool("SitCus", true);
                animCus.SetBool("Look_menuCus", false);
                animCus.SetBool("Eat_foodCus", false);
                nowSit = true;
                return true;

            case AnimationName.look_menuCus://���j���[������
                animCus.SetBool("Look_menuCus", true);
                animCus.SetBool("Eat_foodCus", false);
                return true;

            case AnimationName.eat_foodCus://������H�ׂ�
                animCus.SetBool("Eat_foodCus", true);
                animCus.SetBool("Look_menuCus", false);
                return true;
        }
        return false;
    }


    #endregion

    #region Stop

    public bool AnimationStop(AnimationName name)//��~
    {
        switch (name)
        {

            case AnimationName.sitCus://����
                animCus.SetBool("SitCus", false);
                nowSit = false;
                return false;

            case AnimationName.look_menuCus://���j���[������
                animCus.SetBool("Look_menuCus", false);
                animCus.SetBool("SitCus", false);
                nowSit = false;

                return false;

            case AnimationName.eat_foodCus://������H�ׂ�
                animCus.SetBool("Eat_foodCus", false);
                animCus.SetBool("SitCus", false);
                nowSit = false;

                return false;
        }
        return false;
    }

    #endregion

    void Update()
    {
        /*
        //����A�N�V�����i�H�ׂ�A���j���[�q���j���̃A�j���[�V���������ɂ��ā�

        //����A�N�V�������ɕ��ʂ̍����Ԃɖ߂��������́A�uaniName�v��sitCus�ɕς��Ă�
        //�H�ׁE���j�����@�̃A�j���[�V������Ԃ͎����ŉ��������悤�ɂȂ��Ă��܂�

        //�A�N�V�������ɕ��ʂ̍�����o�R�����A���̂܂ܗ��������ꍇ�́uAnimationStop�v
        //���Ăяo���Ă�


        if (!nowSit)//�����Ă��Ȃ��Ƃ�
        {
            //��������
            if (Input.GetKey(KeyCode.W))
            {
                aniName = AnimationName.walkCus;
            }
            else//�����Ă��Ȃ��Ƃ�
            {
                aniName = AnimationName.neutralCus;
            }
        }


        //����u��
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            aniName = AnimationName.sitCus;
        }

        //�����Ă��ă��j���[������u��
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            aniName = AnimationName.look_menuCus;
        }

        //�����Ă��ė�����H�ׂ�u��
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            aniName = AnimationName.eat_foodCus;
        }


        //�Đ�
        AnimationPlay(aniName);

        //��~����u��
        if (Input.GetKeyDown(KeyCode.M) && NowAnimationPlayed(aniName))
        {
            //����m�F������true��������~�߂�
            AnimationStop(aniName);
        }
        */
    }
    
}
