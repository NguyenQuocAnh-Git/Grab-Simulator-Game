using System;
using System.Threading.Tasks;
using UnityEngine;

public class LeaderboardService : MonoBehaviour
{
    public static LeaderboardService Instance => _instance ?? CreateSingleton();
    private static LeaderboardService _instance;

    private LeaderboardResponse _cachedLeaderboard;
    private DateTime _cacheTime;
    private const float CacheDurationSeconds = 30f;

    private static LeaderboardService CreateSingleton()
    {
        if (_instance != null)
        {
            return _instance;
        }

        var obj = new GameObject(nameof(LeaderboardService));
        _instance = obj.AddComponent<LeaderboardService>();
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

    public async Task<LeaderboardResponse> GetTopPlayersAsync(int count = 10, bool forceRefresh = false)
    {
        if (!forceRefresh && _cachedLeaderboard != null &&
            (DateTime.UtcNow - _cacheTime).TotalSeconds < CacheDurationSeconds)
        {
            return _cachedLeaderboard;
        }

        // JsonUtility doesn't support List directly, so we use the wrapper
        var wrapper = await ApiClient.Instance.GetAsync<LeaderboardResponseWrapper>($"/Leaderboard?top={count}", requiresAuth: true);

        if (wrapper != null)
        {
            // Convert array to List
            var response = new LeaderboardResponse
            {
                entries = new System.Collections.Generic.List<LeaderboardEntry>(wrapper.entries),
                totalCount = wrapper.totalCount
            };

            _cachedLeaderboard = response;
            _cacheTime = DateTime.UtcNow;

            return response;
        }

        return null;
    }
}

