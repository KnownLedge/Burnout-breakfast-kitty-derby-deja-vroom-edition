using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Reference video
//https://www.youtube.com/watch?v=Ki-tWT50cEQ&list=PL1R2qsKCcUCKY1p7URUct96O0dorgQnO6

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public GameObject plrObj;// Reference to object for gameplay physics and collision
    public GameObject plrKart; //Reference to Kart model holder
    public GameObject kartModel; //Reference to the actual kart model
    public Rigidbody plrObjRb; //Reference to rigidbody for game physics

    [Header("Movement")]
    public float acceleration;
    public float topSpeed;
    public float speedDecay; //How much velcity is divided by each fixed step, used to help redirect turning
    public float turnRate;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float currentRotate;
    private float rotate = 0;
    public float hopForce = 20f;

    [SerializeField] private bool isDrifting = false; //Whether player is drifting or not
    [SerializeField] private bool startDrifting = true; //Whether player is starting to drift or not
    [SerializeField] private int driftDirection = 0; //direction drift goes in
    public float driftStartTime = 0.3f;
    public float driftStartTimer = 0f;
    public float driftPower = 0.75f;
    public float driftCharge = 0f;
    public float[] driftRequirements = { 3, 6, 9 };
    public float[] boostStrengths = { 5, 8, 10 };
    public float boostPower = 0f;
    public float boostForce = 60f;

    public GameObject[] boostSignals;

    public float extraGravity = 9.8f;

    // Start is called before the first frame update
    void Start()
    {
        if(plrObj == null)
        {
            plrObj = transform.Find("PlayerObj").gameObject;
            plrKart = transform.Find("PlayerKart").gameObject;
            kartModel = plrKart.transform.Find("KartModel").gameObject;
        }
        plrObjRb = plrObj.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Vertical"))
        {
            currentSpeed = acceleration * Input.GetAxisRaw("Vertical");
        }
        else
        {
            currentSpeed = 0;
        }
        if(Input.GetAxisRaw("Horizontal") != 0 && !startDrifting)
        {
            int dir = Input.GetAxis("Horizontal") > 0 ? 1 : -1;
            //Get input as either -1 to 1
            float amount = Mathf.Abs(Input.GetAxis("Horizontal"));
            Steer(dir, amount);
        }



        //Drift
        if (Input.GetButtonDown("Jump") && !isDrifting) //Drift Hop
        {
            startDrifting = true;
            //Start state for getting into drift
            driftStartTimer = 0f;
            //Start timer for getting into drift, updates every frame
            plrObjRb.AddForce(plrKart.transform.up * hopForce, ForceMode.Impulse);
            //Drift hop

        }
        else if (Input.GetButtonUp("Jump")) //Release drift
        {
            isDrifting = false;
            startDrifting = false;
            driftStartTimer = 0f;

            if(driftCharge > driftRequirements[2])
            {
                boostPower = boostStrengths[2];
            }else if(driftCharge > driftRequirements[1])
            {
                boostPower = boostStrengths[1];
            }else if(driftCharge > driftRequirements[0])
            {
                boostPower = boostStrengths[0];
            }


                driftCharge = 0f;

           //Reset drift states
        }

        if(driftStartTimer > driftStartTime) //Check for drift state when hop ends
        {
            if (Input.GetAxis("Horizontal") != 0) //Player is moving left/right
            {
                isDrifting = true;
                startDrifting = false;
                driftStartTimer = 0f; //Reset timer to stop if statement retriggering
                driftDirection = Input.GetAxis("Horizontal") > 0 ? 1 : -1;
                //Start drift
                
            }
            else//Player is not moving left/right, we will not drift
            {
                startDrifting = false;
            }
        }

        if (isDrifting) //Player is actively drifting
        {
            float control = Mathf.Abs((Input.GetAxis("Horizontal") / 2) + driftDirection);
            //If drifting into direction, will be 1.5, if drifting away, will be 0.5
            Steer(driftDirection, control * driftPower);
            driftCharge += control * Time.deltaTime;
            //steer with drift change
        }else if (Input.GetButtonUp("Jump")) //Release drift
        {
            isDrifting = false;
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
        

    }
    private void FixedUpdate()
    {
        //Forward Acceleration
        if (boostPower > 0) {
            plrObjRb.AddForce(plrKart.transform.forward * (currentSpeed + boostForce), ForceMode.Acceleration);

        } else
        {
            plrObjRb.AddForce(plrKart.transform.forward * currentSpeed, ForceMode.Acceleration);
        }
           

        Vector3 hVelocity = plrObjRb.velocity;
        hVelocity.y = 0;
        hVelocity = Vector3.ClampMagnitude(hVelocity, topSpeed);

        hVelocity /= speedDecay; //Halve the velocity, helps for redirecting it effectively

        plrObjRb.velocity = new Vector3(hVelocity.x, plrObjRb.velocity.y, hVelocity.z);

        Quaternion targetRotation = new Quaternion();
        targetRotation =  Quaternion.Euler(new Vector3(0, plrKart.transform.eulerAngles.y + currentRotate, 0));

        plrKart.transform.rotation = Quaternion.Lerp(plrKart.transform.rotation, targetRotation, Time.deltaTime * 5f);
        //Still no idea about the magic 5f number

        if (startDrifting)
        {
            driftStartTimer += Time.deltaTime;
        }
        boostPower -= Time.deltaTime;


        //Gravity
        plrObjRb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);



    }

    private void LateUpdate()
    {
        plrKart.transform.position = plrObj.transform.position;
    }

    public void Steer(int direction, float amount)
    {
        rotate = (turnRate *  direction) * amount;
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
