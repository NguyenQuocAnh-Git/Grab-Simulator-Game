using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action<GameObject> OnBikeSpawn;
    public static event Action<bool> OnPlayerDead;
    public static void BikeSpawned(GameObject bro)
    {
        OnBikeSpawn?.Invoke(bro);
    }

    public static void PlayerDeadInvoke(bool isDead)
    {
        OnPlayerDead?.Invoke(isDead);
    }
}
