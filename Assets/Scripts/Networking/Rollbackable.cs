using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNetworking {
    public class NetworkingVars 
    {
        public const int bufferSize = 1024; 
        public int tick = 0;
        public int serverTick = 0;
        public int ticksPerSecond = 60;
    }

    public interface IStateTracked<T>
    {
        static IList<T> stateBuffer = new T[NetworkingVars.bufferSize];
    }

    public interface IInputTracked<T>
    {
        static IList<T> inputBuffer = new T[NetworkingVars.bufferSize];
    }

    public interface IRollbackable<T1, T2>: IStateTracked<T1>, IInputTracked<T2>
    {

        void applyActionsAndForces();

    }
}
