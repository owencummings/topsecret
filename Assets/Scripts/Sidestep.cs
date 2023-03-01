using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class Sidestep : NetworkBehaviour
{
    Rigidbody rb;
    int cd = 180; // in ticks ... probably best just doing this in seconds
    int currCd = 0;
    //int powerLevel = 1; // Can increment this to increase jump power


    [ServerRpc]
    void SidestepServerRpc(bool left, ServerRpcParams serverRpcParams = default){

        var clientId = serverRpcParams.Receive.SenderClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            if (currCd == 0)
            {
                var xMovement = 1000.0f;
                if (left){
                    xMovement *= -1;
                }
                var moveVector = new Vector3(xMovement, 400.0f, 0.0f);
                rb.AddRelativeForce(moveVector);
                currCd = cd;
            }
        }
    }


    public override void OnNetworkSpawn()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (IsOwner){
            if (Input.GetKeyDown(KeyCode.Q)) 
            {
                SidestepServerRpc(true);
            } 
            else if (Input.GetKeyDown(KeyCode.E)) 
            {
                SidestepServerRpc(false);
            }
        }
    }

    void FixedUpdate()
    {
        // Should this be in some kind of time-delayed event?
        if (IsServer)
        {
            if (currCd > 0)
            {
                currCd -= 1;
            }
        }
    }
}
