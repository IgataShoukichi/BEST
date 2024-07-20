using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameVariable
{
    ////////////////////////////////////////////
    //�ݒ�ύX

    public static int maxRegisterWaitingCustomers = 3;//�ő僌�W�҂����ł��邨�q����̐�
    public static int set3DUILayer = 10;//3DUI�̃��C���[

    public static readonly string[] skinName = new string[]
    {
        "�m�[�}��1",
        "�m�[�}��2",
        "�m�[�}��3",
        "�m�[�}��4",
        "�m�[�}��5",
        "�m�[�}��6",
        "�m�[�}��7",
        "�m�[�}��8",
        "�m�[�}��9",
        "�m�[�}��10",
        "�X��",
        "�o�C�g1",
        "�o�C�g2",
        "�o�C�g3",
        "�o�C�g4",
        "�o�C�g5",
        "�ԋp�}��",
        "�J�I�X(�O���[��)",
        "�J�I�X(�u���[)",
        "�J�I�X(���b�h)"
    };

    ////////////////////////////////////////////

    //Network�Ǘ��֌W
    public static int masterClientActorNumber = -1;//�}�X�^�[�N���C�A���g�̔ԍ�
    public static bool isMasterClient = false;//�������}�X�^�[�N���C�A���g��
    public static bool nowApplicationFocus = false;//��ʂ�I�����Ă��邩(���Z�b�g�Ȃ�)

    //�f�[�^�x�[�X
    public static FoodDataBase foodDataBase = null;
    public static CustomerGroupDataBase customerDataBase = null;
    
    //�I�u�W�F�N�g�֌W
    public static Dictionary<int,int> allPlayerSkins = new Dictionary<int,int>();//�S�v���C���[��Skin
    public static List<Transform> allPlayerTransform = new List<Transform>();//�S�v���C���[�̃g�����X�t�H�[�� 
    public static List<GameObject> foodObjectList = new List<GameObject>();//�H�ו����X�g
    public static List<GameObject> foodStandList = new List<GameObject>();//�H�ו��o���X�^���h���X�g
    public static List<GameObject> tableList = new List<GameObject>();//�e�[�u�����X�g
    public static CashRegister cashRegister = null;//���W
    public static List<GameObject> cleaningToolList = new List<GameObject>();//�|���p��X�g
    public static List<GameObject> waterSpotList = new List<GameObject>();//�E�H�[�^�[�T�[�o�[���X�g

    public static CustomerMovePoint customerMovePoint = null;//���q����̈ړ��|�C���g

    //�Q�[�����֌W
    public static bool gameStarted = false;//�Q�[�����N��������
    public static List<GameObject> orderList = new List<GameObject>();//�������X�g
    public static List<CustomerGroup> customerGroupList = new List<CustomerGroup>();//�Q�[���ɓo�ꂷ��c�̃��X�g
    public static List<int> customerShuffleList = new List<int>();//�V���b�t�����ꂽ���ۂɎg���c�̃��X�g�ԍ�
    public static List<int> nowStandbyCustomerList = new List<int>();//���݂̑҂��Ă���c�̃��X�g�ԍ�
    public static List<Family> nowCustomerScriptList = new List<Family>();//���ݏo�Ă��邨�q����̃X�N���v�g
    public static int foodCount = 0;//�o���������̐�
    public static int customerCount = 0;//�����c�̗l�̐�
    //public static int nowRegisterWaitingCustomers = 0;//���݃��W�҂������Ă��邨�q����̐�
    public static List<Family> nowRegisterReadyCustomers = new List<Family>();//���W�ɍs�����Ƃ��Ă��邨�q����̐�
    public static List<Family> nowRegisterWaitingCustomers = new List<Family>();//���݃��W�҂������Ă��邨�q����̃X�N���v�g
    //public static bool nowRegisterMoveCustomer = false;//���݂��q���񂪃��W�Ɍ����Ĉړ����Ă��邩

    //�Q�[����(�{��֌W)
    public static UnityEvent onAngerChange = new UnityEvent();
    public static int angerGuideCount = 0;
    public static int angerCallCount = 0;
    public static int angerOrderCount = 0;
    public static int angerCashCount = 0;

    //�Q�[�����J�E���g
    public static int guideCustomerResult = 0;//�ē�������
    public static int orderResult = 0;//�����𕷂�����
    public static int carryfoodResult = 0;//�������^�񂾐�
    public static int cashResult = 0;//��v������
    public static int cleanupTableResult = 0;//�ЂÂ�����
    public static int pushResult = 0;//��������


    //�ŏI����
    public static int finalEvaluation = 0;//�ŏI�I�ȕ]��


    ////////////////////////////////////////////
    public static void AngerChangeGuideCount(bool plus)
    {
        angerGuideCount += plus ? 1 : -1;
        onAngerChange.Invoke();
    }
    public static void AngerChangeCallCount(bool plus)
    {
        angerCallCount += plus ? 1 : -1;
        onAngerChange.Invoke();
    }
    public static void AngerChangeOrderCount(bool plus)
    {
        angerOrderCount += plus ? 1 : -1;
        onAngerChange.Invoke();
    }
    public static void AngerChangeCashCount(bool plus)
    {
        angerCashCount += plus ? 1 : -1;
        onAngerChange.Invoke();
    }


    ////////////////////////////////////////////

    public static void Reset()
    {
        masterClientActorNumber = -1;
        isMasterClient = false;

        foodDataBase = null;
        customerDataBase = null;

        allPlayerSkins.Clear();
        allPlayerTransform.Clear();
        foodObjectList.Clear();
        foodStandList.Clear();
        tableList.Clear();
        cashRegister = null;
        cleaningToolList.Clear();
        waterSpotList.Clear();
        customerMovePoint = null;

        orderList.Clear();
        customerGroupList.Clear();
        customerShuffleList.Clear();
        nowStandbyCustomerList.Clear();
        nowCustomerScriptList.Clear();
        foodCount = 0;
        customerCount = 0;
        nowRegisterReadyCustomers.Clear();
        nowRegisterWaitingCustomers.Clear();

        onAngerChange.RemoveAllListeners();
        angerGuideCount = 0;
        angerCallCount = 0;
        angerOrderCount = 0;
        angerCashCount = 0;

        guideCustomerResult = 0;
        orderResult = 0;
        carryfoodResult = 0;
        cashResult = 0;
        cleanupTableResult = 0;
        pushResult = 0;

        finalEvaluation = 0;
    }
}
