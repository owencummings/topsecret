using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Unity.Gameplay.Actions
{
    public abstract class Action : ScriptableObject
    {
        // Hold all actions here?
        public enum ActionId {
            Sidestep,
            Slash
        }

        // It's nice to have these all in one req, causes extra network traffic.
        // Consider modular system to only include some members of struct
        public struct ActionRequestData {
            public ActionId actionId; // from Enum
            public Vector3 position;
            public Vector3 direction;
            public ulong[] TargetIds;
            public bool Choice;
            // Feel free to add more actions as necessary
        }

        [ServerRpc]
        public virtual void ActionServerRpc(ActionRequestData actionData){} 

        public virtual void ActionClientPredict(ActionRequestData actionData){} // Maybe just avoid this for now

        [ClientRpc]
        public virtual void ActionClientRpc(ActionRequestData actionData){}

        public virtual void OnUpdate(){} 

        public virtual void OnFixedUpdate(){}

        public virtual void OnEnd(){}

        public virtual void OnNewAction(ActionRequestData actionData){}

        //add more as necessary
    }
}
