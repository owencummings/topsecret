using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Netcode;

public class PlayerAttackManager : NetworkBehaviour
{

    // This whole system could be rehashed... Not a big fan.

    // Could use this to custom-load attack prefabs without individually linking all of them
    /*
    private UnityEngine.Object LoadPrefabFromFile(string filename)
    {
        Debug.Log("Trying to load LevelPrefab from file ("+filename+ ")...");
        var loadedObject = Resources.Load(filename);
        if (loadedObject == null)
        {
            throw new FileNotFoundException("...no file ("+filename+ ") found - please check the configuration");
        }
        return loadedObject;
    }
    */

    // Load prefabs here? You could potentially make this a singleton
    // Could have future loading issues...
    // Also, don't I just want to get these from a network pool?
    public UnityEngine.Object slashPrefab;
    //private GameObject slash;

    void Awake(){
        // For now, we can instantiate one of each object per player... but eventually we want network pooling
        /*
        slash = (GameObject)Instantiate(slashPrefab);
        if (slash.TryGetComponent(out NetworkObject networkObject)){
            networkObject.TrySetParent(transform);
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
        if (IsClient){
            /*
            if (Input.GetKeyDown(KeyCode.R)){
                slash.GetComponent<Slash>().Fire();
            }
            */
        }
    }
}
