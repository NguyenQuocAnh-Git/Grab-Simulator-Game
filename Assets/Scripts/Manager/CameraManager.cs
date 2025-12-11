using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    
    [SerializeField] private CinemachineVirtualCamera _playerCamera;
    [SerializeField] private CinemachineVirtualCamera _bikeCamera;
    [SerializeField] private CinemachineVirtualCamera _mainMenuCamera;
    
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void SwitchToPlayerCamera()
    {
        _bikeCamera.Priority = 0;
        _mainMenuCamera.Priority = 0;
        _playerCamera.Priority = 10;
    }

    public void SwitchToBikeCamera()
    {
        _playerCamera.Priority = 0;
        _mainMenuCamera.Priority = 0;
        _bikeCamera.Priority = 10;
    }

    public void SwitchToMainMenuCamera()
    {
        _playerCamera.Priority = 0;
        _bikeCamera.Priority = 0;
        _mainMenuCamera.Priority = 10;
    }
}
