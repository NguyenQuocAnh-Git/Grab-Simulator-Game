using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public sealed class ApiClient
{
    private const string BaseUrl = "https://brobackend-t1l0.onrender.com/api";
    private static readonly Lazy<ApiClient> lazyInstance = new Lazy<ApiClient>(() => new ApiClient());

    public static ApiClient Instance => lazyInstance.Value;

    private ApiClient()
    {
    }

    public async Task<TResponse> PostJsonAsync<TRequest, TResponse>(string endpoint, TRequest body, bool includeAuth = false)
        where TResponse : class, new()
    {
        var url = BuildUrl(endpoint);
        var payload = JsonUtility.ToJson(body);

        using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            var bodyRaw = Encoding.UTF8.GetBytes(payload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (includeAuth && TokenManager.Instance.HasValidToken)
            {
                request.SetRequestHeader("Authorization", $"Bearer {TokenManager.Instance.Token}");
            }

            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.DataProcessingError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[ApiClient] POST {url} failed: {request.responseCode} {request.error}");
                return default;
            }

            var json = request.downloadHandler.text;
            if (string.IsNullOrWhiteSpace(json))
            {
                return new TResponse();
            }

            try
            {
                return JsonUtility.FromJson<TResponse>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ApiClient] Failed to parse response for {url}: {ex.Message}");
                return default;
            }
        }
    }

    private string BuildUrl(string endpoint)
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

