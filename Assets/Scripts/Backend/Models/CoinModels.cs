using System;

[Serializable]
public class CoinResponse
{
    public long coins;
}

[Serializable]
public class UpdateCoinRequest
{
    public long coins;
}

[Serializable]
public class UpdateCoinResponse
{
    public bool success;
    public long coins;
    public string message;
}

// ⭐ MỚI: Stats Models
[Serializable]
public class StatsResponse
{
    public long coins;
    public int totalShipmentDelivered;
    public long totalIncome;
}

[Serializable]
public class UpdateStatsRequest
{
    public long coins;
    public int totalShipmentDelivered;
    public long totalIncome;
}

[Serializable]
public class UpdateStatsResponse
{
    public bool success;
    public long coins;
    public int totalShipmentDelivered;
    public long totalIncome;
    public string message;
}

