using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCoin : MonoBehaviour
{
    private int currentCoin;
    private int totalCoin;
    [SerializeField] private PlayerState playerState;
    private void Start()
    {
        currentCoin = 0;
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
            currentCoin += 15;
            Debug.Log($"current coin: {currentCoin}");
        }
    }
    private void OnGameOver(GameState gameState)
    {
        if(gameState != GameState.GameOver) return;
        totalCoin += currentCoin;
    } 
    private void OnGamePlay(GameState gameState)
    {
        if (gameState != GameState.GamePlaying) return;
        currentCoin = 0;
    } 
    private int GetCoinWhenDelivered(int time)
    {
        int _baseCoin = 15;

        return _baseCoin;
    }
    public int GetCurrentCoint() => currentCoin;
}
