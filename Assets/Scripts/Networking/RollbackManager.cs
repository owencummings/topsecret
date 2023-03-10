using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomNetcode;

// TODO if necessary... let's try it client driven and 
public class RollbackManager : MonoBehaviour
{ 
    public static int tick = 0;
    public static int ticksPerSecond = 60;
    public float tickRate = (float) tick / (float) ticksPerSecond;
    public Dictionary<int, IList> idToStateBufferMap;
    public Dictionary<int, IList> idToInputBufferMap;
    public HashSet<int> rollbackableIds;

    // What is the process for non-rollbackables?
    // Do the buffers go here or in each rollbackable?

    // Statefuls need state buffers
    // Inputfuls need input buffers
    // rollbackables need to be able to assume a provided state instantly (transition to it over time with one input)
    
    // Process is...
    // Component created with RollbackableHelper
    // -> Helper creates a copy in parallel game scene with same state
    // ---> Probably need to add some extra stuff for rigidbody/transform/animator information
    // -> enter components to parallelMap and idToComponentMap
    // -> add to rollbackableIds if necessary 
    // -> add buffers to buffer maps. it isnt automatic.
    // !! instantiate a helper object within each rollbackable component
    // ?? how to deal with adding components to objects?
    // ---> Each component can Reference a particular state string to effect...
    // ---> so maybe like map(objectId, map(componentEnum, Component/Input+StateBuffer)) ... some kind of structure like that
    // ?? I guess we instead make components with IRollbackable with instead? Well..
    // Would need to have some kind of rollbackable 

    // Object is updated via input
    // -> pass input to input buffer 

    // Object is updated via sync

    // Component is removed

    // It would be ideal to map to the rollback component instead? A little annoying but... that's fine.
    // How to separate rollback targets from 
    /*
    So normally I would do...
    playerStateBuffer=x
    inputBuffer=y
    gameStateBuffer=z

    // Recreate scene or physics scene around 
    */

    // OK, we need to do the resimulation in-scene? hmmm... OK. Lets


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
