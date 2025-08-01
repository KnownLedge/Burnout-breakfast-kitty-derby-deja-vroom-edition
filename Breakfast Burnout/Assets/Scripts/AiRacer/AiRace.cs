using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Reference video
//https://www.youtube.com/watch?v=Ki-tWT50cEQ&list=PL1R2qsKCcUCKY1p7URUct96O0dorgQnO6

public class AiRace : MonoBehaviour
{
    [Header("References")]
    public GameObject physObj;// Reference to object for gameplay physics and collision
    public GameObject aiKart; //Reference to Kart model holder
    public GameObject kartModel; //Reference to the actual kart model
    public Rigidbody physObjRb; //Reference to rigidbody for game physics

    [Header("Movement")]
    public float acceleration;
    public float topSpeed;
    public float speedDecay; //How much velcity is divided by each fixed step, used to help redirect turning
    public float turnRate;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float currentRotate;
    private float rotate = 0;

    public float extraGravity = 9.8f;
    public Transform wayPointParent;
    public List<Transform> wayPoints;
    public Transform currentTarget;
    public int targetIndex = 0;

    public int npcRacerIndex; //For use in the checkpoint system: number each npc racer, starting from 0

    // Start is called before the first frame update
    void Start()
    {
        if (physObj == null)
        {
            physObj = transform.Find("PlayerObj").gameObject;
            aiKart = transform.Find("PlayerKart").gameObject;
            kartModel = aiKart.transform.Find("KartModel").gameObject;
        }
        physObjRb = physObj.GetComponent<Rigidbody>();

        currentTarget = wayPoints[0];
    }

    // Update is called once per frame
    void Update()
    {
            currentSpeed = acceleration * 1; //Always accelerate, these lot got their foot stuck on the gas pedal

        //if (Input.GetAxisRaw("Horizontal") != 0 && !startDrifting)
        //{
        //    int dir = Input.GetAxis("Horizontal") > 0 ? 1 : -1;
        //    //Get input as either -1 to 1
        //    float amount = Mathf.Abs(Input.GetAxis("Horizontal"));
        //    Steer(dir, amount);
        //}

        //Gravity
        physObjRb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * 4f);
        //No idea what the magic number is for
        rotate = 0;

        RaycastHit hitOn;
        RaycastHit hitNear;

        Physics.Raycast(aiKart.transform.position, Vector3.down, out hitOn, 1.1f);
        Physics.Raycast(aiKart.transform.position, Vector3.down, out hitNear, 2.0f);


        kartModel.transform.up = Vector3.Lerp(kartModel.transform.up, hitNear.normal, Time.deltaTime * 8.0f);
        kartModel.transform.Rotate(0, aiKart.transform.eulerAngles.y, 0);



    }
    private void FixedUpdate()
    {
        
        physObjRb.AddForce(aiKart.transform.forward * currentSpeed, ForceMode.Acceleration);
        


        Vector3 hVelocity = physObjRb.velocity;
        hVelocity.y = 0;
        hVelocity = Vector3.ClampMagnitude(hVelocity, topSpeed);

        hVelocity /= speedDecay; //Halve the velocity, helps for redirecting it effectively

        physObjRb.velocity = new Vector3(hVelocity.x, physObjRb.velocity.y, hVelocity.z);

        Vector3 targetRotation = (transform.position + currentTarget.position).normalized;
        Vector3 storedEuler = aiKart.transform.eulerAngles;
        aiKart.transform.LookAt(currentTarget.position);

        float lerpTurn = Mathf.LerpAngle(storedEuler.y, aiKart.transform.eulerAngles.y, Time.deltaTime * 5f);
        aiKart.transform.eulerAngles = new Vector3(storedEuler.x, lerpTurn, storedEuler.z);

        if(Vector3.Distance(aiKart.transform.position, currentTarget.transform.position) < DrawLink.reachRadius)
        {
            targetIndex++;
            if (targetIndex + 1 > wayPoints.Count) {
                targetIndex = 0;
            }
            currentTarget = wayPoints[targetIndex];
        }

        //aiKart.transform.rotation = Vector3.Lerp(aiKart.transform.rotation, targetRotation, Time.deltaTime * 5f);
        //Still no idea about the magic 5f number
    }

    private void LateUpdate()
    {
        aiKart.transform.position = physObj.transform.position;
    }


    public void RespawnStats()
    {
        Debug.Log("stats resetting");
        currentSpeed = 0;

        physObjRb.velocity = Vector3.zero;
        physObjRb.angularVelocity = Vector3.zero;
        //Reset spin
    }

}
