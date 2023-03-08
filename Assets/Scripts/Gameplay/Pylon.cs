using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pylon : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
         this.GetComponent<Renderer>().material.color = new Color(0.99f, 0.99f, 0.99f, 1f);
    }
}
