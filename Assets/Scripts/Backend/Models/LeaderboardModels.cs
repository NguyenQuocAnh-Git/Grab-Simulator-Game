using System;
using System.Collections.Generic;

[Serializable]
public class LeaderboardEntry
{
    public int rank;
    public string email;
    public int totalShipmentDelivered;
    public long totalIncome;
    public long value; // = totalShipmentDelivered * totalIncome
}

[Serializable]
public class LeaderboardResponse
{
    public List<LeaderboardEntry> entries;
    public int totalCount;
}

// Wrapper class for JsonUtility (JsonUtility doesn't support List directly)
[Serializable]
public class LeaderboardResponseWrapper
{
    public LeaderboardEntry[] entries;
    public int totalCount;
}

