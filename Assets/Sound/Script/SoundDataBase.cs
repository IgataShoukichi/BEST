using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundDataBase", menuName = "CreateSoundDataBase")]
public class SoundDataBase : ScriptableObject
{
    [SerializeField]
    [Header("�������X�g(���ʉ�)")]
    private List<SoundData> soundEffectList = new List<SoundData>();
    [SerializeField]
    [Header("�������X�g(BGM)")]
    private List<SoundData> BGMList = new List<SoundData>();

    public AudioClip GetSoundEffect(int searchNumber)//�S�̂̉���
    {
        return soundEffectList[searchNumber].clip;
    }
    public AudioClip GetBGM(int searchNumber)//���́��d����̉���
    {
        return BGMList[searchNumber].clip;
    }

}

[System.Serializable]
public class SoundData
{
    [Tooltip("�����N���b�v")]
    public AudioClip clip;
    [Tooltip("�N���b�v�̐���")]
    public string information;
}
