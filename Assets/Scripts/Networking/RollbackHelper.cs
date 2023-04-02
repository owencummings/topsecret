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

    public interface IStateful {
        bool RollingBack { get; set; }
        // Consider reworking to reduce the implementation load for the IStateful.
        // Really only ApplyCurrInputToCurrState() and GenerateCurrState() should need to be rewritten.
        void ApplyCurrInputToCurrState();
        void AssumeCurrState();
        void AssumeStateAtTick(int tick); // Generally calling assumeState
        void AssumeInputAtTick(int tick); // Generally calling ApplyCurrInputToCurrState
        void BlendToStateAtTick(int tick);
        void CopyBuffersOfObject(GameObject matchingObj);
        void BufferizeCurrState(int tick);
        void GenerateCurrState();
    }

    public interface IRollbackable : IStateful{
        bool IncorrectSyncState();
        void TriggerRollback(int toTick){ // Move to Manager? probably.
            if (!RollbackManager.Instance.toRollback) {
                RollbackManager.Instance.rollbackTick = toTick;
            }
            RollbackManager.Instance.toRollback = true;
            RollbackManager.Instance.rollbackTick = toTick;
        }
    }
}
