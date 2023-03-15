using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;
using CustomNetcode;

public class Player : NetworkBehaviour, IRollbackable
{
    #region declareVars
    private GameObject playerCinemachine;
    private GameObject mainCam;
    private Rigidbody rb;
        
    // Input and state for rollback
    public struct InputPayload : INetworkSerializable
    {
        // Can change to a vector if adding a controller...
        public int tick; // Needed?
        public bool up;
        public bool down;
        public bool left;
        public bool right;
        public bool jump;
        public Quaternion rotation; // Should I track change in rotation with mouse? Or just actual rotation 

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref up);
            serializer.SerializeValue(ref down);
            serializer.SerializeValue(ref left);
            serializer.SerializeValue(ref right);
            serializer.SerializeValue(ref jump);
            serializer.SerializeValue(ref rotation); // Can re-serialize as just the Y eulerangle
        }
    }

    public struct StatePayload : INetworkSerializable
    {
        public int tick;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public int stateEnum;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation); // Can re-serialize as just the Y eulerangle
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref stateEnum);
        }
    }

    private InputPayload currInput;
    private InputPayload recievedInput;
    private StatePayload currState;
    private StatePayload recievedState;
    private StatePayload[] stateBuffer = new StatePayload[NetcodeGlobals.bufferSize];
    private InputPayload[] inputBuffer = new InputPayload[NetcodeGlobals.bufferSize];

    int layerMask = ~0;
    float speedForce = 20;
    private Animator animator; // How to implement with rollback?
    #endregion

    #region rollbackInterfaceMethods
    // Add rollbackable interface logic
    public int instanceId;
    public int InstanceId
    {
        get => instanceId;
    }

    public bool rollingBack = false;
    public bool RollingBack
    {
        get => rollingBack;
        set => rollingBack = value;
    }

    public void AssumeCurrState(){
        transform.position = currState.position;
        transform.rotation = currState.rotation;
        rb.velocity = currState.velocity;
        // State enum tracking eventually
    }

    public void AssumeStateAtTick(int tick){
        currState = stateBuffer[tick % NetcodeGlobals.bufferSize];
        AssumeCurrState();
    }

    public void BufferizeCurrState(int tick){
        stateBuffer[tick % NetcodeGlobals.bufferSize] = currState;
    }

    public StatePayload GenerateState(){
        StatePayload outState = new StatePayload();
        outState.position = transform.position;
        outState.rotation = transform.rotation;
        outState.velocity = rb.velocity;
        // state enum eventually
        return outState;
    }

    public bool IncorrectSyncState(){
        return false;
    }


    public void ApplyCurrInputToCurrState(){
        // Player will have a variety of inputs based on state... make an extended player input pattern that links up here

        // Apply jump and set grounded state...
        if (Physics.Raycast(this.transform.position, Vector3.down, 1f, layerMask)){
            speedForce = 30.0f;
            rb.drag = 0.1f;
            if (currInput.jump){
                rb.AddForce(Vector3.up * 500);
            }
        } else {
            speedForce = 20.0f;
            rb.drag = 0.005f;
        }

        // Set rotation
        transform.rotation = currInput.rotation;

        // Apply movement vector
        Vector3 moveVector = Vector3.zero;
        if (currInput.up){
            moveVector += Vector3.forward;
        }
        if (currInput.down){
            moveVector += Vector3.back;
        }
        if (currInput.left){
            moveVector += Vector3.left;
        }
        if (currInput.right){
            moveVector += Vector3.right;
        }
        rb.AddRelativeForce(moveVector.normalized * speedForce);
    }

    public void AssumeInputAtTick(int tick){
        currInput = inputBuffer[tick % NetcodeGlobals.bufferSize];
    }
    #endregion


    // TODO: put jumping on a different component and have this just be movement.
    #region RPCs
    [ServerRpc]
    void ApplyInputServerRpc(InputPayload input, ServerRpcParams serverRpcParams = default){
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            currInput = input; // Would this get overwritten during a critical operation?
            inputBuffer[NetworkManager.Singleton.LocalTime.Tick%NetcodeGlobals.bufferSize] = input;
        }
    }

    [ClientRpc]
    void SyncStateClientRpc(StatePayload state){
        if (IsOwner){
            if (IncorrectSyncState()){
                (this as IRollbackable).TriggerRollback(state.tick); // Get tick from state?
            }
        } else {
            state.tick = NetworkManager.Singleton.LocalTime.Tick; // turn tick from sent-tick to recieved-tick
            currState = state;
            AssumeCurrState();
            BufferizeCurrState(state.tick);
        }
    }

    #endregion

    public override void OnNetworkSpawn()
    {
        instanceId = GetInstanceID();
        // Subscribe to rollback handler
        rb = this.GetComponent<Rigidbody>();
        animator = this.GetComponent<Animator>();
        playerCinemachine = GameObject.Find("PlayerCam"); // Could do some in-scene renaming here
        mainCam = GameObject.Find("Main Camera");
        if (IsOwner){
            playerCinemachine.GetComponent<PlayerFollow>().SetTarget(this.gameObject);
        }
        if (IsClient){
            (this as IStateful).SubscribeToRollbackManager();
        }
    }

    void FixedUpdate(){
        if (!IsSpawned){
            return;
        }
        if (IsOwner){

            // Mirror rotation of camera (fine for now)
            var newEulerY = mainCam.transform.eulerAngles.y; // Eventually abstract to some kind of sensitivity + damping factor 
            currInput.rotation = Quaternion.Euler(transform.eulerAngles.x, newEulerY, transform.eulerAngles.z);

            // Pass movement input vector to server.
            if (Input.GetKey(KeyCode.W)){
                currInput.up = true;
            }
            if (Input.GetKey(KeyCode.A)){
                currInput.left = true;
            }
            if (Input.GetKey(KeyCode.S)){
                currInput.down = true;
            }
            if (Input.GetKey(KeyCode.D)){
                currInput.right = true;
            }

            currState = GenerateState();
            ApplyCurrInputToCurrState();
            ApplyInputServerRpc(currInput);
            currInput = new InputPayload();
        }


        if (IsServer){
            // ? Just respond to RPCs?
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)){
            currInput.jump = true;
        }

        // Add some kind of animating tracking to rollback? Not these aren't really animation breakpoints.
        if (!Physics.Raycast(this.transform.position, Vector3.down, 2f, layerMask)){
            animator.SetBool("isAirborne", true);
        }
        else {
            animator.SetBool("isAirborne", false);
            animator.SetFloat("speed", rb.velocity.magnitude);
        }
    }

    public override void OnNetworkDespawn(){ // Is there a network-equivalent?
        if (IsClient){
            (this as IStateful).UnsubscribeFromRollbackManager();
        }
    }
}
