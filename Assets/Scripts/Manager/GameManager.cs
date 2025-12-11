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

        DestroyOldBike();

        bro.SetActive(true);
        var playerController = bro.GetComponent<PlayerController>();
        
        // when play again => return to player state and camera
        playerController.enabled = true;
        CameraManager.Instance.SwitchToPlayerCamera();
        
        bro.transform.position = broOriginalPos.position;
        GameFactory.Instance.SpawnBike(bikeOriginalPos.position, Quaternion.identity);
    }

    
    public void ReturnLobby()
    {
        SetGameState(GameState.Lobby);
        DestroyOldBike();
        bro.SetActive(true);

        CameraManager.Instance.SwitchToMainMenuCamera();
        bro.transform.position = broOriginalPos.position;
        var playerController = bro.GetComponent<PlayerController>();
        playerController.enabled = true;
        
        GameFactory.Instance.SpawnBike(bikeOriginalPos.position, Quaternion.identity);
    }

    public void GameOver()
    {
        SetGameState(GameState.GameOver);
        
        EventManager.PlayerDeadInvoke(true);
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
}
