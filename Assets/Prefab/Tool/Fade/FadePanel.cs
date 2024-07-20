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
	float fadeSpeed = 0.1f;        //透明度が変わるスピード
	float red, green, blue,alfa;   //色、不透明度

	Image fadePanelImage;

	#region Singleton //折りたためる

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
		red = fadePanelImage.color.r;//赤
		green = fadePanelImage.color.g;// 緑
		blue = fadePanelImage.color.b;//青
		alfa = fadePanelImage.color.a;//透明度
		fadePanelImage.enabled = false;
		loading.SetActive(false);
	}

    #region FadeSelect
    /// <summary>
    /// 通常フェードモード
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
	/// オートシーンフェードモード
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
	/// マニュアルシーンフェードモード
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