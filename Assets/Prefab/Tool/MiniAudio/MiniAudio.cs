using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniAudio : MonoBehaviour
{
    [SerializeField] List<AudioClip> audioList = new List<AudioClip>();
    [SerializeField] AudioSource audioSource;


    public void SoundPlay(int number, float valume = 1.0f)
    {
        audioSource.PlayOneShot(audioList[number], valume * SaveData.SystemSaveData.seVolume);
    }
}
