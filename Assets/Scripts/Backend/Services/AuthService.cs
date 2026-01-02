using System;
using System.Threading.Tasks;
using UnityEngine;

public class AuthService : MonoBehaviour
{
    public static AuthService Instance => _instance ?? CreateSingleton();
    private static AuthService _instance;

    public event Action OnLoginSuccess;
    public event Action OnLogout;

    public bool IsLoggedIn => TokenManager.Instance.IsTokenValid;
    public string CurrentEmail => TokenManager.Instance.CurrentEmail;

    private static AuthService CreateSingleton()
    {
        if (_instance != null)
        {
            return _instance;
        }

        var obj = new GameObject(nameof(AuthService));
        _instance = obj.AddComponent<AuthService>();
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

    public async Task<RequestOtpResponse> RequestOtpAsync(string email)
    {
        var request = new RequestOtpRequest { email = email };
        var response = await ApiClient.Instance.PostAsync<RequestOtpRequest, RequestOtpResponse>("/Auth/request-otp", request);
        return response;
    }

    public async Task<VerifyOtpResponse> VerifyOtpAsync(string email, string otp)
    {
        var request = new VerifyOtpRequest { email = email, otp = otp };
        var response = await ApiClient.Instance.PostAsync<VerifyOtpRequest, VerifyOtpResponse>("/Auth/verify-otp", request);

        if (response != null && response.success && !string.IsNullOrEmpty(response.token))
        {
            TokenManager.Instance.SaveToken(response.token, email);
            OnLoginSuccess?.Invoke();
        }

        return response;
    }

    public void Logout()
    {
        TokenManager.Instance.ClearToken();
        OnLogout?.Invoke();
    }
}

