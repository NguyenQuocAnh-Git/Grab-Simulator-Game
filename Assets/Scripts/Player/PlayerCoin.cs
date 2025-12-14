using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCoin : MonoBehaviour
{
    private int currentCoin;
    private int totalCoin;
    public event Action<int> OnTotalCoinChanged;
    [SerializeField] private PlayerState playerState;

    [SerializeField] private TimeDeliveryUI timerUI;
    private void Start()
    {
        currentCoin = 0;
        totalCoin = 0;
        OnTotalCoinChanged?.Invoke(totalCoin);
        playerState.OnStateChanged += OnPlayerDeliveredFood;
        GameManager.Instance.OnGameStateChanged += OnGameOver;
        GameManager.Instance.OnGameStateChanged += OnGamePlay;
    }
    private void OnDisable()
    {
        playerState.OnStateChanged -= OnPlayerDeliveredFood;
        GameManager.Instance.OnGameStateChanged -= OnGameOver;
        GameManager.Instance.OnGameStateChanged -= OnGamePlay;
    }
    private void OnPlayerDeliveredFood(EPlayerState playerState)
    {
        // Giao hàng thành công
        if(playerState == EPlayerState.DeliveredFood)
        {
            AddCoin();
            totalCoin += currentCoin;
            OnTotalCoinChanged?.Invoke(totalCoin);
            Debug.Log($"current coin: {currentCoin}");
        }
    }
    private void OnGameOver(GameState gameState)
    {
        if(gameState != GameState.GameOver) return;

    } 
    private void OnGamePlay(GameState gameState)
    {
        if (gameState != GameState.GamePlaying) return;
        currentCoin = 0;
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
}
