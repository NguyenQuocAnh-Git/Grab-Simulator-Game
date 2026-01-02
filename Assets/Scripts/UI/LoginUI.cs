using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [Header("UI Elements")]   
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField otpInput;
    [SerializeField] private Button requestOtpButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject loginPanel;

    [Header("Animation Settings")]
    [SerializeField] private float panelSlideDistance = 450f;
    [SerializeField] private float fieldOffsetY = 120f;
    [SerializeField] private float tweenDuration = 0.4f;
    [SerializeField] private float messageMoveUp = 50f;

    [Header("UI Elements Loading bar")]
    [SerializeField] private Image loadingFill; // 0->1
    [SerializeField] private Image loadingRect;
    [SerializeField] private TMP_Text loadingText; // show percentage loading
    [SerializeField] private float loadingDuration = 1.5f;
    [SerializeField] private float loadingDelay = 0.5f;

    [Header("Transition")]
    [SerializeField] private string mainSceneName = "Main";
    [SerializeField] private Image blackOverlay; // full-screen image for fade
    [SerializeField] private float fadeToBlackDuration = 0.5f;
    [SerializeField] private float fadeFromBlackDuration = 0.6f;
    [SerializeField] private bool keepOverlayBetweenScenes = true;

    private RectTransform panelRect;
    private RectTransform emailRect;
    private RectTransform otpRect;
    private RectTransform loginRect;
    private Canvas loginCanvas;
    private CanvasGroup otpCanvasGroup;
    private CanvasGroup loginCanvasGroup;
    private CanvasGroup messageCanvasGroup;
    private CanvasGroup loadingCanvasGroup;
    private Sequence messageSequence;
    private Vector2 otpTargetPos;
    private bool otpRevealed;

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        requestOtpButton.onClick.AddListener(OnRequestOtpButtonClicked);
        CacheAndSetup();
        StartGame();
    }
    private bool CheckTokenValid()
    {
        return TokenManager.Instance.HasValidToken;
    }
    private async void StartGame()
    {
        if(CheckTokenValid())
        {
            await HandlePostLoginSequence();
        }else
        {
            PlayPanelIntro();
        }
    }
    private void CacheAndSetup()
    {
        panelRect = loginPanel.GetComponent<RectTransform>();
        emailRect = emailInput.GetComponent<RectTransform>();
        otpRect = otpInput.GetComponent<RectTransform>();
        loginRect = loginButton.GetComponent<RectTransform>();
        loginCanvas = loginPanel.GetComponentInParent<Canvas>();

        otpCanvasGroup = RequireCanvasGroup(otpInput.gameObject);
        loginCanvasGroup = RequireCanvasGroup(loginButton.gameObject);
        messageCanvasGroup = RequireCanvasGroup(messageText.gameObject);
        if (loadingRect != null)
        {
            loadingCanvasGroup = RequireCanvasGroup(loadingRect.gameObject);
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.blocksRaycasts = false;
        }
        if (loadingFill != null)
        {
            loadingFill.fillAmount = 0f;
            loadingFill.gameObject.SetActive(false);
        }
        if (loadingRect != null)
        {
            loadingRect.gameObject.SetActive(false);
        }
        if (loadingText != null)
        {
            loadingText.text = string.Empty;
            loadingText.gameObject.SetActive(false);
        }
        if (blackOverlay != null)
        {
            var c = blackOverlay.color;
            blackOverlay.color = new Color(c.r, c.g, c.b, 0f);
            blackOverlay.raycastTarget = false;
            if (keepOverlayBetweenScenes && blackOverlay.canvas != null)
            {
                DontDestroyOnLoad(blackOverlay.canvas.gameObject);
            }
        }

        otpRect.anchoredPosition = emailRect.anchoredPosition;
        otpTargetPos = emailRect.anchoredPosition + new Vector2(0f, -fieldOffsetY);
        otpCanvasGroup.alpha = 0f;
        otpInput.interactable = false;
        otpCanvasGroup.blocksRaycasts = false;

        loginCanvasGroup.alpha = 0f;
        loginRect.sizeDelta = new Vector2(0f, loginRect.sizeDelta.y);
        loginButton.interactable = false;
        loginCanvasGroup.blocksRaycasts = false;

        messageCanvasGroup.alpha = 0f;
        AnchorMessageTop();
    }

    private void PlayPanelIntro()
    {
        if (panelRect == null) return;
        panelRect.anchoredPosition = new Vector2(panelSlideDistance, 0f);
        panelRect.DOAnchorPos(Vector2.zero, 0.6f).SetEase(Ease.OutQuad);
    }

    private async void OnLoginButtonClicked()
    {
        string email = emailInput.text;
        string otp = otpInput.text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp))
        {
            ShowMessage("Please enter email and OTP", true);
            return;
        }

        loginButton.interactable = false;
        ShowMessage("Verify ...", false);

        var response = await AuthService.Instance.VerifyOtpAsync(email, otp);
        if (response.success)
        {
            ShowMessage("Success", false);
            await HandlePostLoginSequence();
        }
        else
        {
            ShowMessage(response.message ?? "OTP invalid", true);
        }

        loginButton.interactable = true;
    }
    private async void OnRequestOtpButtonClicked()
    {
        string email = emailInput.text;
        if (string.IsNullOrWhiteSpace(email))
        {
            ShowMessage("Please enter email", true);
            return;
        }

        requestOtpButton.interactable = false;
        ShowMessage("Send OTP...", false);

        var response = await AuthService.Instance.RequestOtpAsync(email);
        if (response.success)
        {
            ShowMessage("sent OTP. Please check your email", false);
            RevealOtpAndLogin();
        }
        else
        {
            ShowMessage(response.message ?? "Sent OTP fail", true);
        }

        requestOtpButton.interactable = true;
    }

    private void RevealOtpAndLogin()
    {
        if (otpRevealed) return;
        otpRevealed = true;

        otpInput.interactable = true;
        otpCanvasGroup.blocksRaycasts = true;
        otpCanvasGroup.DOFade(1f, tweenDuration);
        otpRect.DOAnchorPos(otpTargetPos, tweenDuration).SetEase(Ease.OutQuad);

        loginButton.interactable = true;
        loginCanvasGroup.blocksRaycasts = true;
        loginCanvasGroup.DOFade(1f, tweenDuration * 0.9f);
        loginRect.DOSizeDelta(new Vector2(300f, loginRect.sizeDelta.y), tweenDuration)
            .SetEase(Ease.OutBack);
    }

    private void ShowMessage(string text, bool isError)
    {
        if (messageText == null) return;

        messageSequence?.Kill();
        messageText.text = text;
        messageText.color = isError ? new Color32(227, 52, 47, 255) : new Color32(56, 193, 114, 255);

        var rect = messageText.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0f, 0f);
        messageCanvasGroup.alpha = 0f;

        messageSequence = DOTween.Sequence();
        messageSequence.Append(rect.DOAnchorPosY(messageMoveUp, 0.35f).SetEase(Ease.OutQuad));
        messageSequence.Join(messageCanvasGroup.DOFade(1f, 0.35f));
        messageSequence.AppendInterval(2f);
        messageSequence.Append(messageCanvasGroup.DOFade(0f, 0.25f));
    }

    private CanvasGroup RequireCanvasGroup(GameObject target)
    {
        if (target.TryGetComponent<CanvasGroup>(out var group))
        {
            return group;
        }

        return target.AddComponent<CanvasGroup>();
    }

    private void AnchorMessageTop()
    {
        if (messageText == null) return;
        var rect = messageText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1f); 
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, 0f);
    }

    private async Task HandlePostLoginSequence()
    {
        loginButton.interactable = false;
        requestOtpButton.interactable = false;
        emailInput.interactable = false;
        otpInput.interactable = false;

        await System.Threading.Tasks.Task.Delay(1500);
        await SlidePanelOut();
        await RunFakeLoading();
        await FadeToBlackAndLoadScene();
    }

    private Task SlidePanelOut()
    {
        if (panelRect == null)
        {
            return Task.CompletedTask;
        }

        var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
        panelRect.DOAnchorPos(new Vector2(panelSlideDistance, 0f), tweenDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => tcs.TrySetResult(true));
        return tcs.Task;
    }

    private Task RunFakeLoading()
    {
        if (loadingFill == null || loadingCanvasGroup == null)
        {
            return Task.CompletedTask;
        }

        loadingFill.fillAmount = 0f;
        loadingFill.gameObject.SetActive(true);
        if (loadingRect != null) loadingRect.gameObject.SetActive(true);
        if (loadingText != null)
        {
            loadingText.text = "0%";
            loadingText.gameObject.SetActive(true);
        }

        loadingCanvasGroup.blocksRaycasts = true;
        loadingCanvasGroup.alpha = 1f;

        var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
        DOTween.To(() => loadingFill.fillAmount, v =>
        {
            loadingFill.fillAmount = v;
            if (loadingText != null)
            {
                loadingText.text = $"{Mathf.RoundToInt(v * 100f)}%";
            }
        }, 1f, loadingDuration)
        .SetDelay(loadingDelay)
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            if (loadingText != null) loadingText.text = "100%";
            loadingCanvasGroup.blocksRaycasts = false;
            loadingFill.gameObject.SetActive(false);
            if (loadingRect != null) loadingRect.gameObject.SetActive(false);
            if (loadingText != null) loadingText.gameObject.SetActive(false);
            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    private async Task FadeToBlackAndLoadScene()
    {
        if (blackOverlay != null)
        {
            await FadeImage(blackOverlay, 1f, fadeToBlackDuration);
        }

        if (!string.IsNullOrEmpty(mainSceneName))
        {
            var op = SceneManager.LoadSceneAsync(mainSceneName);
            if (op != null)
            {
                while (!op.isDone)
                {
                    await System.Threading.Tasks.Task.Yield();
                }
            }
        }
        HideLoginCanvas();
        if (blackOverlay != null)
        {
            await FadeImage(blackOverlay, 0f, fadeFromBlackDuration);
        }
    }

    private System.Threading.Tasks.Task FadeImage(Image image, float targetAlpha, float duration)
    {
        if (image == null || duration <= 0f)
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }

        var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
        image.DOFade(targetAlpha, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() => tcs.TrySetResult(true));
        return tcs.Task;
    }

    private void HideLoginCanvas()
    {
        if (loginCanvas != null)
        {
            loginCanvas.gameObject.SetActive(false);
            Destroy(loginCanvas.gameObject);
            return;
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
