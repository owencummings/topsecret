using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomNetworking;

public class RollbackManager : MonoBehaviour
{
    public int tick = 0;
    public int ticksPerSecond = 60;
    public IRollbackable[] rollbackableBehaviours;
    public bool newServerResponse;

    // Singleton pattern
    public static RollbackManager Instance { get; private set; }
    private void Awake() 
    { 
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
        DontDestroyOnLoad(gameObject);
    }

    void FixedUpdate(){
        if (newServerResponse){
            // Run a simulation on the provided

            // So for each object to be calculated in the environment... we need...
            // - Their rbs (with full info) at start
            // - And, inputs that change RBs
            // - Obvious problem is that includes every grid square...
            // - But we can have a separate interface for INPUTBUFFER which rollbackable will inherit from
            // create  new physics scene
        }
        // So what would this look like exactly? We would have distributed buffers on each component attached to this one???
        // And then
    }



}
