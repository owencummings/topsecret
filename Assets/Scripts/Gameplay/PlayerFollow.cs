using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CinemachineFreeLook))]
public class PlayerFollow : MonoBehaviour
{
    public void SetTarget(GameObject target){
        var cam  = this.GetComponent<CinemachineFreeLook>();
        cam.LookAt = target.transform;
        cam.Follow = target.transform;
    }
}
