using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Respawn : NetworkBehaviour
{
    Transform tf;

    public override void OnNetworkSpawn(){
        tf = this.gameObject.transform;
    }

    void FixedUpdate()
    {
        if (IsServer){
            if(tf.position.y < -100.0f){
                tf.position = new Vector3(0.0f, 50.0f, 0.0f);
            }
        }
    }
}
