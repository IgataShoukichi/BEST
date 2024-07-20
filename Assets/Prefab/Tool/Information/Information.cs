using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Information : MonoBehaviour
{
    [SerializeField] 
    GameObject textPanel;
    [SerializeField]
    [Header("ï˚å¸[0.è„  1.â∫  2.âE  3.ç∂]")]
    int presetNumber = 0;

    RectTransform textPanelTransform;
    Vector2 iconSize;
    float iconSizeSpace = 10.0f;
    float nowPosition = 0.0f;

    void Start()
    {
        iconSize = this.gameObject.GetComponent<RectTransform>().sizeDelta;
        textPanelTransform = textPanel.GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        textPanel.GetComponent<RectTransform>().localScale = Vector3.zero;
    }

    public void PanelOpen()
    {
        Debug.Log("InformationOpen");
        PanelAnchorSetting(presetNumber);
        StartCoroutine(Open());
        IEnumerator Open()
        {
            nowPosition = 0.0f;
            while (nowPosition <= 1.1f)
            {
                textPanelTransform.localScale = Vector3.Lerp(textPanelTransform.localScale, Vector3.one, nowPosition);
                nowPosition += 0.2f;
                yield return new WaitForSeconds(0.01f);
            }
        }
    }
    public void PanelClose()
    {
        Debug.Log("InformationClose");
        StartCoroutine(Open());
        IEnumerator Open()
        {
            nowPosition = 0.0f;
            while (nowPosition <= 1.1f)
            {
                textPanelTransform.localScale = Vector3.Lerp(textPanelTransform.localScale, Vector3.zero, nowPosition);
                nowPosition += 0.2f;
                yield return new WaitForSeconds(0.01f);
            }
        }
    }

    void PanelAnchorSetting(int number)//0.è„  1.â∫  2.âE  3.ç∂
    {
        switch (number)
        {
            case 0:
                textPanelTransform.anchorMax = new Vector2(0.5f, 0.0f);
                textPanelTransform.anchorMin = new Vector2(0.5f, 0.0f);
                textPanelTransform.pivot = new Vector2(0.5f, 0.0f);
                textPanelTransform.anchoredPosition = Vector3.up * (iconSize.y + iconSizeSpace);
                break;
            case 1:
                textPanelTransform.anchorMax = new Vector2(0.5f, 1.0f);
                textPanelTransform.anchorMin = new Vector2(0.5f, 1.0f);
                textPanelTransform.pivot = new Vector2(0.5f, 1.0f);
                textPanelTransform.anchoredPosition = Vector3.down * (iconSize.y + iconSizeSpace);
                break;
            case 2:
                textPanelTransform.anchorMax = new Vector2(0.0f, 0.5f);
                textPanelTransform.anchorMin = new Vector2(0.0f, 0.5f);
                textPanelTransform.pivot = new Vector2(0.0f, 0.5f);
                textPanelTransform.anchoredPosition = Vector3.right * (iconSize.x + iconSizeSpace);
                break;
            case 3:
                textPanelTransform.anchorMax = new Vector2(1.0f, 0.5f);
                textPanelTransform.anchorMin = new Vector2(1.0f, 0.5f);
                textPanelTransform.pivot = new Vector2(1.0f, 0.5f);
                textPanelTransform.anchoredPosition = Vector3.left * (iconSize.x + iconSizeSpace);
                break;
        }
    }
}
