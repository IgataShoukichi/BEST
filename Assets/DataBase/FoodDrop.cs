using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodDrop : MonoBehaviour
{
    /*�Z�b�g���@
     * Tray���A�^�b�`
     * decal��Prefab�t�@�C������A�^�b�`
     * colorRGBA�ɉ���̐F�����
     * ����D�������Ɣ������܂�
     */

    private float dropPos;//�I�u�W�F�N�g�̐^���̃I�u�W�F�N�g�̈ʒu���

    public GameObject tray;//�g���C�̃Q�[���I�u�W�F�N�g
    public GameObject decal;//�f�J�[�����ۂ�����I�u�W�F�N�g

    [SerializeField] private Color32 colorRGBA;//����̐F


    void Update()
    {
        //�Ԃ������Ƃ��̔���ivelocity�Ƃ��Ŕ��f�j
        if (Input.GetKeyDown("d"))
        {
            //�^����Ray���΂�
            Ray ray = new Ray(gameObject.transform.position, -gameObject.transform.up);
            foreach (RaycastHit hit in Physics.RaycastAll(ray))
            {
                if (hit.collider.gameObject.tag == "Floor")//��"Floor"�^�O�Ƀ��C������������
                {
                    //���̂��ʒu + ���X�P�[��/2 + 0.01f(�e�N�X�`�������Ԃ�Ȃ��悤�ɂ���)
                    dropPos = hit.collider.gameObject.transform.position.y +
                        hit.collider.gameObject.transform.localScale.y / 2 + 0.01f;

                    //�f�J�[�����I�u�W�F�N�g�̐^���ɐ�������
                    GameObject myDecal = Instantiate(decal, new Vector3(gameObject.transform.position.x, dropPos, gameObject.transform.position.z),
                        Quaternion.EulerRotation(0f, 0f, 0f));

                    //�f�J�[���̐F��ύX
                    myDecal.GetComponent<MeshRenderer>().material.color = colorRGBA;

                    //�������g�ƃg���C���\����
                    this.gameObject.SetActive(false);
                    tray.gameObject.SetActive(false);

                }
            }
        }
    }
}
