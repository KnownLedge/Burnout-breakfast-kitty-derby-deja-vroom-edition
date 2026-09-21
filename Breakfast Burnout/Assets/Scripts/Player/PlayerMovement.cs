using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UIElements;
using Unity.Netcode;
//Reference video
//https://www.youtube.com/watch?v=Ki-tWT50cEQ&list=PL1R2qsKCcUCKY1p7URUct96O0dorgQnO6

public class PlayerMovement : NetworkBehaviour
{

    [Header("NetCode")] 
    private bool PLAYING_ONLINE = false;

    public NetworkVariable<int> playerID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> playerKart = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> playerCharacter = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>("Player", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<float> raceStartTimer = new NetworkVariable<float>(3f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    public NetworkVariable<float> raceProgress = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public bool isNetReady = false;
    private bool isNetSpawned = false;
    public bool isGroupReadyH = false;
    //TODO: clean this mess of booleans up
    private NetworkRaceManager raceManager;
    private CheckpointSystem checkpointRef; //Refernce to checkpoint system, only *need* this if playing online


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
    public float turnFix = 0f; //How much the velocity is redirected to players facing direction

    public enum DriftStates { Steering, StartDrift, Drifting };
    [Header("States")]
    public DriftStates state = DriftStates.Steering;

    [Header("DriftHop")]
    public float hopTime; //How long the hop last before gravity kicks in
    public float hopForce;
    public float driftGrav; //Extra gravity while drifting
    public float driftForce; //Force to apply on the kart sideways while drifting


    [Header("Drifting")]
    public float driftPower = 0.75f;
    public float driftInfluencePos = 0f; //How much left/right inputs influence the drift angle drifing into the turn
    public float driftInfluenceNeg = 0f; //How much left/right inputs influence the drift angle drifing away from the turn
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

    public ApplyPlayerSkin skinRef;


    [Header("MISC")]
    public float extraGravity = 9.8f;
    public float groundDist = 3f;
    public bool freeRoam = false; //Skips race start phase if true

    [Header("DEBUG")]
    //SPEED
    [SerializeField] private float currentSpeed;
    [SerializeField] private Vector3 hVelocity; //Horizontal velocity player is moving at
    [SerializeField] private float currentVelocityH; //Current horizontal velocity
    [SerializeField] private float currentVelocityALL; // Current velocity
    [SerializeField] internal Vector3 externalBoost; //Boost applied by external sources, useful for things like conveyors or water streams.
    [SerializeField] private int externalBoostSources; //Amount of objects trying to apply external boost to the player
    [SerializeField] private bool drivingForward; //Whether player last accel input was to drive forward or backward


    //TURNING
    [SerializeField] private float currentRotate;
    [SerializeField] private float rotate = 0;
    [SerializeField] private Vector3 driveForward;

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

    //RACE START
    [SerializeField] private bool raceStarted; //Whether the race is actually active, player shouldn't be able to move until it is.
    [SerializeField] private float raceStartTimerLocal = 3f; //Timer for starting the race, used in singleplayer



    //REFERENCE
    [SerializeField] private Transform reversePointer; //child of turnpointer that faces the opposite direction, for easy reversing force


    void Start()
    {

        PLAYING_ONLINE = NetworkInfo.PLAYING_ONLINE;
        if (freeRoam || !PLAYING_ONLINE) raceStarted = true;
        if (plrObj == null)
        {
            plrObj = transform.Find("PlayerObj").gameObject;
            plrKart = transform.Find("PlayerKart").gameObject;
            kartModel = plrKart.transform.Find("KartModel").gameObject;
        }
        plrObjRb = plrObj.GetComponent<Rigidbody>();
        initScale = kartModel.transform.localScale;
        intendScale = initScale;
        if (PLAYING_ONLINE && !IsOwner)
        {
            CI.Camera.gameObject.SetActive(false);
            audio.volume = 0f; //Stop this player from playing sounds to the owner player
        }

        reversePointer = turnPointer.GetComponentInChildren<Transform>();
        if (PLAYING_ONLINE)
        {
            checkpointRef = GameObject.FindFirstObjectByType<CheckpointSystem>();
        }
    }

    public override void OnNetworkSpawn()
    {

        //isNetSpawned = true;
        raceManager = NetworkManager.Singleton.GetComponent<NetworkRaceManager>();
        raceManager.ReceivePlayer(this);
        if (IsHost)
        {
            raceManager.ReceiveHost(this);
        }
        isNetSpawned = true;
    }

    // Update is called once per frame
    void Update()
    {
        if ((PLAYING_ONLINE && isNetReady == false && isNetSpawned && Input.GetKeyDown(KeyCode.Space))|| Input.GetKeyDown(KeyCode.F)) 
        {
            Debug.Log("LOBBY VAL KART " + LobbyScript.gameColorID);
            Debug.Log("LOBBY VAL CHAR " + LobbyScript.gameIconID);
            if (PLAYING_ONLINE && IsOwner)
            {
                playerCharacter.Value = LobbyScript.gameIconID;
                playerKart.Value = LobbyScript.gameColorID;
                playerName.Value = LobbyScript.gamePlayerName;
                isNetReady = true;
                CallNetReadyServerRpc();
            }


        }
        else if (!PLAYING_ONLINE || IsOwner)
        {
            if (raceStarted)
            {
                //ACCELERATION
                driveForward = reversePointer.forward;
                if (Input.GetButton("Vertical") && Time.timeScale == 1)
                {
                    currentSpeed = acceleration;// * Input.GetAxisRaw("Vertical");
                    if (Input.GetAxisRaw("Vertical") < 0)
                    {
                        drivingForward = false;
                    }
                    else
                    {
                        drivingForward = true;
                        //driveForward = reversePointer.forward;
                    }
                }
                else
                {

                    currentSpeed = 0;
                }
                driveForward = (drivingForward) ? reversePointer.forward : -reversePointer.forward;

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
                    audio.PlayOneShot(SL.driftHop);

                    canHop = false;

                    plrObjRb.AddForce(Vector3.up * hopForce, ForceMode.Impulse);
                    //Drift hop
                    driftDirection = Input.GetAxis("Horizontal") > 0 ? 1 : -1;
                    turnPointer.transform.forward = plrKart.transform.forward;
                    turnPointer.transform.Rotate(new Vector3(0, driftPivot * driftDirection, 0));

                    //VISUAL
                    kartModel.transform.localScale = hopStretch;
                    intendScale = driftSquash;

                }
                else if (Input.GetButtonUp("Jump"))
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
                //Yoinked this from the ground check below to skip having to hit the ground to drift, maybe a bad idea we'll see
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
                        turnPointer.transform.Rotate(new Vector3(0, driftPivot * driftDirection, 0));

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

                RaycastHit groundHit;

                if (Physics.Raycast(plrKart.transform.position, Vector3.down, out groundHit, groundDist))
                {
                    canHop = true;


                }

                if (state == DriftStates.Drifting)
                {
                    float control = Mathf.Abs((Input.GetAxis("Horizontal") / 2) + driftDirection);
                    //If drifting into direction, will be 1.5, if drifting away, will be 0.5
                    if (control >= 1.5)
                    {
                        control += driftInfluencePos;
                    }
                    else if (control <= 0.5f)
                    {
                        control += driftInfluenceNeg;
                    }

                    Steer(driftDirection, control * driftPower);
                    driftCharge += control * Time.deltaTime;
                    //steer with drift change
                }

                currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * 4f);
                //No idea what the magic number is for
                rotate = 0;

                RaycastHit hitOn;
                RaycastHit hitNear;

                Physics.Raycast(plrKart.transform.position, Vector3.down, out hitOn, groundDist + 1.1f);
                Physics.Raycast(plrKart.transform.position, Vector3.down, out hitNear, groundDist + 2.0f);


                kartModel.transform.up = Vector3.Lerp(kartModel.transform.up, hitNear.normal, Time.deltaTime * 8.0f);
                kartModel.transform.Rotate(0, plrKart.transform.eulerAngles.y, 0);
                if (state == DriftStates.Drifting)
                {
                    driftRotate = Mathf.Lerp(driftRotate, visualTurn + (visualIncrement * Input.GetAxisRaw("Horizontal") * driftDirection), Time.deltaTime * 8f);
                    //Lerp the rotation for drifing, adding on extra turn depdning on the direction player is holding relative to the drift
                    kartModel.transform.Rotate(new Vector3(0, driftRotate * -driftDirection, 0));
                    Vector3 camTargetPos = CI.Camera.transform.localPosition;
                    camTargetPos.x = 0;
                    camTargetPos.x += CI.pivotDist * driftDirection;

                    if (CI.twoPivotSpeeds)
                    {
                        CI.pivotSpeed = CI.outPivotSpeed;
                    }

                    CI.Camera.transform.localPosition = Vector3.Lerp(CI.Camera.transform.localPosition, camTargetPos, Time.deltaTime * CI.pivotSpeed);

                }
                else
                {
                    if (CI.twoPivotSpeeds)
                    {
                        CI.pivotSpeed = CI.inPivotSpeed;
                    }
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

                if (Input.GetKeyDown(KeyCode.P))
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
            else
            {
                if (isGroupReadyH)
                {
                    if (IsHost)
                    {
                        raceStartTimer.Value -= Time.deltaTime;
                        if (raceStartTimer.Value <= 0)
                        {
                            //Declare race start to all players
                            raceManager.DeclareRaceStart();
                        }
                    }
                    else if (PLAYING_ONLINE == false)
                    {
                        raceStartTimerLocal -= Time.deltaTime;
                        if (raceStartTimerLocal < 0)
                        {
                            //Let player start driving
                            raceStarted = true;
                        }
                    }
                }
                else { 
                if(IsHost && raceManager.loadedPlayerCount == LobbyScript.expectedPlayers && raceManager.CheckNetReady())
                    {
                        raceManager.DeclareNetReady();
                    }
                
                }
            }
        }           
    }
    private void FixedUpdate()
    {
        if (!PLAYING_ONLINE || IsOwner)
        {
            if (raceStarted)
            {
                hVelocity = plrObjRb.linearVelocity;
                currentVelocityALL = hVelocity.magnitude;
                hVelocity.y = 0;
                currentVelocityH = hVelocity.magnitude;
                if (hVelocity.magnitude < topSpeed)
                {
                    if (currentSpeed > 10)
                    {
                        plrObjRb.AddForce(driveForward * (currentSpeed + boostForce), ForceMode.Acceleration);

                    }
                    else
                    {
                        plrObjRb.AddForce(driveForward * currentSpeed, ForceMode.Acceleration);
                    }
                }

                //Turnpointer faces the same way as player kart, but will be turned when drifting to make turning go at an odd angle
                hVelocity = plrObjRb.linearVelocity;

                hVelocity.y = 0;
                //hVelocity = Vector3.ClampMagnitude(hVelocity, topSpeed);

                //Visual: Making wheel speed match horizontal velocity
                // VI.frontWheels.Rotate(0, hVelocity.magnitude * VI.frontWheelSpeed, 0);
                VI.rearWheels.Rotate(0, hVelocity.magnitude * VI.backWheelSPeed, 0);

                Vector3 forceDir = hVelocity.normalized;
                Vector3 playerDir;
                //if (true)
                //{
                //    playerDir = turnPointer.transform.forward;
                //}
                //else
                //{
                //    playerDir = -turnPointer.transform.forward;
                //}

                playerDir = driveForward;

                Vector3 correctedHVelocity = Vector3.Lerp(forceDir, playerDir, turnFix).normalized * hVelocity.magnitude;

                correctedHVelocity /= speedDecay; //Halve the velocity, helps for redirecting it effectively

                plrObjRb.linearVelocity = new Vector3(correctedHVelocity.x, plrObjRb.linearVelocity.y, correctedHVelocity.z);


                Quaternion targetRotation = new Quaternion();
                targetRotation = Quaternion.Euler(new Vector3(0, plrKart.transform.eulerAngles.y + currentRotate, 0));

                plrKart.transform.rotation = Quaternion.Lerp(plrKart.transform.rotation, targetRotation, Time.deltaTime * 5f);
                //Still no idea about the magic 5f number


                if (state == DriftStates.StartDrift || state == DriftStates.Drifting)
                {
                    hopTimer += Time.deltaTime;


                    plrObjRb.AddForce((turnPointer.transform.right * driftDirection) * driftForce, ForceMode.Acceleration);

                    if (hopTimer > hopTime)
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

                if (boostPower < 0)
                {
                    CI.intendFov = CI.defaultFov;
                }
                else
                {
                    CI.intendFov = CI.boostFov;
                }

                //Camera FOv
                CI.Camera.fieldOfView = Mathf.Lerp(CI.Camera.fieldOfView, CI.intendFov, Time.deltaTime * CI.fovSpeed);

                //External boost source application
                plrObjRb.AddForce(externalBoost, ForceMode.VelocityChange);

                externalBoost = Vector3.zero;
                //Reset the external boost, if the player is still on a boost source, it'll be reapplied next frame, otherwise this will end the boost
                externalBoostSources = 0;
            }
            else
            {

            }
        }
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
           // CI.Camera.fieldOfView = CI.burstFov[2];
        }
        else if (driftCharge > driftRequirements[1])
        {
            boostPower = boostStrengths[1];
            plrObjRb.AddForce(plrKart.transform.forward * boostBursts[1], ForceMode.Impulse);
           // CI.Camera.fieldOfView = CI.burstFov[1];
        }
        else if (driftCharge > driftRequirements[0])
        {
            boostPower = boostStrengths[0];
            plrObjRb.AddForce(plrKart.transform.forward * boostBursts[0], ForceMode.Impulse);
           // CI.Camera.fieldOfView = CI.burstFov[0];
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
        plrObjRb.linearVelocity = Vector3.zero;
        plrObjRb.angularVelocity = Vector3.zero;
        //Reset spin
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Collectable")
        {
            GetCollectable();
        }

    }

    public void GetCollectable()
    {
        audio.PlayOneShot(SL.collectSound);
    }
    //Function for applying a boost to the player that will be constant every frame, intended for streams and conveyors
    public void ApplyExternalBoost(Vector3 boost)
    {
        externalBoostSources += 1;
        externalBoost = (externalBoost + boost) / externalBoostSources;
    }

    public void setRaceProgress(float raceProg)
    {
        raceProgress.Value = raceProg;
    }


    //Network functions go here:

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetPlayerIdServerRpc(int newID)
    {
        playerID.Value = newID;
        SetplayerIdClientRpc(newID);
    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void CallNetReadyServerRpc()
    {//Call to the server that this player is ready to connect
        
        ApplyPlayerSkinClientRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void HostStartRaceServerRpc()
    {//Call to all players that race has started, done through the host
        raceStarted = true;
        StartRaceClientRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetNetReadyServerRpc()
    {
        isNetSpawned = true;
        SetNetReadyClientRpc();
        Debug.Log("SETTING NET SPAWNED THROUGH SERVER RPC");
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void DeclareRaceFinishServerRpc(int finishID)
    {
         ReceiveRaceFinishClientRpc(finishID);
    }

    [ClientRpc]
    private void ReceiveRaceFinishClientRpc(int finishingID)
    {
        if (finishingID == playerID.Value && IsOwner == false)
        {
            checkpointRef.ReceiveOnlineFinish(finishingID);
        }
    }

    [ClientRpc]
    private void SetplayerIdClientRpc(int newID)
    {
       // playerID.Value = newID;
    }
    [ClientRpc]
    public void SetNetReadyClientRpc()
    {
        isNetSpawned = true;
        isGroupReadyH = true;
        raceManager.netReady = true; //Setting this here so its set for all players
        Debug.Log("SETTING NET SPAWNED THROUGH CLIENT RPC");
        ApplyPlayerSkinClientRpc();
    } 
    [ClientRpc]
    public void StartRaceClientRpc()
    {//Lets client start race when received
        raceStarted = true;
        if (IsOwner)
        {
            checkpointRef.receiveSystemPlayer(playerID.Value, plrObj);
        }
        else
        {
            checkpointRef.receiveOnlinePlayer(playerID.Value);
        }
            ApplyPlayerSkinClientRpc();
    }
    [ClientRpc]
    private void ApplyPlayerSkinClientRpc()
    {
        isNetReady = true;
        Debug.Log("KART " + playerKart.Value);
        Debug.Log("CHAR " + playerCharacter.Value);
        skinRef.updateAppearance(playerKart.Value, playerCharacter.Value);
    }

}