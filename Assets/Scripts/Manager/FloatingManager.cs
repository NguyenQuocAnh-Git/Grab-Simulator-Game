using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FloatingManager : MonoBehaviour
{
    public static FloatingManager Instance;

    [SerializeField] private GameObject coinFloat;

    private void Awake()
    {
        if(Instance != null) return;

        Instance = this;
    }

    public void ShowCoinFloat(int coin, Vector3 pos)
    {
        GameObject go = Instantiate(coinFloat, pos, Quaternion.identity);
        go.GetComponent<CoinFloat>().Show(coin);
    }
}
