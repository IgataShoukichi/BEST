using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSpot : MonoBehaviour
{
    [System.NonSerialized]public bool nowUse = false;//現在使っているか
    void Start()
    {
        nowUse = false;
    }
}
