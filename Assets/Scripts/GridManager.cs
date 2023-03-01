using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Mathematics;
using UnityEngine;

public class GridManager : NetworkBehaviour
{
    public GameObject prefab;
    private float cubeSize = 15.0f;
    private float cubeMass = 500.0f;
    private int gridSize = 20; // Could make this not a square, of course
    private float[,] staticHeights; // Set this to be the height that each individual pylon should static at
    private float[,] currStaticHeights; // Set this to be the height that each individual pylon should static at
    private GameObject[,] cubes;
    private Rigidbody rb;


    #region movementCoroutines
    // Coroutines for maze movement
    // Not optimal, but this but shouldn't be that expensive
    IEnumerator Wave(float power=1000.0f, float interval=.3f){
        for (int time = 0; time < gridSize*2 + 1; time++){
            for (int i  = 0; i < gridSize; i++){
                for (int j = 0; j < gridSize; j++){
                    if (i + j != time || cubes[i,j] == null){
                        continue;
                    }
                    rb = cubes[i,j].GetComponent<Rigidbody>();
                    if (!(( rb.constraints & RigidbodyConstraints.FreezePositionY ) != RigidbodyConstraints.None)){
                        rb.AddForce(Vector3.up*500*power);
                    }
                }
            }
            yield return new WaitForSeconds(interval);   
        }

    }

    IEnumerator  Snake(float power=5000.0f, float interval=.3f){
        // TODO: create util function to reverse-engineer desired force of push for reaching destination
        // Fire each square in order
        int targetJ;
        for (int i  = 0; i < gridSize; i++){
            for (int j = 0; j < gridSize; j++){
                // Use targetJ to have snake wrap on the same side
                targetJ = j;
                if (i % 2 == 1){
                    targetJ = gridSize - 1 - j;
                }                    
                if (cubes[i,targetJ] == null){
                    continue;
                }
                rb = cubes[i,targetJ].GetComponent<Rigidbody>();
                if (!(( rb.constraints & RigidbodyConstraints.FreezePositionY ) != RigidbodyConstraints.None)){
                    currStaticHeights[i,targetJ] = 0;
                }
                yield return new WaitForSeconds(interval);   
            }
        }
        yield return new WaitForSeconds(1.0f);   
        ResetStaticHeights();
    }

    IEnumerator  Spiral(float power=5000.0f, float interval=.3f){  
        bool[, ] seen = new bool[gridSize, gridSize];
        int[] dr = { 0, 1, 0, -1 };
        int[] dc = { 1, 0, -1, 0 };
        int r = 0, c = 0, di = 0;
  
        // Iterate from 0 to R * C - 1
        for (int i = 0; i < Mathf.Pow(gridSize, 2); i++) {
            if (cubes[r,c] != null) {
                continue;
            }

            if (!(( rb.constraints & RigidbodyConstraints.FreezePositionY ) != RigidbodyConstraints.None) ){
                currStaticHeights[r,c] = 0;
            }
            yield return new WaitForSeconds(interval);               
            seen[r, c] = true;
            int cr = r + dr[di];
            int cc = c + dc[di];
  
            if (0 <= cr && cr < gridSize && 0 <= cc && cc < gridSize
                && !seen[cr, cc]) {
                r = cr;
                c = cc;
            }
            else {
                di = (di + 1) % 4;
                r += dr[di];
                c += dc[di];
            }
        }
        yield return new WaitForSeconds(1.0f);   
        ResetStaticHeights();
    }


    // Presets for InvokeRepeating()
    void SmallWave(){
        StartCoroutine(Wave(1000.0f, .6f));
    }

    void LargeWave(){
        StartCoroutine(Wave(4000.0f, .1f));
    }

    void AutoSnake(){
        StartCoroutine(Snake());
    }

    void AutoSpiral(){
        StartCoroutine(Spiral());
    }
    #endregion

    void ResetStaticHeights(){
        currStaticHeights = staticHeights;
    }

    public override void OnNetworkSpawn()
    {
        // Only spawn grid on server connection
        // Maybe want some of these variables for interpolation purposes?
        if (!IsServer){
            return;
        }


        // Let's look at a cellular noise approach, with fixed block sizes.
        float xSinAmp = UnityEngine.Random.Range(0.0f, 2.0f);
        float xSinPeriod = UnityEngine.Random.Range(0.05f, 0.1f);
        float xSinShift = UnityEngine.Random.Range(0.0f, 2*Mathf.PI);
        float ySinAmp = UnityEngine.Random.Range(0.0f, 2.0f);
        float ySinPeriod = UnityEngine.Random.Range(0.05f, 0.1f);
        float ySinShift = UnityEngine.Random.Range(0.0f, 2*Mathf.PI);

        staticHeights = new float[gridSize,gridSize];
        cubes = new GameObject[gridSize,gridSize];
        for (int i = 0; i < gridSize; i++){
            for (int j = 0; j < gridSize; j++){

                // Input into a noise function to see if they should exist...
                float iScaled = (float)i/(float)gridSize * 4; // 4 is magic number that looks good with this grid size
                float jScaled =  (float)j/(float)gridSize * 4;
                // TODO: Add a radial vignette here...
                if (noise.cellular(new float2(iScaled, jScaled))[0] < 0.5f){
                    continue;
                }

                // Initialize grid objects
                staticHeights[i,j] = -0.5f * gridSize + xSinAmp*Mathf.Sin(xSinPeriod*(i + xSinShift)) + ySinAmp*Mathf.Sin(ySinPeriod*(j + ySinShift));
                cubes[i,j] = Instantiate(prefab, new Vector3((i-gridSize/2)*cubeSize, -0.5f*gridSize*cubeSize, (j-gridSize/2)*cubeSize),
                                                                Quaternion.identity);
                cubes[i,j].GetComponent<NetworkObject>().Spawn();
                cubes[i,j].transform.localScale = new Vector3(cubeSize, gridSize*cubeSize, cubeSize);
                rb = cubes[i,j].GetComponent<Rigidbody>();
                rb.useGravity = false;
                rb.mass = cubeMass; // Could base this on size
                rb.drag = 2f;
                // Keep an eye on doing nearby hex interpolation only (for client performance reasons)
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                // These constraints help keep the grid stable
                rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionX
                                 | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            }
        }

        currStaticHeights = staticHeights;

        InvokeRepeating("SmallWave", 0.0f, 20.0f);
        //InvokeRepeating("LargeWave", 10.0f, 20.0f);
        //InvokeRepeating("AutoSnake", 0.0f, 50.0f);
        //InvokeRepeating("AutoSpiral", 0.0f, 40.0f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsServer){
            return;
        }

        float diff;
        for (int i  = 0; i < gridSize; i++){
            for (int j = 0; j < gridSize; j++){
                if (cubes[i,j] == null) {
                    continue;
                }
                // Add force pushing hex back to center
                diff = Mathf.Abs(cubes[i,j].transform.position.y - currStaticHeights[i,j]);
                if (cubes[i,j].transform.position.y > 0.95f * cubeSize * currStaticHeights[i, j]){
                    cubes[i,j].GetComponent<Rigidbody>().AddForce(Vector3.down*cubeMass*10*diff);
                } else if(cubes[i,j].transform.position.y < 1.05f * cubeSize * currStaticHeights[i,j]){
                    cubes[i,j].GetComponent<Rigidbody>().AddForce(Vector3.up*cubeMass*10*diff);
                }
            }
        }
    }
}
