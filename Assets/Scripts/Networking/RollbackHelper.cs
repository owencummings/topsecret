using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CustomNetworking {
    public abstract class RollbackHelper<T,U> : MonoBehaviour
    {
        private int bufferSize = 1024;
        public bool stateTracked;
        public bool inputTracked;
        public bool rollbacking;


        public T[] stateBuffer;
        public U[] inputBuffer;

        public T serverState;
        public int serverTick;

        public void addToStateBuffer(T state, int tick){
            stateBuffer[tick%bufferSize] = state;
            // Tell manager that, if no frame is earlier, this is the target frame
            // Tell manager to roll back
        }   

        public void addToInputBuffer(U input, int tick){
            inputBuffer[tick%bufferSize] = input;
        }

    

        // Some way of notifying rollbackManager that it is time to rollback

        private void Awake(){
            // subscribe to rollbackManager on changes to serverstate
            // Pass ref of required buffers to the buffer collection on rollbackManager
        }

        private void Destroy(){
            // unsubscribe to rollbackManager
        }



    }
}