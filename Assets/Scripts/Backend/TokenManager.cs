using System;
using System.Text;
using UnityEngine;

public class TokenManager : MonoBehaviour
{
    private const string TokenKey = "jwt_token";
    private const string EmailKey = "user_email";

    private static TokenManager _instance;
    public static TokenManager Instance => _instance ?? CreateSingleton();

    public string CurrentToken { get; private set; }
    public string Token => CurrentToken;
    public string CurrentEmail { get; private set; }

    public bool IsTokenValid => !string.IsNullOrEmpty(CurrentToken) && !IsExpired(CurrentToken);
    public bool HasValidToken => IsTokenValid;

    private static TokenManager CreateSingleton()
    {
        if (_instance != null)
        {
            return _instance;
        }

        var obj = new GameObject(nameof(TokenManager));
        _instance = obj.AddComponent<TokenManager>();
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
        LoadToken();
    }

    public void SaveToken(string token, string email = null)
    {
        CurrentToken = token;

        if (!string.IsNullOrEmpty(email))
        {
            CurrentEmail = email;
            PlayerPrefs.SetString(EmailKey, email);
        }

        PlayerPrefs.SetString(TokenKey, token ?? string.Empty);
        PlayerPrefs.Save();
    }

    public void ClearToken()
    {
        CurrentToken = null;
        CurrentEmail = null;
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.DeleteKey(EmailKey);
        PlayerPrefs.Save();
    }

    public bool IsLoggedIn()
    {
        return IsTokenValid;
    }

    private void LoadToken()
    {
        CurrentToken = PlayerPrefs.GetString(TokenKey, null);
        CurrentEmail = PlayerPrefs.GetString(EmailKey, null);
    }

    private bool IsExpired(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
            {
                return true;
            }

            var payload = parts[1];
            var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var bytes = Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
            var json = Encoding.UTF8.GetString(bytes);
            var payloadData = JsonUtility.FromJson<JwtPayload>(json);

            if (payloadData == null || payloadData.exp <= 0)
            {
                return true;
            }

            var expiry = DateTimeOffset.FromUnixTimeSeconds(payloadData.exp);
            return expiry <= DateTimeOffset.UtcNow;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[TokenManager] Failed to parse token: {ex.Message}");
            return true;
        }
    }

    [Serializable]
    private class JwtPayload
    {
        public long exp;
    }
}

