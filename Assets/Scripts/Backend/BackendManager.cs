using UnityEngine;

public class BackendManager : MonoBehaviour
{
    public static BackendManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            gameObject.AddComponent<ApiClient>();
            gameObject.AddComponent<TokenManager>();
            gameObject.AddComponent<AuthService>();
            gameObject.AddComponent<CoinService>();
            gameObject.AddComponent<LeaderboardService>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

