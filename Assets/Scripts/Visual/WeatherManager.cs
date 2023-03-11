using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public Vector3 sunLocation = new Vector3(1f, 2f, 0f);
    public float sunSize = 0.01f;
    public Color lightAtmosphereColor = new Color(0.637f, 0.877f, 0.950f, 1f);
    public Color darkAtmosphereColor = new Color(0.419f, 0.705f, 0.910f, 1f);
    public GameObject mainLightObject;
    private Light mainLight;
    public Color mainLightColor;
    public Material skyboxMaterial;
    private Shader skyboxShader;

    void Awake()
    {
        RenderSettings.ambientLight = lightAtmosphereColor; // Currently I think the shader overshoots the lerp, so farAtmosphere is actually the midrange. 
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = darkAtmosphereColor;
        RenderSettings.fogDensity = 0.01f;
    }

    void Start(){
        updateSunLocation();
        mainLight = mainLightObject.GetComponent<Light>();
        updateColors();
        skyboxMaterial.SetFloat("_SunSize", sunSize);
        StartCoroutine(RotateSun(new Vector3(3f, 0.01f, 0f)));
        StartCoroutine(ShiftColors(Color.white, Color.yellow, Color.yellow));
    }

    void updateSunLocation(){
        sunLocation = sunLocation.normalized;
        skyboxMaterial.SetVector("_SunDirection", new Vector4(-1*sunLocation.x, -1*sunLocation.y, -1*sunLocation.z, 0f));
        mainLightObject.transform.rotation = Quaternion.LookRotation(-1*sunLocation);
    }

    void updateColors(){
        mainLight.color = mainLightColor;
        skyboxMaterial.SetColor("_LightAtmosphereColor", lightAtmosphereColor);
        skyboxMaterial.SetColor("_DarkAtmosphereColor", darkAtmosphereColor);
        RenderSettings.ambientLight = lightAtmosphereColor;
        RenderSettings.fogColor = darkAtmosphereColor;
    }

    IEnumerator RotateSun(Vector3 targetVector, float rotateSpeed=0.002f){
        while (sunLocation != targetVector){
            sunLocation = Vector3.RotateTowards(sunLocation, targetVector, rotateSpeed, 0f);
            updateSunLocation();
            yield return new WaitForSeconds(0.033f);
        }
    }

    IEnumerator ShiftColors(Color targetLight, Color targetDark, Color targetAmbient, float duration=20f){
        Color prevLight = lightAtmosphereColor;
        Color prevDark = darkAtmosphereColor;
        Color prevAmbient = mainLightColor;
        float timer = 0f;
        float div = timer/duration;
        while (div < duration){
            lightAtmosphereColor = Color.Lerp(prevLight, targetLight, div);
            darkAtmosphereColor = Color.Lerp(prevDark, targetDark, div);
            mainLightColor = Color.Lerp(prevAmbient, targetAmbient, div);
            updateColors();
            timer += 0.033f;
            div = timer/duration;
            yield return new WaitForSeconds(0.033f);
        }
    }
}
