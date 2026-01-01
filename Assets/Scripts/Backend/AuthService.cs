using System.Threading.Tasks;
using UnityEngine;

public sealed class AuthService
{
    private static readonly System.Lazy<AuthService> lazyInstance = new System.Lazy<AuthService>(() => new AuthService());
    public static AuthService Instance => lazyInstance.Value;

    private readonly ApiClient apiClient;

    public bool IsLoggedIn => TokenManager.Instance.HasValidToken;

    private AuthService()
    {
        apiClient = ApiClient.Instance;
    }

    public async Task<RequestOtpResponse> RequestOtpAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new RequestOtpResponse { success = false, message = "Email is required" };
        }

        var payload = new RequestOtpRequest { email = email.Trim() };
        var response = await apiClient.PostJsonAsync<RequestOtpRequest, RequestOtpResponse>("/Auth/request-otp", payload);
        return response ?? new RequestOtpResponse { success = false, message = "Request failed" };
    }

    public async Task<VerifyOtpResponse> VerifyOtpAsync(string email, string otp)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
        {
            return new VerifyOtpResponse { success = false, message = "Email and OTP are required" };
        }

        var payload = new VerifyOtpRequest { email = email.Trim(), otp = otp.Trim() };
        var response = await apiClient.PostJsonAsync<VerifyOtpRequest, VerifyOtpResponse>("/Auth/verify-otp", payload);

        if (response != null && response.success && !string.IsNullOrEmpty(response.token))
        {
            TokenManager.Instance.SaveToken(response.token);
        }

        return response ?? new VerifyOtpResponse { success = false, message = "Verification failed" };
    }

    public void Logout()
    {
        TokenManager.Instance.ClearToken();
        Debug.Log("[AuthService] Logged out, token cleared.");
    }
}

