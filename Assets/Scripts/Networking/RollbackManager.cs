using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using CustomNetcode;

namespace CustomNetcode{
    public class RollbackManager : MonoBehaviour
    { 
        [System.NonSerialized]
        public bool toRollback = false;
        [System.NonSerialized]
        public int rollbackTick = 0;
        [System.NonSerialized]
        public Dictionary<int, IStateful> rollbackableMap;


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
            rollbackableMap = new Dictionary<int, IStateful>();
        }


        public void rewind(int tick){
            // Consider moving objects to new scene ?
            while (tick < NetworkManager.Singleton.LocalTime.Tick) {
                foreach (var (objectId,rollbackable) in rollbackableMap){
                    rollbackable.RollingBack = true;
                    rollbackable.BufferizeCurrState(tick);
                    rollbackable.AssumeStateAtTick(tick);
                    rollbackable.AssumeInputAtTick(tick);
                    rollbackable.ApplyCurrInputToCurrState();
                    tick += 1;
                }
                Physics.Simulate(NetcodeGlobals.tickRate);
            }
        }

        void FixedUpdate(){
            if (toRollback){
                rewind(rollbackTick);
                toRollback = false;
            }
        }
    }
}

