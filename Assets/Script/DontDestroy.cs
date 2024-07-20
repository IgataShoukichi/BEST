using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{

    #region Singleton //�܂肽���߂�

    private static DontDestroy instance;

    public static DontDestroy Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (DontDestroy)FindObjectOfType(typeof(DontDestroy));

                if (instance == null)
                {
                    Debug.LogError(typeof(DontDestroy) + "is nothing");
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

    [SerializeField] GameObject tapPanel;

    void Start()
    {
        GameVariable.nowApplicationFocus = Application.isFocused;//���݃t�H�[�J�X����Ă��邩
        tapPanel.SetActive(!GameVariable.nowApplicationFocus);
    }

    void OnApplicationFocus(bool focus)//�t�H�[�J�X�̍X�V
    {
        GameVariable.nowApplicationFocus = focus;
        tapPanel.SetActive(!GameVariable.nowApplicationFocus);
    }

    /*
    void Update()
    {
        
    }
    */
}
