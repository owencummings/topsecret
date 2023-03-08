using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pylon : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
         this.GetComponent<Renderer>().material.color = Color.white;
    }
}
