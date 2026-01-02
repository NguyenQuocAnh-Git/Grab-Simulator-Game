using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelRanking : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private GameObject rankingTemplate; // Template với 3 TMP_Text: rank, email, value
    [SerializeField] private Transform rankingContainer; // Container để chứa các ranking entries
    [SerializeField] private TMP_Text pressAnyKeyText; // Text hiển thị "Press any key to close" (optional)
    
    [Header("Animation Settings")]
    [SerializeField] private float slideInX = -300f; // Vị trí khi hiển thị
    [SerializeField] private float slideOutX = -2000f; // Vị trí khi ẩn
    [SerializeField] private float slideDuration = 0.5f;
    
    [Header("Colors")]
    [SerializeField] private Color grayColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] private Color whiteColor = Color.white;
    
    [Header("Auto Refresh")]
    [SerializeField] private float refreshIntervalMinutes = 5f;
    
    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;
    private List<GameObject> rankingEntries = new List<GameObject>();
    private Coroutine autoRefreshCoroutine;
    private string currentUserEmail;
    private bool isPanelVisible = false;
    
    private void Awake()
    {
        if (panelRect == null)
        {
            panelRect = GetComponent<RectTransform>();
        }
        
        hiddenPosition = new Vector2(slideOutX, panelRect.anchoredPosition.y);
        visiblePosition = new Vector2(slideInX, panelRect.anchoredPosition.y);
        
        // Bắt đầu ở vị trí ẩn
        panelRect.anchoredPosition = hiddenPosition;
        gameObject.SetActive(false);
    }
    
    private void Start()
    {
        // Subscribe to login event để load leaderboard khi login
        if (AuthService.Instance != null)
        {
            AuthService.Instance.OnLoginSuccess += OnLoginSuccess;
        }
        
        // Load leaderboard nếu đã login
        if (TokenManager.Instance.HasValidToken)
        {
            currentUserEmail = TokenManager.Instance.CurrentEmail;
            _ = LoadLeaderboardAsync();
        }
    }
    
    private void Update()
    {
        // Check if any key is pressed when panel is visible
        if (isPanelVisible && Input.anyKeyDown)
        {
            HidePanel();
        }
    }
    
    private void OnDestroy()
    {
        if (AuthService.Instance != null)
        {
            AuthService.Instance.OnLoginSuccess -= OnLoginSuccess;
        }
        
        if (autoRefreshCoroutine != null)
        {
            StopCoroutine(autoRefreshCoroutine);
        }
    }
    
    private void OnLoginSuccess()
    {
        currentUserEmail = TokenManager.Instance.CurrentEmail;
        _ = LoadLeaderboardAsync();
    }
    
    public void ShowPanel()
    {
        if (gameObject.activeSelf)
        {
            return; // Đã hiển thị rồi
        }
        
        isPanelVisible = true;
        gameObject.SetActive(true);
        panelRect.anchoredPosition = hiddenPosition;
        
        // Show "Press any key to close" text
        if (pressAnyKeyText != null)
        {
            pressAnyKeyText.gameObject.SetActive(true);
        }
        
        // Slide in animation
        panelRect.DOAnchorPos(visiblePosition, slideDuration)
            .SetEase(Ease.OutQuad);
        
        // Refresh leaderboard khi show
        _ = LoadLeaderboardAsync();
    }
    
    public void HidePanel()
    {
        if (!isPanelVisible)
        {
            return; // Đã ẩn rồi
        }
        
        isPanelVisible = false;
        
        // Hide "Press any key to close" text
        if (pressAnyKeyText != null)
        {
            pressAnyKeyText.gameObject.SetActive(false);
        }
        
        // Slide out animation
        panelRect.DOAnchorPos(hiddenPosition, slideDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => {
                gameObject.SetActive(false);
            });
    }
    
    private async Task LoadLeaderboardAsync()
    {
        if (!TokenManager.Instance.HasValidToken)
        {
            Debug.LogWarning("[PanelRanking] Cannot load leaderboard: Not logged in");
            DisplayEmptyLeaderboard();
            return;
        }
        
        try
        {
            var leaderboard = await LeaderboardService.Instance.GetTopPlayersAsync(10, forceRefresh: true);
            
            if (leaderboard != null && leaderboard.entries != null)
            {
                DisplayLeaderboard(leaderboard);
                
                // Start auto-refresh coroutine
                if (autoRefreshCoroutine != null)
                {
                    StopCoroutine(autoRefreshCoroutine);
                }
                autoRefreshCoroutine = StartCoroutine(AutoRefreshCoroutine());
            }
            else
            {
                DisplayEmptyLeaderboard();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PanelRanking] Error loading leaderboard: {ex.Message}");
            DisplayEmptyLeaderboard();
        }
    }
    
    private IEnumerator AutoRefreshCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshIntervalMinutes * 60f);
            
            if (TokenManager.Instance.HasValidToken)
            {
                _ = LoadLeaderboardAsync();
            }
        }
    }
    
    private void DisplayLeaderboard(LeaderboardResponse leaderboard)
    {
        // Clear existing entries
        ClearRankingEntries();
        
        if (leaderboard.entries == null || leaderboard.entries.Count == 0)
        {
            DisplayEmptyLeaderboard();
            return;
        }
        
        // Tìm vị trí của user hiện tại
        int userRank = -1;
        LeaderboardEntry userEntry = null;
        
        for (int i = 0; i < leaderboard.entries.Count; i++)
        {
            if (leaderboard.entries[i].email == currentUserEmail)
            {
                userRank = i;
                userEntry = leaderboard.entries[i];
                break;
            }
        }
        
        // Hiển thị top entries (tối đa 4)
        int displayCount = Mathf.Min(4, leaderboard.entries.Count);
        for (int i = 0; i < displayCount; i++)
        {
            var entry = leaderboard.entries[i];
            CreateRankingEntry(entry.rank, entry.email, entry.value, i, isCurrentUser: entry.email == currentUserEmail);
        }
        
        // Hiển thị "You ..." nếu user không trong top 4
        if (userRank >= 4 || userRank == -1)
        {
            if (userEntry != null)
            {
                CreateRankingEntry(userEntry.rank, "You", userEntry.value, 4, isCurrentUser: true);
            }
            else
            {
                // User không có trong leaderboard, hiển thị "-"
                CreateRankingEntry(-1, "You", 0, 4, isCurrentUser: true, isEmpty: true);
            }
        }
    }
    
    private void DisplayEmptyLeaderboard()
    {
        ClearRankingEntries();
        
        // Hiển thị 5 dòng với "-"
        for (int i = 0; i < 5; i++)
        {
            string label = i == 4 ? "You" : $"#{i + 1}";
            CreateRankingEntry(i + 1, label, 0, i, isEmpty: true);
        }
    }
    
    private void CreateRankingEntry(int rank, string email, long value, int index, bool isCurrentUser = false, bool isEmpty = false)
    {
        if (rankingTemplate == null || rankingContainer == null)
        {
            Debug.LogError("[PanelRanking] rankingTemplate or rankingContainer is not assigned!");
            return;
        }
        
        GameObject entryObj = Instantiate(rankingTemplate, rankingContainer);
        entryObj.SetActive(true);
        
        // Lấy các TMP_Text components
        TMP_Text[] texts = entryObj.GetComponentsInChildren<TMP_Text>();
        if (texts.Length < 3)
        {
            Debug.LogError("[PanelRanking] rankingTemplate should have at least 3 TMP_Text components!");
            Destroy(entryObj);
            return;
        }
        
        TMP_Text rankText = texts[0];
        TMP_Text emailText = texts[1];
        TMP_Text valueText = texts[2];
        
        // Set text
        if (isEmpty)
        {
            rankText.text = rank > 0 ? $"#{rank}" : "-";
            emailText.text = "-";
            valueText.text = "-";
        }
        else
        {
            rankText.text = rank > 0 ? $"#{rank}" : "-";
            emailText.text = email;
            valueText.text = value.ToString();
        }
        
        // Set màu nền (xen kẽ xám và trắng)
        Image bgImage = entryObj.GetComponent<Image>();
        if (bgImage == null)
        {
            bgImage = entryObj.AddComponent<Image>();
        }
        
        bgImage.color = (index % 2 == 0) ? whiteColor : grayColor;
        
        rankingEntries.Add(entryObj);
    }
    
    private void ClearRankingEntries()
    {
        foreach (var entry in rankingEntries)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }
        rankingEntries.Clear();
    }
}
