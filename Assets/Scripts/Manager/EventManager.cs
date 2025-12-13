using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;
    void Awake()
    {
        if(Instance != null) return;
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public event Action<GameObject> OnBikeSpawn;
    public event Action<bool> OnPlayerDead;
    public void BikeSpawned(GameObject bike)
    {
        Debug.Log("Invoking OnBikeSpawn");
        OnBikeSpawn?.Invoke(bike);
    }

    public void PlayerDeadInvoke(bool isDead)
    {
        OnPlayerDead?.Invoke(isDead);
    }
}
