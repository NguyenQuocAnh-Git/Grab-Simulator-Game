using UnityEngine;
using System;


public class BikeInteraction : MonoBehaviour
{
    public static event Action OnEnterBike;
    public static event Action OnExitBike;

    public GameObject interactionUI;       // Canvas "E to drive"
    public Camera playerCamera;            // Camera nhân vật
    public Camera bikeCamera;              // Camera xe
    public MonoBehaviour playerController; // Script điều khiển nhân vật
    public MonoBehaviour bikeController;   // Script điều khiển xe
    public Transform player;
    [Header("Audio")]
    public AudioSource engineSound;
    public AudioSource skidSound;

    private bool isNear = false;
    private bool isDriving = false;

    void Start()
    {
        interactionUI.SetActive(false);
        bikeController.enabled = false;
        bikeCamera.gameObject.SetActive(false);

    }

    void Update()
    {
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
            playerCamera.gameObject.SetActive(false);

        // Enable xe
        bikeController.enabled = true;
        bikeCamera.gameObject.SetActive(true);
        engineSound.Play();
        skidSound.Play();
        OnEnterBike?.Invoke();
    }

    void ExitBike()
    {
        isDriving = false;

        // Enable player lại
        player.gameObject.SetActive(true);
        playerController.enabled = true;
        playerCamera.gameObject.SetActive(true);

        // Disable xe
        bikeController.enabled = false;
        bikeCamera.gameObject.SetActive(false);
        engineSound.Stop();
        skidSound.Stop();
        OnExitBike?.Invoke();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNear = true;
            if (!isDriving)
                interactionUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNear = false;
            interactionUI.SetActive(false);
        }
    }
}
