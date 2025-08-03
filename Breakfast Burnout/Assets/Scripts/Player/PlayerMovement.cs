using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
//Reference video
//https://www.youtube.com/watch?v=Ki-tWT50cEQ&list=PL1R2qsKCcUCKY1p7URUct96O0dorgQnO6

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public GameObject plrObj;// Reference to object for gameplay physics and collision
    public GameObject plrKart; //Reference to Kart model holder
    public GameObject kartModel; //Reference to the actual kart model
    public GameObject turnPointer; //Reference to object in kartmodel to control where object is actually turning
    private Rigidbody plrObjRb; //Reference to rigidbody for game physics
    public GameObject[] boostSignals;
    public AudioSource audio;
    public SoundList SL; //List of sounds in script

    public VehicleInfo VI; //Reference to vehicle transforms to edit vehicle (Used a struct for visual space sake.)
    public CameraInfo CI; //Same as above, but for the Camera details

    [SerializeField] private UIManager uiManagerReference;

    public GameObject gameHUD;

    [Header("Speed")]
    public float acceleration;
    public float topSpeed;
    public float speedDecay; //How much velcity is divided by each fixed step, used to help redirect turning

    [Header("Turning")]
    public float turnRate;
 
    public enum DriftStates { Steering, StartDrift, Drifting };
    [Header("States")]
    public DriftStates state = DriftStates.Steering;

    [Header("DriftHop")]
    public float hopTime; //How long the hop last before gravity kicks in
    public float hopForce;
    public float driftGrav; //Extra gravity while drifting


    [Header("Drifting")]
    public float driftPower = 0.75f;
    public float driftPivot; //How much the drift changes the turn angle by while drift is active
    public float[] driftRequirements = { 3, 6, 9 };
    public float[] boostStrengths = { 5, 8, 10 };
    public float[] boostBursts = { 5, 8, 10 };
    public float boostForce = 60f;



    [Header("Visual")]

    public float visualTurn = 45f;
    public float visualIncrement = 5f;

    public float rescaleSpeed = 1f;
    public Vector3 hopStretch = new Vector3(1, 1.5f, 1);
    public Vector3 driftSquash = new Vector3(1, 0.8f, 1);



    [Header("MISC")]
    public float extraGravity = 9.8f;
    public float groundDist = 3f;

    [Header("DEBUG")]
    //SPEED
    [SerializeField] private float currentSpeed;

    //TURNING
    [SerializeField] private float currentRotate;
    [SerializeField] private float rotate = 0;

    //DRIFT HOP
    [SerializeField] private bool canHop = true;
    [SerializeField] private float hopTimer;

    //DRIFTING
    [SerializeField] private int driftDirection;
    [SerializeField] private float driftCharge = 0f;
    [SerializeField] private float boostPower = 0f;

    //VISUAL

    [SerializeField] private float driftRotate = 0f;

    [SerializeField] private Vector3 initScale; //Original scale of object
    [SerializeField] private Vector3 intendScale;// Intended scale of object in current time in gameplay


    void Start()
    {
        if (plrObj == null)
        {
            plrObj = transform.Find("PlayerObj").gameObject;
            plrKart = transform.Find("PlayerKart").gameObject;
            kartModel = plrKart.transform.Find("KartModel").gameObject;
        }
        plrObjRb = plrObj.GetComponent<Rigidbody>();
        initScale = kartModel.transform.localScale;
        intendScale = initScale;
    }

    // Update is called once per frame
    void Update()
    {
        //ACCELERATION
        if (Input.GetButton("Vertical") && Time.timeScale == 1)
        {
            currentSpeed = acceleration * Input.GetAxisRaw("Vertical");
        }
        else
        {
            currentSpeed = 0;
        }


        //STEERING
        if (Input.GetAxisRaw("Horizontal") != 0 && state != DriftStates.StartDrift && Time.timeScale == 1)
        {
            int dir = Input.GetAxis("Horizontal") > 0 ? 1 : -1;
            //Get input as either -1 to 1
            float amount = Mathf.Abs(Input.GetAxis("Horizontal"));
            Steer(dir, amount);


        }

        //Visual
        VI.steerFRotate = Mathf.Lerp(VI.steerFRotate, VI.frontTurnRange * Input.GetAxisRaw("Horizontal"), Time.deltaTime * 8f);
        VI.frontSect.rotation = new Quaternion();
        VI.frontSect.Rotate(0, VI.steerFRotate, 0);
        VI.steerBRotate = Mathf.Lerp(VI.steerBRotate, VI.backTurnRange * Input.GetAxisRaw("Horizontal"), Time.deltaTime * 8f);
        VI.rearSect.rotation = new Quaternion();
        VI.rearSect.Rotate(0, VI.steerBRotate, 0);
        VI.tiltRotate = Mathf.Lerp(VI.tiltRotate, VI.catTilt * Input.GetAxisRaw("Horizontal"), Time.deltaTime * 8f);
        VI.cat.rotation = new Quaternion();
        VI.cat.Rotate(0, 0, VI.tiltRotate);


        //DRIFTING
        if (Input.GetButtonDown("Jump") && state == DriftStates.Steering && canHop && Time.timeScale == 1)
        {
            state = DriftStates.StartDrift;
            //Start process of drifting
            hopTimer = 0f;
            //reset timer

            canHop = false;

            plrObjRb.AddForce(Vector3.up * hopForce, ForceMode.Impulse);
            //Drift hop

            //VISUAL
            kartModel.transform.localScale = hopStretch;
            intendScale = driftSquash;

        }else if (Input.GetButtonUp("Jump"))
        {

            audio.Stop();
            if (state == DriftStates.Drifting)
            {
                audio.PlayOneShot(SL.driftEnd);
            }

            state = DriftStates.Steering;
            hopTimer = 0f;
            //Reset timer

            turnPointer.transform.forward = plrKart.transform.forward;
            kartModel.transform.forward = plrKart.transform.forward;



            DriftBoost();
            //Activate DriftBoost

            //Visual
            intendScale = initScale;



        }

        RaycastHit groundHit;

        if(Physics.Raycast(plrKart.transform.position, Vector3.down, out groundHit, groundDist))
        {
            canHop = true;
            if (state == DriftStates.StartDrift && hopTimer > hopTime)
            {
                if (Input.GetAxisRaw("Horizontal") != 0)
                {// If moving left/right, starting a drift and the timer for starting a drift is up
                    state = DriftStates.Drifting;
                    Debug.Log("DRifting now!");
                    //Then start a drift
                    driftDirection = Input.GetAxis("Horizontal") > 0 ? 1 : -1;
                    //Get drift direction to hold throughout drift

                    turnPointer.transform.forward = plrKart.transform.forward;
                    turnPointer.transform.Rotate(new Vector3(0,driftPivot * driftDirection,0));

                    //Visual
                    driftRotate = 0f;
                    audio.PlayOneShot(SL.driftStart);
                    audio.Play();


                }
                else//Direction was not held when drift should start, cancel drift
                {
                    state = DriftStates.Steering;
                    hopTimer = 0f;
                }

            }
            
        }

        if(state == DriftStates.Drifting)
        {
            float control = Mathf.Abs((Input.GetAxis("Horizontal") / 2) + driftDirection);
            //If drifting into direction, will be 1.5, if drifting away, will be 0.5
            Steer(driftDirection, control * driftPower);
            driftCharge += control * Time.deltaTime;
            //steer with drift change
        }

        currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * 4f);
        //No idea what the magic number is for
        rotate = 0;

        RaycastHit hitOn;
        RaycastHit hitNear;

        Physics.Raycast(plrKart.transform.position, Vector3.down, out hitOn, 1.1f);
        Physics.Raycast(plrKart.transform.position, Vector3.down, out hitNear, 2.0f);


        kartModel.transform.up = Vector3.Lerp(kartModel.transform.up, hitNear.normal, Time.deltaTime * 8.0f);
        kartModel.transform.Rotate(0, plrKart.transform.eulerAngles.y, 0);
        if(state == DriftStates.Drifting)
        {
            driftRotate = Mathf.Lerp(driftRotate, visualTurn + (visualIncrement * Input.GetAxisRaw("Horizontal") * driftDirection), Time.deltaTime * 8f);
            //Lerp the rotation for drifing, adding on extra turn depdning on the direction player is holding relative to the drift
            kartModel.transform.Rotate(new Vector3(0, driftRotate * -driftDirection, 0));
            Vector3 camTargetPos = CI.Camera.transform.localPosition;
            camTargetPos.x = 0;
            camTargetPos.x += CI.pivotDist * driftDirection;
            CI.Camera.transform.localPosition = Vector3.Lerp(CI.Camera.transform.localPosition, camTargetPos, Time.deltaTime * CI.pivotSpeed);

        }
        else
        {
            Vector3 camTargetPos = CI.Camera.transform.localPosition;
            camTargetPos.x = 0;
            CI.Camera.transform.localPosition = Vector3.Lerp(CI.Camera.transform.localPosition, camTargetPos, Time.deltaTime * CI.pivotSpeed);
        }


        if (driftCharge > driftRequirements[2])
        {
            boostSignals[2].SetActive(true);
            boostSignals[0].SetActive(false);
            boostSignals[1].SetActive(false);
        }
        else if (driftCharge > driftRequirements[1])
        {
            boostSignals[1].SetActive(true);
            boostSignals[0].SetActive(false);
            boostSignals[2].SetActive(false);
        }
        else if (driftCharge > driftRequirements[0])
        {
            boostSignals[0].SetActive(true);
            boostSignals[1].SetActive(false);
            boostSignals[2].SetActive(false);
        }
        else
        {
            boostSignals[0].SetActive(false);
            boostSignals[1].SetActive(false);
            boostSignals[2].SetActive(false);
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            if (!uiManagerReference.pauseMenuPanel.activeSelf && Time.timeScale == 1)
            {
                Time.timeScale = 0; //Pause time.
                uiManagerReference.pauseMenuPanel.SetActive(true);
                gameHUD.SetActive(false);
}
            else if (uiManagerReference.pauseMenuPanel.activeSelf && Time.timeScale == 0)
            {
                Time.timeScale = 1; //Unpause time.
                uiManagerReference.pauseMenuPanel.SetActive(false);
                gameHUD.SetActive(true);
            }
        }
    }
    private void FixedUpdate()
    {
        if (boostPower > 0)
        {
            plrObjRb.AddForce(turnPointer.transform.forward * (currentSpeed + boostForce), ForceMode.Acceleration);

        }
        else
        {
            plrObjRb.AddForce(turnPointer.transform.forward * currentSpeed, ForceMode.Acceleration);
        }

        //Turnpointer faces the same way as player kart, but will be turned when drifting to make turning go at an odd angle

        Vector3 hVelocity = plrObjRb.velocity;
        hVelocity.y = 0;
        hVelocity = Vector3.ClampMagnitude(hVelocity, topSpeed);

        //Visual: Making wheel speed match horizontal velocity
        VI.frontWheels.Rotate(0, hVelocity.magnitude * VI.frontWheelSpeed, 0);
        VI.rearWheels.Rotate(0, hVelocity.magnitude * VI.backWheelSPeed, 0);



        hVelocity /= speedDecay; //Halve the velocity, helps for redirecting it effectively

        plrObjRb.velocity = new Vector3(hVelocity.x, plrObjRb.velocity.y, hVelocity.z);


        Quaternion targetRotation = new Quaternion();
        targetRotation = Quaternion.Euler(new Vector3(0, plrKart.transform.eulerAngles.y + currentRotate, 0));

        plrKart.transform.rotation = Quaternion.Lerp(plrKart.transform.rotation, targetRotation, Time.deltaTime * 5f);
        //Still no idea about the magic 5f number


        if(state == DriftStates.StartDrift || state == DriftStates.Drifting)
        {
            hopTimer += Time.deltaTime;

            if(hopTimer > hopTime)
            {
                plrObjRb.AddForce(Vector3.down * driftGrav, ForceMode.Acceleration);
                //Apply extra force to keep player to floor while drifting
            }
        }

        boostPower -= Time.deltaTime;

        //Gravity
        plrObjRb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        //Visual squash and stretch
        kartModel.transform.localScale = Vector3.Lerp(kartModel.transform.localScale, intendScale, Time.deltaTime * rescaleSpeed);

        if(boostPower < 0)
        {
            CI.intendFov = CI.defaultFov;
        }
        else
        {
            CI.intendFov = CI.boostFov;
        }

            //Camera FOv
            CI.Camera.fieldOfView = Mathf.Lerp(CI.Camera.fieldOfView, CI.intendFov, Time.deltaTime * CI.fovSpeed);


    }

    private void LateUpdate()
    {
        plrKart.transform.position = plrObj.transform.position;
    }

    public void Steer(int direction, float amount)
    {
        rotate = (turnRate * direction) * amount;
    }


    private void DriftBoost()
    {
        if (driftCharge > driftRequirements[2])
        {
            boostPower = boostStrengths[2];
            plrObjRb.AddForce(plrKart.transform.forward * boostBursts[2], ForceMode.Impulse);
            CI.Camera.fieldOfView = CI.burstFov[2];
        }
        else if (driftCharge > driftRequirements[1])
        {
            boostPower = boostStrengths[1];
            plrObjRb.AddForce(plrKart.transform.forward * boostBursts[1], ForceMode.Impulse);
            CI.Camera.fieldOfView = CI.burstFov[1];
        }
        else if (driftCharge > driftRequirements[0])
        {
            boostPower = boostStrengths[0];
            plrObjRb.AddForce(plrKart.transform.forward * boostBursts[0], ForceMode.Impulse);
            CI.Camera.fieldOfView = CI.burstFov[0];
        }


        driftCharge = 0f;

        //Reset drift states
    }

    public void RespawnStats()
    {
        Debug.Log("stats resetting");
        currentSpeed = 0;
        boostPower = 0;
        driftCharge = 0;
        plrObjRb.velocity = Vector3.zero;
        plrObjRb.angularVelocity = Vector3.zero;
        //Reset spin
    }

}
