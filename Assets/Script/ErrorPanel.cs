using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ErrorPanel : MonoBehaviour
{

    #region Instance

    private static ErrorPanel instance;

    public static ErrorPanel Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (ErrorPanel)FindObjectOfType(typeof(ErrorPanel));

                if (instance == null)
                {
                    Debug.LogError(typeof(ErrorPanel) + "is nothing");
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


    [SerializeField] CanvasGroup baseObject;
    [SerializeField] Text errorText;


    void Start()
    {
        baseObject.alpha = 0f;
        baseObject.gameObject.SetActive(false);
    }

    public void ErrorOpen(string text,float time)
    {
        errorText.text = text;
        baseObject.gameObject.SetActive(true);
        baseObject.DOFade(1, 0.1f).SetEase(Ease.Linear);
        Invoke(nameof(Close), time);
    }
    void Close()
    {
        baseObject.DOFade(0, 0.1f).SetEase(Ease.Linear)
            .OnComplete(() => baseObject.gameObject.SetActive(false));
    }
}
