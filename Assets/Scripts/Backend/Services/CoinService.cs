using System;
using System.Threading.Tasks;
using UnityEngine;

public class CoinService : MonoBehaviour
{
    public static CoinService Instance => _instance ?? CreateSingleton();
    private static CoinService _instance;

    public event Action<long> OnCoinsChanged;

    public long CachedCoins { get; private set; }

    private static CoinService CreateSingleton()
    {
        if (_instance != null)
        {
            return _instance;
        }

        var obj = new GameObject(nameof(CoinService));
        _instance = obj.AddComponent<CoinService>();
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

    public async Task<long> GetCoinsAsync()
    {
        var response = await ApiClient.Instance.GetAsync<CoinResponse>("/Coin", requiresAuth: true);

        if (response != null)
        {
            CachedCoins = response.coins;
        }

        return CachedCoins;
    }

    public async Task<UpdateCoinResponse> SetCoinsAsync(long coins)
    {
        var request = new UpdateCoinRequest { coins = coins };
        var response = await ApiClient.Instance.PutAsync<UpdateCoinRequest, UpdateCoinResponse>("/Coin", request, requiresAuth: true);

        if (response != null && response.success)
        {
            CachedCoins = response.coins;
            OnCoinsChanged?.Invoke(CachedCoins);
        }

        return response;
    }

    public async Task<UpdateCoinResponse> AddCoinsAsync(long amount)
    {
        await GetCoinsAsync();
        return await SetCoinsAsync(CachedCoins + amount);
    }

    // ⭐ MỚI: GET /api/Coin/stats
    public async Task<StatsResponse> GetStatsAsync()
    {
        var response = await ApiClient.Instance.GetAsync<StatsResponse>("/Coin/stats", requiresAuth: true);

        if (response != null)
        {
            CachedCoins = response.coins;
        }

        return response;
    }

    // ⭐ MỚI: PUT /api/Coin/stats
    public async Task<UpdateStatsResponse> UpdateStatsAsync(UpdateStatsRequest request)
    {
        // Validate values cannot be negative
        if (request.coins < 0 || request.totalShipmentDelivered < 0 || request.totalIncome < 0)
        {
            Debug.LogError("[CoinService] Stats cannot be negative!");
            return null;
        }

        var response = await ApiClient.Instance.PutAsync<UpdateStatsRequest, UpdateStatsResponse>("/Coin/stats", request, requiresAuth: true);

        if (response != null && response.success)
        {
            CachedCoins = response.coins;
            OnCoinsChanged?.Invoke(CachedCoins);
        }

        return response;
    }

    // ⭐ MỚI: Helper method to update stats with individual values
    public async Task<UpdateStatsResponse> UpdateStatsAsync(long coins, int totalShipmentDelivered, long totalIncome)
    {
        var request = new UpdateStatsRequest
        {
            coins = coins,
            totalShipmentDelivered = totalShipmentDelivered,
            totalIncome = totalIncome
        };

        return await UpdateStatsAsync(request);
    }
}

