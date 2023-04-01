using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using CustomNetcode;

public class PlayerState : MonoBehaviour, IRollbackable
{
    #region declareVars
    private Rigidbody rb;
    int layerMask = ~0;
    
    // Input and state for rollback
    public struct InputPayload : INetworkSerializable
    {
        // Can change to a vector if adding a controller...
        public int tick;
        public bool up;
        public bool down;
        public bool left;
        public bool right;
        public bool jump;
        public Quaternion rotation; // Should I track change in rotation with mouse? Or just actual rotation 

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
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

    public InputPayload currInput;
    public InputPayload recievedInput;
    public StatePayload currState;
    public StatePayload recievedState;
    public StatePayload[] stateBuffer = new StatePayload[NetcodeGlobals.bufferSize];
    public InputPayload[] inputBuffer = new InputPayload[NetcodeGlobals.bufferSize];

    float speedForce = 20;

    #endregion

    #region rollbackInterfaceMethods
    // Add rollbackable interface logic
    public bool rollingBack = false;
    public bool RollingBack
    {
        get => rollingBack;
        set => rollingBack = value;
    }

    public void CopyBuffersOfObject(GameObject matchingObj){
        PlayerState matchingState = matchingObj.GetComponent<PlayerState>();
        if (matchingState){
            stateBuffer = matchingState.stateBuffer;
            inputBuffer = matchingState.inputBuffer;
        }
    }

    public void AssumeCurrState(){
        rb.position = currState.position;
        rb.rotation = currState.rotation;
        rb.velocity = currState.velocity;
        Debug.Log("Assume received at " + NetworkManager.Singleton.LocalTime.Tick.ToString());
        // State enum tracking eventually
    }

    public void AssumeRecievedState(){
        rb.position = recievedState.position;
        rb.rotation = recievedState.rotation;
        rb.velocity = recievedState.velocity;

        // State enum tracking eventually
    }

    public void AssumeStateAtTick(int tick){
        currState = stateBuffer[tick % NetcodeGlobals.bufferSize];
        AssumeCurrState();
    }

    public void BufferizeCurrState(int tick){
        stateBuffer[tick % NetcodeGlobals.bufferSize] = currState;
    }

    public void BufferizeRecievedState(int tick){
        stateBuffer[tick % NetcodeGlobals.bufferSize] = recievedState;
    }

    public void GenerateCurrState(){
        currState.position = rb.position;
        currState.rotation = rb.rotation;
        currState.velocity = rb.velocity;
    }

    public StatePayload GenerateState(){
        StatePayload outState = new StatePayload();
        outState.position = transform.position;
        outState.rotation = transform.rotation;
        outState.velocity = rb.velocity;
        outState.tick = NetworkManager.Singleton.LocalTime.Tick;
        // state enum eventually
        return outState;
    }

    public bool IncorrectSyncState(){
        bool incorrect = (Vector3.Distance(recievedState.position, stateBuffer[recievedState.tick % NetcodeGlobals.bufferSize].position) > .1f);
        if (incorrect){
            Debug.Log("Received: " + recievedState.position.ToString() + "\n" + "Expected: " + stateBuffer[recievedState.tick % NetcodeGlobals.bufferSize].position.ToString()
                    + "\n" + "Previous: " + stateBuffer[(recievedState.tick-1)% NetcodeGlobals.bufferSize].position.ToString()
                     + stateBuffer[(recievedState.tick-1)% NetcodeGlobals.bufferSize].velocity.ToString()
                    + "\n" + "PreviousIn: " + inputBuffer[(recievedState.tick-1)% NetcodeGlobals.bufferSize].up.ToString()
                     + inputBuffer[(recievedState.tick-1)% NetcodeGlobals.bufferSize].down.ToString()
                      + inputBuffer[(recievedState.tick-1)% NetcodeGlobals.bufferSize].left.ToString()
                       + inputBuffer[(recievedState.tick-1)% NetcodeGlobals.bufferSize].right.ToString());
        }
        return (Vector3.Distance(recievedState.position, stateBuffer[recievedState.tick % NetcodeGlobals.bufferSize].position) > .1f);
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

        PlayerBehaviours.RunAggregateBehaviours(transform, rb);
    }

    public void AssumeInputAtTick(int tick){
        currInput = inputBuffer[tick % NetcodeGlobals.bufferSize];
    }
    #endregion

    void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
    }

}
