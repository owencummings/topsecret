using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomNetworking {
    public interface IStateTracked<T>
    {
        static int bufferSize;
        static T[] stateBuffer = new T[bufferSize];

    }


    public interface IRollbackable<T>: IStateTracked<T>
    {

        void applyActionsAndForces();

    }
}
