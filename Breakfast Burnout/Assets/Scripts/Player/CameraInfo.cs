using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInfo : MonoBehaviour
{
    public Camera Camera;
    public float pivotDist = 5f;
    public float pivotSpeed = 4f;
    public bool twoPivotSpeeds = false;
    public float outPivotSpeed = 0.5f;
    public float inPivotSpeed = 2f;

    public float defaultFov = 90f;
    public float boostFov = 100f;
    public float[] burstFov = { 110, 120, 130 };
    public float fovSpeed = 4f;

    //Debug
    public float intendFov;    
}
