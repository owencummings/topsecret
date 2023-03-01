using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class Player : NetworkBehaviour
{
    private GameObject playerCinemachine;
    private GameObject mainCam;
    private Rigidbody rb;
    private Dictionary < KeyCode, Vector3 > keyMap = new Dictionary < KeyCode, Vector3 >{
        {KeyCode.W, Vector3.forward},
        {KeyCode.S, Vector3.back},
        {KeyCode.A, Vector3.left},
        {KeyCode.D, Vector3.right}
    };
    bool toJump = false;
    int layerMask = ~0;
    float speedForce = 20;

    // TODO: put jumping on a different component and have this just be movement.

    [ServerRpc]
    void ApplyRotateInputServerRpc(Quaternion newRotation, ServerRpcParams serverRpcParams = default){
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            var tf = this.gameObject.transform;
            tf.rotation = new Quaternion(tf.rotation.x, newRotation.y, tf.rotation.z, tf.rotation.w);
        }
    }

    [ServerRpc]
    void ApplyMovementInputServerRpc(Vector3 moveVector, ServerRpcParams serverRpcParams = default){
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            rb.AddRelativeForce(moveVector.normalized * speedForce);
        }
    }

    [ServerRpc]
    void ApplyJumpInputServerRpc(ServerRpcParams serverRpcParams = default){
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            toJump = true;
        }
    }


    public override void OnNetworkSpawn()
    {
        rb = this.GetComponent<Rigidbody>();
        playerCinemachine = GameObject.Find("PlayerCam"); // Could do some in-scene renaming here
        mainCam = GameObject.Find("Main Camera");
        if (IsOwner){
            playerCinemachine.GetComponent<PlayerFollow>().SetTarget(this.gameObject);
        }
    }


    void FixedUpdate(){
        if (!IsSpawned){
            return;
        }
        if (IsOwner){
            // Mirror rotation (fine until we need stun states and gradual rotating)
            ApplyRotateInputServerRpc(mainCam.transform.rotation);

            // Pass movement input vector to server.
            Vector3 moveVector = new Vector3();
            foreach(KeyValuePair<KeyCode,Vector3> entry in keyMap){
                if (Input.GetKey(entry.Key)){
                    moveVector += entry.Value;
                }
            }
            if(moveVector != Vector3.zero){
                ApplyMovementInputServerRpc(moveVector);
            }
        }

        if (IsServer){
            if (Physics.Raycast(this.transform.position, Vector3.down, 1f, layerMask)){
                speedForce = 30.0f;
                rb.drag = 0.1f;
                if (toJump){
                    rb.AddForce(Vector3.up * 500);
                    toJump = false;
                }
            } else {
                speedForce = 20.0f;
                rb.drag = 0.005f;
                toJump = false;
            }
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)){
            ApplyJumpInputServerRpc(); // It's OK to do this before a fixed update I imagine?
        }
    }
}
