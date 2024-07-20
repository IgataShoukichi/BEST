using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] GameObject commandPanel;
    [SerializeField] Text commandNameText;
    [SerializeField] Text longPressText;
    [SerializeField] Image longPressBar;

    void Start()
    {
        commandPanel.gameObject.SetActive(false);
    }

    public void BarActive(bool mode, string commandName = null, bool longPress = false)
    {
        commandPanel.gameObject.SetActive(mode);
        if (mode)
        {
            commandNameText.text = commandName != null ? commandName : commandNameText.text;
            longPressText.enabled = longPress;
            longPressBar.fillAmount = 0f;
        }
    }
    
    public void SetBar(float nowValue,float maxValue)
    {
        longPressBar.fillAmount = (nowValue / maxValue);
    }
}
