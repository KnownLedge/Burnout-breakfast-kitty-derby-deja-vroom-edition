using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleInfo : MonoBehaviour
{
    [Header("References")]
    public Transform frontSect; //Front section of vehicle, has pipeframe and wheels
    public Transform rearSect; // Same as above, but for the back of the car
    public Transform frontWheels; //FrontWheels
    public Transform rearWheels; // RearWheels

    public Transform cat; //Reference to cat model

    [Header("GameFeel")]
    public float frontTurnRange = 5f;
    public float backTurnRange = 3f;
    public float catTilt = 15f;
    public float frontWheelSpeed = 10f;
    public float backWheelSPeed = 5f;

    //PRACTICAL
    [HideInInspector] public float steerFRotate = 0f;
    [HideInInspector] public float steerBRotate = 0f;
    [HideInInspector] public float tiltRotate = 0f;

}
