using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button creditButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button exitButton;
    
    [SerializeField] private GameObject menuSelection;
    
    private void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonClicked);
        creditButton.onClick.AddListener(OnCreditButtonClicked);
        settingButton.onClick.AddListener(OnSettingButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    private void HideMainMenu()
    {
        menuSelection.SetActive(false);
    }
    private void OnPlayButtonClicked()
    {
        GameManager.Instance.SetGameState(GameState.GamePlaying);
        
        CameraManager.Instance.SwitchToPlayerCamera();
        HideMainMenu();
    }

    private void OnCreditButtonClicked()
    {
        Debug.Log("Credit button clicked");
    }
    private void OnSettingButtonClicked()
    {
        Debug.Log("Setting button clicked");
    }

    private void OnExitButtonClicked()
    {
        // To-DO: Quit delay
        Application.Quit();
    }
}
