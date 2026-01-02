using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button rankingButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button exitButton;
    
    [SerializeField] private GameObject menuSelection;
    [SerializeField] private PanelRanking panelRanking;
    
    private void Start()
    {
        playButton.onClick.AddListener(OnPlayButtonClicked);
        rankingButton.onClick.AddListener(OnRankingButtonClicked);
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

    private void OnRankingButtonClicked()
    {
        if (panelRanking != null)
        {
            panelRanking.ShowPanel();
        }
        else
        {
            Debug.LogWarning("[MainMenuUI] PanelRanking is not assigned!");
        }
    }
    private void OnSettingButtonClicked()
    {
        Debug.Log("Setting button clicked");
    }

    private async void OnExitButtonClicked()
    {
        // Disable button to prevent multiple clicks
        exitButton.interactable = false;
        
        try
        {
            // 1. Save data to server
            var player = GameManager.Instance.GetThisPlayer();
            if (player != null)
            {
                var playerCoin = player.GetComponent<PlayerCoin>();
                if (playerCoin != null)
                {
                    await playerCoin.SaveDataAsync();
                }
            }
            
            // 2. Clear token (logout)
            if (AuthService.Instance != null)
            {
                AuthService.Instance.Logout();
            }
            
            // 3. Load Login scene
            await LoadLoginSceneAsync();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MainMenuUI] Error during exit: {ex.Message}");
            // Still try to load login scene even if save fails
            if (AuthService.Instance != null)
            {
                AuthService.Instance.Logout();
            }
            await LoadLoginSceneAsync();
        }
        finally
        {
            exitButton.interactable = true;
        }
    }
    
    private async Task LoadLoginSceneAsync()
    {
        var asyncOperation = SceneManager.LoadSceneAsync("Login");
        if (asyncOperation != null)
        {
            while (!asyncOperation.isDone)
            {
                await Task.Yield();
            }
        }
    }
}
