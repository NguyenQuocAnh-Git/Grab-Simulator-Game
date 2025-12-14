using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameState
{
    Loading,
    Lobby,
    GamePlaying,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private GameState gameState;
    public event Action<GameState> OnGameStateChanged;
    public GameState GameState => gameState;
    
    
    [SerializeField] private Transform broOriginalPos;
    [SerializeField] private Transform bikeOriginalPos;

    [SerializeField] private GameObject bikePrefab;
    [SerializeField] private GameObject bro;
    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetGameState(GameState.Lobby);
    }

    public bool IsGamePlaying()
    {
        return gameState == GameState.GamePlaying;
    }

    public void SetGameState(GameState newGameState)
    {
        gameState = newGameState;
        // Phát sự kiện (đã thay đổi game state) cho các thực thể đăng ký
        OnGameStateChanged?.Invoke(newGameState);
    }

    public Transform GetBroOriginalPos()
    {
        return broOriginalPos;
    }

    public Transform GetBikeOriginalPos()
    {
        return bikeOriginalPos;
    }

    public void ReplayGame()
    {
        SetGameState(GameState.GamePlaying);
        ResetBroState();
        DestroyOldBike();
        GameFactory.Instance.SpawnBike(bikeOriginalPos.position, Quaternion.identity);
    }

    
    public void ReturnLobby()
    {
        SetGameState(GameState.Lobby);

        ResetBroState();
        DestroyOldBike();
        CameraManager.Instance.SwitchToMainMenuCamera();
        GameFactory.Instance.SpawnBike(bikeOriginalPos.position, Quaternion.identity);
    }

    public void GameOver()
    {
        SetGameState(GameState.GameOver);
        
        EventManager.Instance.PlayerDeadInvoke(true);
    }
    private void DestroyOldBike()
    {
        var bikeBodies = TagRegistry.Get("bikeBody").ToArray();
        for (int i = 0; i < bikeBodies.Length; i++)
        {
            var body = bikeBodies[i];
            if (body != null) Destroy(body);
        }

        var currentBikes = TagRegistry.Get("Bike").ToArray();
        for (int i = 0; i < currentBikes.Length; i++)
        {
            var bike = currentBikes[i];
            if (bike != null) Destroy(bike);
        }
    }

    private void ResetBroState()
    {
        var playerController = bro.GetComponent<PlayerController>();
        playerController.ResetBroState();

        var playerState = bro.GetComponent<PlayerState>();
        playerState.CurrentState = EPlayerState.Available;

        playerController.DisableCollider();
        bro.transform.SetPositionAndRotation(broOriginalPos.position, Quaternion.identity);
        playerController.EnableCollider();
        CameraManager.Instance.SwitchToPlayerCamera();
    }
    public GameObject GetThisPlayer()
    {
        return bro;
    }
}
