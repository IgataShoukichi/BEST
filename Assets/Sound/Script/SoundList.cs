using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundList : MonoBehaviour
{
	#region Singleton //ê‹ÇËÇΩÇΩÇﬂÇÈ

	private static SoundList instance;

	public static SoundList Instance
	{
		get
		{
			if (instance == null)
			{
				instance = (SoundList)FindObjectOfType(typeof(SoundList));

				if (instance == null)
				{
					Debug.LogError(typeof(SoundList) + "is nothing");
				}
			}

			return instance;
		}
	}

	public void Awake()
	{
		if (this != Instance)
		{
			Destroy(this.gameObject);
			return;
		}

		DontDestroyOnLoad(this.gameObject);
	}
	#endregion Singleton

	[SerializeField] SoundDataBase soundDataBase;
	[SerializeField] AudioSource soundEffectAudioSource;
	[SerializeField] AudioSource BGMAudioSource;

	public enum PlayMode
	{
		Play,
		Stop,
	}


	/// <summary>
	/// å¯â âπ
	/// </summary>
	/// <param name="number">ÉNÉäÉbÉvî‘çÜ</param>
	/// <param name="valume">âπó </param>
	public void SoundEffectPlay(int number,float valume = 1.0f)
    {
		soundEffectAudioSource.PlayOneShot(soundDataBase.GetSoundEffect(number),valume * SaveData.SystemSaveData.seVolume);
    }

	/// <summary>
	/// BGM
	/// </summary>
	/// <param name="playMode">1.Play 2.Stop</param>
	/// <param name="number">BGM Number</param>
	/// <param name="valume">Sound Valume</param>
	public void BGM(PlayMode playMode, int number = 0, float valume = 1)
	{
        switch (playMode)
        {
			case PlayMode.Play:
				BGMAudioSource.clip = soundDataBase.GetBGM(number);
				BGMAudioSource.volume = valume * SaveData.SystemSaveData.bgmVolume;
				BGMAudioSource.Play();
				Debug.Log("BGM Play");
				break;
			case PlayMode.Stop:
				BGMAudioSource.Stop();
				Debug.Log("BGM Stop");
				break;
			default:
				Debug.Log("BGM Select Error");
				break;
		}
		
	}
}
