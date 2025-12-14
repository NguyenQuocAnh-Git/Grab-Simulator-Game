using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Weather/Data")]
public class WeatherData : ScriptableObject
{
    public Color ambientColor;
    public Color fogColor;
    public float fogDensity;

    public LightSettings light;
    public GameObject weatherFX; // Rain / Snow particle

    public Material skybox;
}


[System.Serializable]
public class LightSettings
{
    public Color color;
    public float intensity;
}
