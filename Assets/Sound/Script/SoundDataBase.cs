using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundDataBase", menuName = "CreateSoundDataBase")]
public class SoundDataBase : ScriptableObject
{
    [SerializeField]
    [Header("音声リスト(効果音)")]
    private List<SoundData> soundEffectList = new List<SoundData>();
    [SerializeField]
    [Header("音声リスト(BGM)")]
    private List<SoundData> BGMList = new List<SoundData>();

    public AudioClip GetSoundEffect(int searchNumber)//全体の音声
    {
        return soundEffectList[searchNumber].clip;
    }
    public AudioClip GetBGM(int searchNumber)//伐採＆薪割りの音声
    {
        return BGMList[searchNumber].clip;
    }

}

[System.Serializable]
public class SoundData
{
    [Tooltip("音声クリップ")]
    public AudioClip clip;
    [Tooltip("クリップの説明")]
    public string information;
}
