using UnityEngine;
using System;
using DG.Tweening;


public class BikeInteraction : MonoBehaviour
{
    public static event Action OnEnterBike;
    public static event Action OnExitBike;

    public GameObject interactionUI;       // Canvas "E to drive"
    public GameObject miniMap;
    public MonoBehaviour playerController; // Script điều khiển nhân vật
    public MonoBehaviour bikeController;   // Script điều khiển xe
    public Transform player;
    [Header("Audio")]
    public AudioSource engineSound;
    public AudioSource skidSound;

    private bool isNear = false;
    private bool isDriving = false;

    private bool isShowMap = false;
    void Start()
    {
        transform.position = GameManager.Instance.GetBikeOriginalPos().position;
        interactionUI.SetActive(false);
        bikeController.enabled = false;
    }
    void OnEnable()
    {
        EventManager.OnPlayerDead += OnPlayerDead;
    }
    
    void OnDisable()
    {
        EventManager.OnPlayerDead -= OnPlayerDead;
    }

    void Update()
    {
        // if not playing game => return 
        if (!GameManager.Instance.IsGamePlaying()) return;
        
        if (isNear && !isDriving && Input.GetKeyDown(KeyCode.E))
        {
            EnterBike();
        }
        else if (isDriving && Input.GetKeyDown(KeyCode.E))
        {
            ExitBike();
        }
    }

    void EnterBike()
    {
        isDriving = true;
        interactionUI.SetActive(false);
        
        // Disable player controller + camera
        playerController.enabled = false;
        player.gameObject.SetActive(false);
        CameraManager.Instance.SwitchToBikeCamera();
        // Enable xe
        bikeController.enabled = true;
        engineSound.Play();
        skidSound.Play();
        OpenMap();
        OnEnterBike?.Invoke();
    }
    void OpenMap()
    {
        if (isShowMap) return;
        isShowMap = true;
        var rect = miniMap.GetComponent<RectTransform>();
        rect.DOMove(new Vector3(50, 50, 0), 1f, true).SetEase(Ease.InOutBounce);
    }
    void CloseMap()
    {
        if(!isShowMap) return;
        isShowMap = false;
        var rect = miniMap.GetComponent<RectTransform>();
        rect.DOMove(new Vector3(50, -300, 0), 1f, true).SetEase(Ease.InOutBounce);
    }
    void ExitBike()
    {
        isDriving = false;

        // Enable player lại
        player.gameObject.SetActive(true);
        playerController.enabled = true;
        CameraManager.Instance.SwitchToPlayerCamera();

        // Disable xe
        bikeController.enabled = false;
        engineSound.Stop();
        skidSound.Stop();
        CloseMap();
        OnExitBike?.Invoke();
    }
    void OnTriggerEnter(Collider other)
    {
        // if not playing game => return 
        if (!GameManager.Instance.IsGamePlaying()) return;
        
        if (other.CompareTag("Player"))
        {
            isNear = true;
            if (!isDriving)
                interactionUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // if not playing game => return 
        if (!GameManager.Instance.IsGamePlaying()) return;
        
        if (other.CompareTag("Player"))
        {
            isNear = false;
            interactionUI.SetActive(false);
        }
    }

    public void Setup(GameObject interactionUI,GameObject  miniMap,PlayerController playerController,Transform playerTransform)
    {
        this.interactionUI = interactionUI;
        this.miniMap = miniMap;
        this.playerController = playerController;
        this.player = playerTransform;
        var playerState = GetComponent<PlayerState>();
        playerState.CurrentState = EPlayerState.Available;
    }

    public void OnPlayerDead(bool isDead)
    {
        engineSound.Stop();
        skidSound.Stop();
        CloseMap();
    }
}
