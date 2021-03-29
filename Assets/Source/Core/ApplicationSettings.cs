using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ApplicationSettings : MonoBehaviour
{
    public int TargetFrameRate;
    
    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0; // turns vsync off { 0 = off, 1 = normal, 2 = every other }
    }

    // Update is called once per frame
    void Update()
    {
        if (TargetFrameRate != Application.targetFrameRate) 
            Application.targetFrameRate = TargetFrameRate < 0 ? Int32.MaxValue : TargetFrameRate;
    }
}
