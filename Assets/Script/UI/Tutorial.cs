using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

public class Tutorial : MonoBehaviour
{
    [System.NonSerialized] public UnityEvent onSettingClose = new UnityEvent();//ï¬Ç∂ÇΩéû

    [SerializeField] GameObject tutorialBasePanel;//ëSëÃ
    [SerializeField] PanelSwipe panelSwipe;

    GameActions gameActions;
    GameActions.UIActions uiActions = new GameActions.UIActions();
	enum InputMode
	{
		Null,
		Select
	}
	InputMode nowInputMode = InputMode.Null;



	#region Instance

	private static Tutorial instance;

	public static Tutorial Instance
	{
		get
		{
			if (instance == null)
			{
				instance = (Tutorial)FindObjectOfType(typeof(Tutorial));
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
		
		gameActions = new GameActions();
		gameActions?.Enable();//ActionÇóLå¯âª
		uiActions = gameActions.UI;
	}
	#endregion Singleton

	void Start()
    {
		tutorialBasePanel.SetActive(true);
		tutorialBasePanel.GetComponent<CanvasGroup>().alpha = 0;
		tutorialBasePanel.GetComponent<CanvasGroup>().blocksRaycasts = false;

		nowInputMode = InputMode.Null;
	}

	void Update()
    {
		if (GameVariable.nowApplicationFocus && nowInputMode == InputMode.Select)
		{

			if (uiActions.Left.WasPressedThisFrame())
			{
				panelSwipe.BackNext(false);
			}
			if (uiActions.Right.WasPressedThisFrame())
			{
				panelSwipe.BackNext(true);
			}
			if (uiActions.Menu.WasPressedThisFrame() || uiActions.Back.WasPressedThisFrame())
			{
				Close();
			}
		}
	}


	public void PanelBackSetting(UnityAction action)//ï¬Ç∂ÇΩéûÇÃê›íË
    {
		onSettingClose.RemoveAllListeners();
		onSettingClose.AddListener(action);
	}


	public void Open()
	{
		tutorialBasePanel.GetComponent<CanvasGroup>().alpha = 0f;
		tutorialBasePanel.GetComponent<CanvasGroup>().blocksRaycasts = true;
		tutorialBasePanel.GetComponent<CanvasGroup>().DOKill();
		tutorialBasePanel.GetComponent<CanvasGroup>().DOFade(1, 0.3f).OnComplete(() => nowInputMode = InputMode.Select);
		nowInputMode = InputMode.Select;
		panelSwipe.PanelSelect(0);
	}

	public void Close()
	{
		nowInputMode = InputMode.Null;
		SoundList.Instance.SoundEffectPlay(3, 1.0f);
		tutorialBasePanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
		tutorialBasePanel.GetComponent<CanvasGroup>().DOKill();
		tutorialBasePanel.GetComponent<CanvasGroup>().DOFade(0, 0.3f);
		onSettingClose.Invoke();
	}
}
