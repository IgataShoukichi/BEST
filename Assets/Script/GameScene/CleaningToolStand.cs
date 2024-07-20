using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleaningToolStand : MonoBehaviour
{
    [System.NonSerialized] public bool nowCleaningTool = false;
    GameObject toolObject;
    Vector3 toolLocalPosition;

    void Start()
    {
        foreach(Transform trans in this.transform)
        {
            if(trans.gameObject.tag == "CleaningTool")
            {
                nowCleaningTool = true;
                toolObject = trans.gameObject;
                toolLocalPosition = toolObject.transform.localPosition;
                break;
            }
        }
    }

    public GameObject GetCleaningTool()
    {
        if (nowCleaningTool)
        {
            nowCleaningTool = false;
            return toolObject;
        }
        return null;
    }

    public void SetCleaningTool(GameObject tool)
    {
        toolObject = tool;
        toolObject.transform.SetParent(this.gameObject.transform, true);
        toolObject.transform.localPosition = toolLocalPosition;
        toolObject.transform.rotation = transform.rotation;
        nowCleaningTool = true;
    }
}
