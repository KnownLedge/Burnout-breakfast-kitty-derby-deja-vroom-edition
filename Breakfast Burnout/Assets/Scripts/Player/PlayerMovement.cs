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
        if (Input.GetButton("Jump"))
        {
            currentSpeed = acceleration;
        }
        else
        {
            currentSpeed = 0;
        }
        if(Input.GetAxisRaw("Horizontal") != 0)
        {
            int dir = Input.GetAxis("Horizontal") > 0 ? 1 : -1;
            //Get input as either -1 to 1
            float amount = Mathf.Abs(Input.GetAxis("Horizontal"));
            Steer(dir, amount);
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

    }
    private void FixedUpdate()
    {
        //Forward Acceleration
        plrObjRb.AddForce(plrKart.transform.forward * currentSpeed, ForceMode.Acceleration);

        Vector3 hVelocity = plrObjRb.velocity;
        hVelocity.y = 0;
        hVelocity = Vector3.ClampMagnitude(hVelocity, topSpeed);

        hVelocity /= speedDecay; //Halve the velocity, helps for redirecting it effectively

        plrObjRb.velocity = new Vector3(hVelocity.x, plrObjRb.velocity.y, hVelocity.z);

        Quaternion targetRotation = new Quaternion();
        targetRotation =  Quaternion.Euler(new Vector3(0, plrKart.transform.eulerAngles.y + currentRotate, 0));

        plrKart.transform.rotation = Quaternion.Lerp(plrKart.transform.rotation, targetRotation, Time.deltaTime * 5f);
        //Still no idea about the magic 5f number
        
    }

    private void LateUpdate()
    {
        plrKart.transform.position = plrObj.transform.position;
    }

    public void Steer(int direction, float amount)
    {
        rotate = (turnRate *  direction) * amount;
    }

}
