using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Respawn : NetworkBehaviour
{
    Transform tf;
    Rigidbody rb;

    public override void OnNetworkSpawn(){
        tf = this.gameObject.transform;
        rb = this.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (IsServer){
            if(tf.position.y < -100.0f){
                tf.position = new Vector3(0.0f, 50.0f, 0.0f);
                rb.velocity = Vector3.zero;
            }
        }
    }
}
