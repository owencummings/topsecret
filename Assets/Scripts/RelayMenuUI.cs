using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
 using UnityEngine.SceneManagement;
 

public class RelayMenuUI : MonoBehaviour
{
    public string hostedServerString = "";
    public string joiningServerString = "";
    private bool ready = false;

    void OnGUI(){

        if (GUI.Button(new Rect(10, 10, 200, 50), "Host Server")){
            // TODO: Pre-cancel any existing connections
            try {
                Action <string> callbackAction = (string toChange) => hostedServerString = toChange;
                RelayManager.Instance.CreateRelay(callbackAction);
            } finally {
                ready = true;
            }
        }

        if (GUI.Button(new Rect(10, 70, 200, 50), "Join Server")){
            if (joiningServerString != ""){
                try {
                    RelayManager.Instance.JoinRelay(joiningServerString);
                } finally {
                    ready = true;
                }
            }
        }

        // launch game... spawn this button after creating server?
        if (ready){
            if (GUI.Button(new Rect(10, 130, 200, 50), "Launch Game")){
                NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            }
        }

        hostedServerString = GUI.TextField(new Rect(280, 10, 200, 50), hostedServerString);
        joiningServerString = GUI.TextField(new Rect(280, 70, 200, 50), joiningServerString);
    }
}
