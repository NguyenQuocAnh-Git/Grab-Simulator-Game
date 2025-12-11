using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelGameLose : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private RectTransform  panelGameLose;
    
    [Header("Tweens")]
    [SerializeField] private int showDelayMs = 2000;
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private float coinAnimDuration = 1f;
    [SerializeField] private float popScale = 1.15f;
    [SerializeField] private float popDuration = 0.15f;

    private Vector2 defaultTrans;
    private void Start()
    {
        defaultTrans = panelGameLose.anchoredPosition;
    }
    private void OnEnable()
    {
        OnEndGame(GameState.GameOver);
        playAgainButton.onClick.AddListener(PlayAgain);
        quitButton.onClick.AddListener(QuitToMainMenu);
    }

    private void OnDisable()
    {
        playAgainButton.onClick.RemoveListener(PlayAgain);
        quitButton.onClick.RemoveListener(QuitToMainMenu);
    }
    private void PlayAgain()
    {
        // chặn ko cho spam nút khi animation đang thực hiện
        
        panelGameLose.anchoredPosition = defaultTrans;
        GameManager.Instance.ReplayGame();
    }

    private void QuitToMainMenu()
    {
        panelGameLose.anchoredPosition = defaultTrans;
        GameManager.Instance.ReturnLobby();
    }
    
    private void OnEndGame(GameState gameState)
    {
        // fire-and-forget the async flow; cancellation handled inside
        OnEndGameAsync(gameState).Forget();
    }

    // Async flow using UniTask
    private async UniTask OnEndGameAsync(GameState gameState)
    {
        if (gameState != GameState.GameOver) return;

        var ct = this.GetCancellationTokenOnDestroy();

        // delay (cancellable)
        await UniTask.Delay(showDelayMs, cancellationToken: ct);

        // activate panel
        panelGameLose.gameObject.SetActive(true);
        playAgainButton.enabled = false;
        quitButton.enabled = false;
        
        // animate anchored pos and await tween completion via completion source
        var tcs = new UniTaskCompletionSource();
        var tween = panelGameLose.DOAnchorPos(Vector2.zero, moveDuration).SetEase(Ease.OutBounce);
        tween.OnComplete(() => tcs.TrySetResult());
        await tcs.Task.AttachExternalCancellation(ct); // cancel if destroyed

        // show coins after panel arrives (replace coinGot with real value)
        ShowCoinsText(123); // TO-DO: truyền giá trị thật
        await UniTask.Delay(showDelayMs, cancellationToken: ct);
        playAgainButton.enabled = true;
        quitButton.enabled = true;
    }

    private void ShowCoinsText(int coinGot)
    {
        // Cancel any previous tweens targeting coinsText's number
        DOTween.Kill(coinsText);

        int current = 0;
        DOTween.To(() => current, x => {
                current = x;
                coinsText.text = Mathf.RoundToInt(x).ToString();
            }, coinGot, coinAnimDuration)
            .OnComplete(() => {
                // pop panel scale
                panelGameLose
                    .DOScale(popScale, popDuration)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => panelGameLose.DOScale(1f, popDuration).SetEase(Ease.InBack));
            });
    }
    
}
