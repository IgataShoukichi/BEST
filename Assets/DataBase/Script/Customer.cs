using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "Customer", menuName = "CreateCustomer")]

public class Customer : ScriptableObject //�Q�[���I�u�W�F�N�g�Ɏ��t����K�v���Ȃ��X�N���v�g
{
    //����
    [SerializeField][Header("3D���f��(GameObject)")]
    private GameObject customerModels;//���f��
    [SerializeField][Header("��e�N�X�`���[(Texture2D)")]
    private List<Texture2D> customerFace;//���f��


    //���̃Z�b�g
    public GameObject GetCustomerModels()
    {
        return customerModels;
    }
    public List<Texture2D> GetCustomerFace()
    {
        return customerFace;
        
    }
}
