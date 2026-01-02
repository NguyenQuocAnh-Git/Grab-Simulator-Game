using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    public static ApiClient Instance => _instance ?? CreateSingleton();
    private static ApiClient _instance;

    private const string BaseUrl = "https://brobackend-t1l0.onrender.com/api";


    private static ApiClient CreateSingleton()
    {
        if (_instance != null)
        {
            return _instance;
        }

        var obj = new GameObject(nameof(ApiClient));
        _instance = obj.AddComponent<ApiClient>();
        return _instance;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task<T> GetAsync<T>(string endpoint, bool requiresAuth = false)
    {
        var url = BuildUrl(endpoint);

        using (var request = UnityWebRequest.Get(url))
        {
            if (requiresAuth && TokenManager.Instance.IsTokenValid)
            {
                request.SetRequestHeader("Authorization", $"Bearer {TokenManager.Instance.CurrentToken}");
            }

            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ApiClient] GET {url} failed: {request.error}");
                return default;
            }

            var json = request.downloadHandler.text;
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ApiClient] Failed to parse GET response for {url}: {ex.Message}");
                return default;
            }
        }
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, bool requiresAuth = false)
    {
        var url = BuildUrl(endpoint);
        var jsonData = JsonUtility.ToJson(data);

        using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            var bodyRaw = Encoding.UTF8.GetBytes(jsonData ?? "{}");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (requiresAuth && TokenManager.Instance.IsTokenValid)
            {
                request.SetRequestHeader("Authorization", $"Bearer {TokenManager.Instance.CurrentToken}");
            }

            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ApiClient] POST {url} failed: {request.error} - {request.downloadHandler.text}");
                return default;
            }

            var json = request.downloadHandler.text;
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            try
            {
                return JsonUtility.FromJson<TResponse>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ApiClient] Failed to parse POST response for {url}: {ex.Message}");
                return default;
            }
        }
    }

    public async Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, bool requiresAuth = false)
    {
        var url = BuildUrl(endpoint);
        var jsonData = JsonUtility.ToJson(data);

        using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPUT))
        {
            var bodyRaw = Encoding.UTF8.GetBytes(jsonData ?? "{}");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (requiresAuth && TokenManager.Instance.IsTokenValid)
            {
                request.SetRequestHeader("Authorization", $"Bearer {TokenManager.Instance.CurrentToken}");
            }

            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[ApiClient] PUT {url} failed: {request.error} - {request.downloadHandler.text}");
                return default;
            }

            var json = request.downloadHandler.text;
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            try
            {
                return JsonUtility.FromJson<TResponse>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ApiClient] Failed to parse PUT response for {url}: {ex.Message}");
                return default;
            }
        }
    }

    public Task<TResponse> PostJsonAsync<TRequest, TResponse>(string endpoint, TRequest body, bool includeAuth = false)
        where TResponse : class, new()
    {
        return PostAsync<TRequest, TResponse>(endpoint, body, includeAuth);
    }

    private static string BuildUrl(string endpoint)
    {
        if (string.IsNullOrEmpty(endpoint))
        {
            return BaseUrl;
        }

        if (!endpoint.StartsWith("/"))
        {
            endpoint = "/" + endpoint;
        }

        return BaseUrl + endpoint;
    }
}

