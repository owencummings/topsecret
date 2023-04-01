using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;
using CustomNetcode;
using static PlayerBehaviours;

// Handles human input & network messages for player object
public class PlayerInput : NetworkBehaviour
{
    private GameObject playerCinemachine;
    private GameObject mainCam;
    public PlayerState playerState; // Set this in a prefab
    public GameObject playerSimPrefab;
    private Animator animator;
    private Rigidbody rb;
    
    int layerMask = ~0;
    private bool toJump = false;

    #region RPCs
    [ServerRpc]
    void ApplyInputServerRpc(PlayerState.InputPayload input, ServerRpcParams serverRpcParams = default){
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            playerState.currInput = input; // Would this get overwritten during a critical operation?
            playerState.currState = playerState.GenerateState();
            playerState.currState.tick = input.tick;
            SyncStateClientRpc(playerState.currState);

        }
    }

    [ClientRpc]
    void SyncStateClientRpc(PlayerState.StatePayload state){
        if (IsOwner && !IsServer){
            playerState.recievedState = state;
            bool incorrect = playerState.IncorrectSyncState();
            playerState.BufferizeRecievedState(state.tick);
            if (incorrect){
                (playerState as IRollbackable).TriggerRollback(state.tick);
            }
        } else if (!IsServer) {
            state.tick = NetworkManager.Singleton.LocalTime.Tick; // change tick from sent-tick to recieved-tick
            playerState.recievedState = state;
            playerState.BufferizeRecievedState(state.tick);
            playerState.AssumeRecievedState();
        }
    }
    #endregion

    public override void OnNetworkSpawn()
    {
        // Subscribe to rollback handler
        playerCinemachine = GameObject.Find("PlayerCam"); // Could do some in-scene renaming here
        mainCam = GameObject.Find("Main Camera");
        animator = this.GetComponent<Animator>();
        rb = this.GetComponent<Rigidbody>();

        if (IsOwner){
            playerCinemachine.GetComponent<PlayerFollow>().SetTarget(this.gameObject);
        }
    }

    void FixedUpdate(){
        playerState.currState = playerState.GenerateState();
        playerState.BufferizeCurrState(NetworkManager.Singleton.LocalTime.Tick);

        if (!IsSpawned){
            return;
        }

        if (IsOwner){
            playerState.currInput = new PlayerState.InputPayload();
            if (toJump){
                playerState.currInput.jump = true;
            }

            // Mirror rotation of camera... Eventually abstract to some kind of sensitivity + damping factor 
            var newEulerY = mainCam.transform.eulerAngles.y;
            playerState.currInput.rotation = Quaternion.Euler(playerState.transform.eulerAngles.x, newEulerY, playerState.transform.eulerAngles.z);

            // Pass movement input vector to server.
            if (Input.GetKey(KeyCode.W)){
                playerState.currInput.up = true;
            }
            if (Input.GetKey(KeyCode.A)){
                playerState.currInput.left = true;
            }
            if (Input.GetKey(KeyCode.S)){
                playerState.currInput.down = true;
            }
            if (Input.GetKey(KeyCode.D)){
                playerState.currInput.right = true;
            }
        }

        if (IsServer || IsOwner){
            playerState.ApplyCurrInputToCurrState();     
        }

        if (IsOwner){
            playerState.currInput.tick = NetworkManager.Singleton.LocalTime.Tick;
            playerState.inputBuffer[NetworkManager.Singleton.LocalTime.Tick%NetcodeGlobals.bufferSize] = playerState.currInput;
            ApplyInputServerRpc(playerState.currInput);
        }

        toJump = false;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)){
            toJump = true;
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

    }
}
