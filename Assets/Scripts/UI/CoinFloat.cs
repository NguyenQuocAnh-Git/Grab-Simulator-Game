using DG.Tweening;
using TMPro;
using UnityEngine;
using Cinemachine;
public class CoinFloat : MonoBehaviour
{
    [SerializeField] TMP_Text coinGetTxt;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] RectTransform rectTransform;

    Tween seq;

    public void Show(int coin)
    {
        coinGetTxt.text = $"+{coin}";
        canvasGroup.alpha = 0;

        // Face camera ngay khi spawn
        rectTransform.forward = Camera.main.transform.forward;

        float startY = rectTransform.anchoredPosition.y;

        seq?.Kill();
        seq = DOTween.Sequence()
            .Append(canvasGroup.DOFade(1f, 0.3f))
            .Join(rectTransform.DOAnchorPosY(startY + 2f, 1.2f).SetEase(Ease.Linear))
            .Append(canvasGroup.DOFade(0f, 0.3f))
            .OnComplete(() => Destroy(gameObject));
    }
    void LateUpdate()
    {
        var brain = Camera.main.GetComponent<CinemachineBrain>();
        if (brain == null) return;

        transform.rotation = Quaternion.LookRotation(brain.OutputCamera.transform.forward);
    }
}
