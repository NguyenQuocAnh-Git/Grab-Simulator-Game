using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private GameObject EButton; // canvas E to drive
    [SerializeField] private GameObject miniMap; // iphone 4
    
    [SerializeField] private PlayerController playerController;
    [SerializeField] private BikeController bikeController;
    private bool isShowMap = false;
    private bool isNearBike = false;
    private bool isDriving = false;
    private void Start()
    {
        EventManager.Instance.OnBikeSpawn += OnBikeSpawned;
        GameManager.Instance.OnGameStateChanged += OnGameOver;
    }
    private void Update()
    {
        if (!GameManager.Instance.IsGamePlaying()) return;
        if (isNearBike && !isDriving && Input.GetKeyDown(KeyCode.E))
        {
            EnterBike();
        }
        else if (isNearBike && isDriving && Input.GetKeyDown(KeyCode.E))
        {
            ExitBike();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!GameManager.Instance.IsGamePlaying()) return;
        if(other.CompareTag("Player"))
        {
            isNearBike = true;
            if(!isDriving)
                EButton.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        // if not playing game => return 
        if (!GameManager.Instance.IsGamePlaying()) return;
        
        if (other.CompareTag("Player"))
        {
            isNearBike = false;
            EButton.SetActive(false);
        }
    }
    private void EnterBike()
    {
        isDriving = true;
        EButton.SetActive(false);
        playerController.EnteredBike();
        bikeController.EnteredBike();
        OpenMap();

        CameraManager.Instance.SwitchToBikeCamera();
    }
    private void ExitBike()
    {
        isDriving = false;
        playerController.ExittedBike();
        bikeController.ExittedBike();
        CloseMap();

        CameraManager.Instance.SwitchToPlayerCamera();
    }
    private void OpenMap()
    {
        if (isShowMap) return;
        isShowMap = true;
        var rect = miniMap.GetComponent<RectTransform>();
        rect.DOMove(new Vector3(50, 50, 0), 1f, true).SetEase(Ease.InOutBounce);
    }
    private void CloseMap()
    {
        if(!isShowMap) return;
        isShowMap = false;
        var rect = miniMap.GetComponent<RectTransform>();
        rect.DOMove(new Vector3(50, -300, 0), 1f, true).SetEase(Ease.InOutBounce);
    }
    private void OnBikeSpawned(GameObject bike)
    {
        if(bike != null)
        {
            bikeController = bike.GetComponentInChildren<BikeController>();
        }
    }
    private void OnGameOver(GameState gameState)
    {
        if(gameState != GameState.GameOver) return;

        isDriving = false;
        CloseMap();
    }
}
