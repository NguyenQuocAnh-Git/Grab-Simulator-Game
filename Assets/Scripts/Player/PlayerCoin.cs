using System;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerCoin : MonoBehaviour
{
    private int currentCoin;       // coin tích trong 1 lần giao hàng
    private int totalCoin;         // tổng coin hiển thị (server + session)
    private int sessionCoins;      // coin kiếm được trong session này (chưa upload)
    private bool hasUploadedThisRound;

    // ⭐ MỚI: Track stats for leaderboard
    private int sessionShipments;  // số đơn hàng giao trong session này (chưa upload)
    private long sessionIncome;   // tổng thu nhập trong session này (chưa upload)
    
    // Server stats (loaded from API)
    private int totalShipmentDelivered;
    private long totalIncome;

    public event Action<int> OnTotalCoinChanged;

    [SerializeField] private PlayerState playerState;
    [SerializeField] private TimeDeliveryUI timerUI;

    private async void Start()
    {
        currentCoin = 0;
        totalCoin = 0;
        sessionCoins = 0;
        sessionShipments = 0;
        sessionIncome = 0;
        hasUploadedThisRound = false;

        playerState.OnStateChanged += OnPlayerDeliveredFood;
        GameManager.Instance.OnGameStateChanged += OnGameOver;
        GameManager.Instance.OnGameStateChanged += OnGamePlay;

        await SyncServerStatsAtStart();
    }

    private void OnDisable()
    {
        playerState.OnStateChanged -= OnPlayerDeliveredFood;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnGameOver;
            GameManager.Instance.OnGameStateChanged -= OnGamePlay;
        }
    }

    private async Task SyncServerStatsAtStart()
    {
        if (!TokenManager.Instance.HasValidToken)
        {
            OnTotalCoinChanged?.Invoke(totalCoin);
            return;
        }

        // ⭐ MỚI: Load stats instead of just coins
        var stats = await CoinService.Instance.GetStatsAsync();
        if (stats != null)
        {
            totalCoin = SafeToInt(stats.coins);
            totalShipmentDelivered = stats.totalShipmentDelivered;
            totalIncome = stats.totalIncome;
            OnTotalCoinChanged?.Invoke(totalCoin);
        }
        else
        {
            // Fallback to old API if stats API fails
            var coins = await CoinService.Instance.GetCoinsAsync();
            totalCoin = SafeToInt(coins);
            OnTotalCoinChanged?.Invoke(totalCoin);
        }
    }

    private void OnPlayerDeliveredFood(EPlayerState playerState)
    {
        if (playerState == EPlayerState.DeliveredFood)
        {
            AddCoin();
            totalCoin += currentCoin;
            sessionCoins += currentCoin;  // track session earnings
            // ⭐ MỚI: Track shipments and income
            sessionShipments++;
            sessionIncome += currentCoin;  // income = coin earned from delivery
            OnTotalCoinChanged?.Invoke(totalCoin);
            Debug.Log($"current coin: {currentCoin}, session: {sessionCoins}, shipments: {sessionShipments}, income: {sessionIncome}");
        }
    }

    private void OnGameOver(GameState gameState)
    {
        if (gameState != GameState.GameOver) return;
        if (hasUploadedThisRound) return;

        hasUploadedThisRound = true;
        _ = UploadCoinsToServer();
    }

    private void OnGamePlay(GameState gameState)
    {
        if (gameState != GameState.GamePlaying) return;

        currentCoin = 0;
        hasUploadedThisRound = false;
        // Không reset sessionCoins ở đây vì session kéo dài qua nhiều ván cho đến khi upload
    }

    private async Task UploadCoinsToServer()
    {
        if (!TokenManager.Instance.HasValidToken)
        {
            return;
        }

        // ⭐ MỚI: Upload stats instead of just coins
        if (sessionCoins <= 0 && sessionShipments <= 0)
        {
            return;
        }

        // Calculate new totals
        long newCoins = totalCoin + sessionCoins;
        int newShipments = totalShipmentDelivered + sessionShipments;
        long newIncome = totalIncome + sessionIncome;

        var response = await CoinService.Instance.UpdateStatsAsync(newCoins, newShipments, newIncome);
        if (response != null && response.success)
        {
            totalCoin = SafeToInt(response.coins);
            totalShipmentDelivered = response.totalShipmentDelivered;
            totalIncome = response.totalIncome;
            
            // Reset session tracking
            sessionCoins = 0;
            sessionShipments = 0;
            sessionIncome = 0;
            
            OnTotalCoinChanged?.Invoke(totalCoin);
            Debug.Log($"Stats updated: Coins={totalCoin}, Shipments={totalShipmentDelivered}, Income={totalIncome}");
        }
        else
        {
            // Fallback to old API if stats API fails
            var coinResponse = await CoinService.Instance.AddCoinsAsync(sessionCoins);
            if (coinResponse != null && coinResponse.success)
            {
                totalCoin = SafeToInt(coinResponse.coins);
                sessionCoins = 0;
                OnTotalCoinChanged?.Invoke(totalCoin);
            }
        }
    }

    /// <summary>
    /// Public method to force save data to server (used when exiting game)
    /// </summary>
    public async Task SaveDataAsync()
    {
        if (!TokenManager.Instance.HasValidToken)
        {
            return;
        }

        // Upload stats if there's any session data
        if (sessionCoins > 0 || sessionShipments > 0)
        {
            await UploadCoinsToServer();
        }
    }

    private void AddCoin()
    {
        const int baseCoin = 15;
        const int maxBonus = 20;

        int snap = timerUI.SnapTime();
        int max = timerUI.MaxTime();

        int bonus;
        float ratio = (float)snap / max;

        if (ratio >= 0.66f)
            bonus = maxBonus;          // nhanh
        else if (ratio >= 0.33f)
            bonus = maxBonus / 2;      // trung bình
        else
            bonus = 0;                 // chậm
        int coinWillAdd = baseCoin + bonus;
        Vector3 spawnPos =
            playerState.transform.position +
            playerState.transform.forward * 1.2f;
        FloatingManager.Instance.ShowCoinFloat(coinWillAdd, spawnPos);
        currentCoin += coinWillAdd;
    }

    public int GetCurrentCoint()
    {
        return currentCoin;
    }

    private int SafeToInt(long value)
    {
        if (value > int.MaxValue) return int.MaxValue;
        if (value < int.MinValue) return int.MinValue;
        return (int)value;
    }
    public bool CanSpendCoin(int amount)
    {
        return totalCoin >= amount;
    }

    public bool SpendCoin(int amount)
    {
        if (amount <= 0) return true;

        if (totalCoin < amount)
            return false;

        totalCoin -= amount;
        OnTotalCoinChanged?.Invoke(totalCoin);
        return true;
    }

    public int GetTotalCoin()
    {
        return totalCoin;
    }

}
