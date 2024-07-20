using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "Food", menuName = "CreateFood")]

public class Food : ScriptableObject //�Q�[���I�u�W�F�N�g�Ɏ��t����K�v���Ȃ��X�N���v�g
{
    //����
    [SerializeField][Header("�Ǘ��ԍ�(int)")]
    private int foodNumber;//�A�C�e���̊Ǘ��ԍ�
    [SerializeField][Header("���O(string)")]
    private string foodName;//�A�C�e���̖��O
    [SerializeField][Header("3D���f��(GameObject)")]
    private GameObject foodModel;//�A�C�e���̃I�u�W�F�N�g
    [SerializeField][Header("UI�p�̃��f���摜(Sprite)")]
    private Sprite foodSprite;//�A�C�e���̉摜
    [SerializeField][Header("�F(Color32)")]
    private Color32 foodColor;//�A�C�e���̐F
    [SerializeField][Header("��������(int:�b)")]
    private int foodTime;//�A�C�e���̒�������
    [SerializeField][Header("�킪����邩(bool)")]
    private bool foodBreak;//�A�C�e��������邩
    [SerializeField][Header("�傫��������(bool)")]
    private bool foodBig;//�傫��������



    //���̃Z�b�g
    public int GetFoodNumber()
    {
        return foodNumber;
    }
    public string GetFoodName()
    {
        return foodName;
    }
    public GameObject GetFoodModel()
    {
        return foodModel;
    }
    public Sprite GetFoodSprite()
    {
        return foodSprite;
    }
    public Color32 GetFoodColor()
    {
        return foodColor;
    }
    public int GetFoodTime()
    {
        return foodTime;
    }
    public bool GetFoodBrake()
    {
        return foodBreak;
    }
    public bool GetFoodBig()
    {
        return foodBig;
    }
}
