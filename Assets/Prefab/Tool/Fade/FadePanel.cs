using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadePanel : MonoBehaviour
{
	[SerializeField] GameObject loading;
	public bool nowFadeState = false;
	Coroutine oldCoroutine = null;

	float waitSecond = 0.01f;
	float fadeSpeed = 0.1f;        //�����x���ς��X�s�[�h
	float red, green, blue,alfa;   //�F�A�s�����x

	Image fadePanelImage;

	#region Singleton //�܂肽���߂�

	private static FadePanel instance;

	public static FadePanel Instance
	{
		get
		{
			if (instance == null)
			{
				instance = (FadePanel)FindObjectOfType(typeof(FadePanel));

				if (instance == null)
				{
					Debug.LogError(typeof(FadePanel) + "is nothing");
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
	void Start()
	{
		DontDestroyOnLoad(transform.root.gameObject);
		fadePanelImage = this.GetComponent<Image>();
		red = fadePanelImage.color.r;//��
		green = fadePanelImage.color.g;// ��
		blue = fadePanelImage.color.b;//��
		alfa = fadePanelImage.color.a;//�����x
		fadePanelImage.enabled = false;
		loading.SetActive(false);
	}

    #region FadeSelect
    /// <summary>
    /// �ʏ�t�F�[�h���[�h
    /// </summary>
    /// <param name="selectBool">(bool) true.FadeOut false.FadeIn</param>
    public void NormalFadeMode(bool selectBool)
    {
        if (selectBool)
        {
			StartCoroutine(FadeOut());
		}
        else
        {
			StartCoroutine(FadeIn());
		}
    }

	/// <summary>
	/// �I�[�g�V�[���t�F�[�h���[�h
	/// </summary>
	/// <param name="selectSceneName">(string) SceneName</param>
	/// <param name="delay">(float) StartDelay</param>
	public void AutoSceneFadeMode(string selectSceneName,float delay = 0)
    {
        if(oldCoroutine != null)
		{
			StopCoroutine(oldCoroutine);
		}
		loading.SetActive(false);
		oldCoroutine = StartCoroutine(FadeSceneChange(selectSceneName,delay));
    }

	/// <summary>
	/// �}�j���A���V�[���t�F�[�h���[�h
	/// </summary>
	/// <param name="selectMode">(bool) true.FadeOut false.FadeIn</param>
	/// <param name="selectSceneName">(string) SceneName</param>
	/// <param name="delay">(float) StartDelay</param>
	public void ManualSceneFadeMode(bool selectMode, string selectSceneName = "", float delay = 0,bool loadingPanel = false)
    {
		if (selectMode)
		{
			StartCoroutine(FadeOut(true,selectSceneName,delay,loadingPanel));
		}
		else
		{
			StartCoroutine(FadeIn());
		}
	}

	#endregion

	IEnumerator FadeOut(bool SceneChange = false, string sceneName = "",float delay = 0.0f,bool loadingPanel = false)
	{
		nowFadeState = true;
		fadePanelImage.enabled = true;
		alfa = 0;
		SetAlpha();
		yield return new WaitForSeconds(delay);
		while (nowFadeState)
		{
			yield return new WaitForSeconds(waitSecond);
			alfa += fadeSpeed;
			SetAlpha();
			if (alfa >= 1)
			{
				nowFadeState = false;
				if (SceneChange)
				{
					if (loadingPanel)
                    {
						loading.SetActive(true);
					}
					SceneManager.LoadScene(sceneName);
				}
			}
		}
	}

	IEnumerator FadeIn()
	{
		fadePanelImage.enabled = true;
		alfa = 1;
		SetAlpha();
		nowFadeState = true;
		loading.SetActive(false);
		while (nowFadeState)
		{
			yield return new WaitForSeconds(waitSecond);
			alfa -= fadeSpeed;
			SetAlpha();
			if (alfa <= 0)
			{
				fadePanelImage.enabled = false;
				nowFadeState = false;
			}

		}
	}

	IEnumerator FadeSceneChange(string sceneName,float delay)
    {
		nowFadeState = true;
		fadePanelImage.enabled = true;
		yield return StartCoroutine(FadeOut(true, sceneName,delay));
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(FadeIn());
	}
	void SetAlpha()
	{
		fadePanelImage.color = new Color(red, green, blue, alfa);
	}
}