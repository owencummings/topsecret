using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        public Dictionary<int, IStateful> originalMap;
        [System.NonSerialized]
        public Dictionary<int, IStateful> ghostMap;
        [System.NonSerialized]
        public Dictionary<int, GameObject> originalObjectMap;
        [System.NonSerialized]
        public Dictionary<int, GameObject> ghostObjectMap;

        private Scene predictionScene;
        private PhysicsScene predictionPhysicsScene;

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
            originalMap = new Dictionary<int, IStateful>();
            ghostMap = new Dictionary<int, IStateful>();
            originalObjectMap = new Dictionary<int, GameObject>();
            ghostObjectMap = new Dictionary<int, GameObject>();
        }


        private void Start(){
            StartCoroutine(LoadParallelSceneAsync());
        }

        public void rewind(int tick){
            // Get buffers from original
            // So, we need a reference to every rollbackGhost
            Debug.Log("Rolling back from " + NetworkManager.Singleton.LocalTime.Tick.ToString()  + " to " + tick.ToString() + "ticks");
            foreach (var (objectId,rollbackable) in originalMap){
                IStateful ghost = ghostMap[objectId];
                ghostObjectMap[objectId].SetActive(true);
                ghost.CopyBuffersOfObject(originalObjectMap[objectId]);
                ghost.AssumeStateAtTick(tick);
            }


            Physics.autoSimulation = false;
            int currTick = tick;
            // same but target ghost instead
            while (currTick <= NetworkManager.Singleton.LocalTime.Tick) {
                foreach (var (objectId,ghost) in ghostMap){
                    ghost.RollingBack = true;
                    ghost.GenerateCurrState();
                    ghost.BufferizeCurrState(currTick);
                    ghost.AssumeInputAtTick(currTick);
                    ghost.ApplyCurrInputToCurrState();
                }
                currTick += 1;
                predictionPhysicsScene.Simulate(NetcodeGlobals.tickRate);
            }

            foreach (var (objectId, rollbackable) in originalMap){
                var ghostObject = ghostObjectMap[objectId];
                rollbackable.CopyBuffersOfObject(ghostObject);
                // v This may only be necessary for rollbackables, not statefuls
                rollbackable.AssumeStateAtTick(NetworkManager.Singleton.LocalTime.Tick);
                ghostObject.SetActive(false);
            }
            Physics.autoSimulation = true;
        }

        void FixedUpdate(){
            if (toRollback){
                toRollback = false;
                rewind(rollbackTick);
            }
        }

        private void OnDestroy(){
            SceneManager.UnloadSceneAsync(predictionScene);
        }


        IEnumerator LoadParallelSceneAsync()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("ParallelGameScene", LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            predictionScene = SceneManager.GetSceneByName("ParallelGameScene");
            predictionPhysicsScene = predictionScene.GetPhysicsScene();
        }

        public void Subscribe(int instanceId, IStateful ogStateful, IStateful ghostStateful, GameObject ogObject, GameObject ghostObject){
            originalMap[instanceId] = ogStateful;
            ghostMap[instanceId] = ghostStateful;
            originalObjectMap[instanceId] = ogObject;
            ghostObjectMap[instanceId] = ghostObject;
        }

        public void Unsubscribe(int instanceId){
            originalMap.Remove(instanceId);
            ghostMap.Remove(instanceId);
            originalObjectMap.Remove(instanceId);
            ghostObjectMap.Remove(instanceId);
        }
    }
}

