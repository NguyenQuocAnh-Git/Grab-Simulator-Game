using System;
using System.Text;
using UnityEngine;

public sealed class TokenManager
{
    private const string TokenKey = "auth_token";
    private static readonly Lazy<TokenManager> lazyInstance = new Lazy<TokenManager>(() => new TokenManager());

    public static TokenManager Instance => lazyInstance.Value;

    public string Token { get; private set; }
    public bool HasValidToken => !string.IsNullOrEmpty(Token) && !IsExpired(Token);

    private TokenManager()
    {
        Token = PlayerPrefs.GetString(TokenKey, string.Empty);
    }

    public void SaveToken(string token)
    {
        Token = token;
        PlayerPrefs.SetString(TokenKey, token);
        PlayerPrefs.Save();
    }

    public void ClearToken()
    {
        Token = string.Empty;
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.Save();
    }

    public bool IsLoggedIn()
    {
        return HasValidToken;
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

