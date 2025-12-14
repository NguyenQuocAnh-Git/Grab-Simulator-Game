using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WeatherManager : MonoBehaviour
{
    [SerializeField] RectTransform Aim;
    [SerializeField] private Light sun;
    [SerializeField] List<WeatherEntry> weathers;

    [SerializeField] private float timer = 0;
    [SerializeField] private int timeToChangeWeather = 10;
    [SerializeField] private int index = 0;
    [SerializeField] private float transitionTime = 3f;
    private Dictionary<WeatherType, WeatherData> weatherMap;
    private GameObject currentFX1;
    private GameObject currentFX2;
    private CancellationTokenSource weatherCTS;
    private void Awake()
    {
        weatherMap = new Dictionary<WeatherType, WeatherData>();
        foreach (var w in weathers)
            weatherMap[w.type] = w.profile;
    }
    private void RotateAim()
    {
        int time = timeToChangeWeather*4;
        Aim
        .DORotate(new Vector3(0, 0, -360f), time, RotateMode.FastBeyond360)
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Restart);
    }
    private void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        SetWeather(weathers[0].type); // default
        RenderSettings.skybox = weatherMap[0].skybox;
        DynamicGI.UpdateEnvironment();
        RotateAim();
    }
    public void Update()
    {
        timer += Time.deltaTime;
         if (timer >= timeToChangeWeather)
        {
            timer = 0;
            index = (index + 1) % weathers.Count;
            SetWeather(weathers[index].type);
        }
    }
    public void SetWeather(WeatherType type)
    {
        if (!weatherMap.TryGetValue(type, out var target))
            return;

        weatherCTS?.Cancel();
        weatherCTS?.Dispose();
        weatherCTS = new CancellationTokenSource();
        
        TransitionWeatherAsync(target, weatherCTS.Token).Forget();
    }
    async UniTask TransitionWeatherAsync(
    WeatherData target,
    CancellationToken token)
    {
        var a0 = RenderSettings.ambientLight;
        var f0 = RenderSettings.fogColor;
        var d0 = RenderSettings.fogDensity;
        var c0 = sun.color;
        var i0 = sun.intensity;
        RenderSettings.skybox = target.skybox;
        DynamicGI.UpdateEnvironment();
        float t = 0f;

        if (currentFX1) Destroy(currentFX1);
        if (currentFX2) Destroy(currentFX2);
        if (target.weatherFX)
        {
            currentFX1 = Instantiate(target.weatherFX);
            currentFX2 = Instantiate(target.weatherFX);
        }

        while (t < 1f)
        {
            if (token.IsCancellationRequested) return;
            t += Time.deltaTime / transitionTime;

            RenderSettings.ambientLight = Color.Lerp(a0, target.ambientColor, t);
            RenderSettings.fogColor     = Color.Lerp(f0, target.fogColor, t);
            RenderSettings.fogDensity   = Mathf.Lerp(d0, target.fogDensity, t);
            sun.color                   = Color.Lerp(c0, target.light.color, t);
            sun.intensity               = Mathf.Lerp(i0, target.light.intensity, t);
            await UniTask.Yield();
        }
        
    }

    void OnDestroy()
    {
        weatherCTS?.Cancel();
        weatherCTS?.Dispose();
    }

}

[System.Serializable]
public class WeatherEntry
{
    public WeatherType type;
    public WeatherData profile;
}

public enum WeatherType
{
    Sunny,
    Rain,
    Snow,
    Cloudy
}