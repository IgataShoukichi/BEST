using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "CustomerGroup", menuName = "CreateCustomerGroup")]

public class CustomerGroup : ScriptableObject //�Q�[���I�u�W�F�N�g�Ɏ��t����K�v���Ȃ��X�N���v�g
{
    //����
    [SerializeField][Header("�c�̊Ǘ��ԍ�(int)")]
    private int customerNumber;//�c�̗l�̊Ǘ��ԍ�
    [SerializeField][Header("�c�̖̂��O(string)")]
    private string customerName;//�c�̗l�̖��O
    [SerializeField][Header("���q����(Customer)")]
    private List<Customer> customerDetail;//���q����P�̂̏��
    [SerializeField][Header("�����܂ł̎���(float)")]
    private float customerOrderTime;//�����܂ł̎���
    [SerializeField][Header("���������Ǘ��ԍ�(int)")]
    private List<int> customerOrderFoodNumber;//�A�C�e���̒�������
    [SerializeField][Header("�H�׏I���܂ł̎���(float)")]
    private float customerEatingEndTime;//�A�C�e��������邩



    //���̃Z�b�g
    public int GetCustomerNumber()
    {
        return customerNumber;
    }
    public string GetCustomerName()
    {
        return customerName;
    }
    public List<Customer> GetCustomerDetail()
    {
        return customerDetail;
    }
    public float GetCustomerOrderTime()
    {
        return customerOrderTime;
    }
    public List<int> GetCustomerOrderFoodNumber()
    {
        return customerOrderFoodNumber;
    }
    public float GetCustomerEatingEndTime()
    {
        return customerEatingEndTime;
    }
}
