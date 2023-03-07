using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomNetworking;

public class RollbackManager : MonoBehaviour
{
    public static int tick = 0;
    public static int ticksPerSecond = 60;
    public float tickRate = (float) tick / (float) ticksPerSecond;
    public List<IList> rollbackableBuffers;
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
            // - a proper collection of buffers of different types
            // - Their rbs (with full info) at start
            // - And, inputs that change RBs
            // - Obvious problem is that includes every grid square...
            // - But we can have a separate interface for INPUTBUFFER which rollbackable will inherit from
            // create  new physics scene


            // Objects should be able to subscribe to the rollback calculations
            // And then, those objects should be able to 'contribute' as they deem necessary
            // Player objects, for instance, are rollbackable, in that then need state and inputs
            //      preserved AND need to update their current state after recalculation
            // Some objects need state and input provided, but don't need to necessarily be rollbackable
            // because they are 'static' (pylons from the maze, though they could be rollbackable)
            // And some objects don't have true input at all? And we wont be tracking in the replay step.
            // What if an object it destroyed client-side when it needs to be rollbacked?
            // Guess we wouldnt do that... or would set come kind of kill-coroutine
            // So yeah we want... INPUTBUFFER, STATEBUFFER, and maybe even a result buffer?
            // All of those 
            // Something to pass on to server saying we were successful on our end... we can handle that later.
            // 
            // How do we subscribe to the rollbackManager? I guess we should use an abstract class
            // With a built-in subscribe function when its initialized


            // So we need...
            // A state format
            // An input format or a state change format, that is replicable 
            // Buffer arrays of each
            // A hookup on awake to the manager (RollbackManager.instance.sync())
            // Something to remove reference on 
            // But the methods that lookup 
            //
        }
        // So what would this look like exactly? We would have distributed buffers on each component attached to this one???
        // And then
    }



}
