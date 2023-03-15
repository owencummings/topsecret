using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using CustomNetcode;

namespace CustomNetcode {

    public class NetcodeGlobals {
        public const int bufferSize = 512;
        public const int ticksPerSecond = 60;
        public const float tickRate = 1f / (float) ticksPerSecond;
        public static int currTick = 0;
    } 

    // process is...
    // ON CLIENT TICK:
    // - Get state after physics, bufferize state (currState)
    // - Apply input to state (currInput, currState)
    // - Bufferize Input (currInput)
    // - Send input to server (currInput)

    // ON SERVER RPC:
    // - recieve currInput 
    // - Send state to all clients (currState)
    // - Apply input (currInput, currState)

    // ON CLIENT RPC:
    // - If owner:
    // - - check if state matches buffered state (recievedState, bufferedState)
    // - - - if not good, ()
    // - - - 
    // - else:
    // - - apply state

    // ON SERVER TICK:
    // - Maybe add input prediction for missing client packets 
    // - (repeating last input for button holds, probably a better way to do this as just in-state management)

    public interface IStateful {
        bool RollingBack { get; set; }
        int InstanceId{ get; }
        void ApplyCurrInputToCurrState();
        void AssumeCurrState();
        void AssumeStateAtTick(int tick); // Generally calling assumeState
        void AssumeInputAtTick(int tick); // Generally calling ApplyCurrInputToCurrState
        void BufferizeCurrState(int tick);
        void SubscribeToRollbackManager(){
            RollbackManager.Instance.rollbackableMap.Add(InstanceId, this);
        }
        void UnsubscribeFromRollbackManager(){
            RollbackManager.Instance.rollbackableMap.Remove(InstanceId);
        }
    }

    public interface IRollbackable : IStateful{
        bool IncorrectSyncState();
        void TriggerRollback(int rollbackTick){
            RollbackManager.Instance.toRollback = true;
            RollbackManager.Instance.rollbackTick = Mathf.Min(rollbackTick, RollbackManager.Instance.rollbackTick);
        }
    }
    
 
}
