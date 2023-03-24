using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerBehaviours
{
    // Could probably have this be a partial class and spread behaviors across many files as-needed.
    public static bool toRespawn = true; // need non-static conditions object for each player I guess

    public static void Respawn(Transform tf , Rigidbody rb){
        if(tf.position.y < -100.0f){
            tf.position = new Vector3(0.0f, 50.0f, 0.0f);
            rb.velocity = Vector3.zero;
        }
    }

    public static void RunAggregateBehaviours(Transform tf , Rigidbody rb){
        if (toRespawn){
            Respawn(tf, rb);
        }
    } 
}
