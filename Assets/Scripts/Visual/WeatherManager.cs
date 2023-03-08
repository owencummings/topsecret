using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public Vector3 sunLocation = new Vector3(1f, 1f, 0f);
    public float sunSize = 0.01f;
    public Color closeAtmosphereColor = new Color(0.637f, 0.877f, 0.950f, 1f);
    public Color farAtmosphereColor = new Color(0.419f, 0.705f, 0.910f, 1f);
    public GameObject mainLightObject;
    private Light mainLight;
    public Color mainLightColor;
    public Material skyboxMaterial;
    private Shader skyboxShader;

    void Awake()
    {
        RenderSettings.ambientLight = (closeAtmosphereColor+farAtmosphereColor)/2;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
    }

    void Start(){
        sunLocation = sunLocation.normalized;
        mainLight = mainLightObject.GetComponent<Light>();
        mainLight.color = mainLightColor;
        skyboxMaterial.SetVector("_SunDirection", new Vector4(-1*sunLocation.x, -1*sunLocation.y, -1*sunLocation.z, 0f));
        skyboxMaterial.SetFloat("_SunSize", sunSize);
        skyboxMaterial.SetColor("_CloseAtmosphereColor", closeAtmosphereColor);
        skyboxMaterial.SetColor("_FarAtmosphereColor", farAtmosphereColor);
        mainLightObject.transform.rotation = Quaternion.LookRotation(-1*sunLocation);
    }

}
