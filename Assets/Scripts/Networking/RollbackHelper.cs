using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CustomNetcode {

    public class NetcodeConstants {
        public const int bufferSize = 512;
        public static int tick = 0;
        public static int ticksPerSecond = 60;
        public float tickRate = (float) tick / (float) ticksPerSecond;
    } 

    public interface IRollbackable<T, U> {
        bool correctable{get; set;}
        void applyInputToState(T input, U state);
        void bufferizeInput(T input, T[] inputBuffer); // Do I need to include tick here?
        void bufferizeState(U state, U[] stateBuffer);
        void subscribeToRollbackManager(){
            //Do the thing
        }
        void unsubscribeFromRollbackManager(){
            // Do the thing
        }
        void triggerRewind(){
            // Do the thing
        }
    }

    public abstract class RollbackHelper : MonoBehaviour
    {
        //public IRollbackable connectedComponent;
    }
    
    public class RollbackManager : MonoBehaviour
    { 
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

        // NEED: Link RollbackHelper components
        // --> This is subscribed to by IRollbackable


        public void rewind(int tick){
            // Get all rollbackables
            // Move them to parallel physics scene (pretend this is necessary)
            // Simulate update + physics loop 
            // Move them to normal physics scene
        }
    }
    

    /*
    What are our tools?
    - Interface (with methods/properties) connecting directly to main state component of 

    - Rollback helper component which can use some pre-defined connection to do dumb shit

    - Rollback Manager component to coordinate between them all

    */
}