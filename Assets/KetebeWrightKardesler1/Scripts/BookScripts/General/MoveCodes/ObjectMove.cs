using UnityEngine;
using DG.Tweening;
using System.Collections;
//Nesnenin başlangıçtaki yerel pozisyonuna verdigimiz deger eklenerek gitmesini saglayan kod blogu. 
public class ObjectMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Nesnenin başlangıçtaki yerel pozisyonuna eklenecek ofset değeri. Nesne, initialPosition + moveOffset pozisyonuna hareket edecektir.")]
    [SerializeField] private Vector3 moveOffset = Vector3.zero;

    [Tooltip("Hareket animasyonunun saniye cinsinden süresi.")]
    [SerializeField] private float duration = 1f;

    [Tooltip("Hareket animasyonu için easing fonksiyonu.")]
    [SerializeField] private Ease easeType = Ease.InOutSine;

    [Header("Timing")]
    [Tooltip("Animasyonun başlamadan önceki gecikmesi (varsa tutorial tetiklemesinden sonra).")]
    [SerializeField] private float delay = 0f;

    [Header("Looping (Optional)")]
    [Tooltip("Hareket animasyonunu döngüye sokmak için işaretleyin.")]
    [SerializeField] private bool loop = false;

    [Tooltip("Döngü aktifse döngü tipi (Örn: Yoyo, Restart).")]
    [SerializeField] private LoopType loopType = LoopType.Yoyo;

    [Tooltip("Döngü sayısı. -1 sonsuz döngü anlamına gelir. Sadece 'loop' aktifse çalışır.")]
    [SerializeField] private int loops = -1;

    [Header("Tutorial Trigger (Optional)")]
    [Tooltip("İşaretlenirse, animasyon bu tutorial'ın tamamlanmasını veya atlanmasını bekler.")]
    [SerializeField] private bool triggerAfterTutorial = false;

    [SerializeField] private TutorialItem waitForTutorialItem;

    // --- Private Fields ---
    private Vector3 initialLocalPosition;
    private Coroutine controlCoroutine;
    private Tween moveTween;

    void Awake()
    {
        initialLocalPosition = transform.localPosition;
        if (duration <= 0) duration = 1f;
    }

    void OnEnable()
    {
        if (controlCoroutine != null)
        {
            StopCoroutine(controlCoroutine);
            controlCoroutine = null;
        }
        KillCurrentTween();

        transform.localPosition = initialLocalPosition;
        if (triggerAfterTutorial && waitForTutorialItem != null)
        {
            controlCoroutine = StartCoroutine(WaitForTutorialAndStart());
        }
        else
        {
            controlCoroutine = StartCoroutine(DelayedStart(delay));
        }
    }

    void OnDisable()
    {
        if (controlCoroutine != null)
        {
            StopCoroutine(controlCoroutine);
            controlCoroutine = null;
        }
        KillCurrentTween();

        transform.localPosition = initialLocalPosition;
    }

    private void KillCurrentTween()
    {
        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill(false);
        }
        moveTween = null;
    }

    private IEnumerator WaitForTutorialAndStart()
    {
        yield return null;
        if (waitForTutorialItem == null || waitForTutorialItem.IsSkipped) { yield return StartCoroutine(DelayedStart(this.delay)); yield break; }
        if (!waitForTutorialItem.IsActive) { yield return new WaitUntil(() => waitForTutorialItem.IsActive || waitForTutorialItem.IsSkipped); }
        if (waitForTutorialItem.IsSkipped) { yield return StartCoroutine(DelayedStart(this.delay)); yield break; }
        yield return new WaitUntil(() => !waitForTutorialItem.IsActive || waitForTutorialItem.IsSkipped);
        yield return StartCoroutine(DelayedStart(this.delay));
    }

    private IEnumerator DelayedStart(float waitTime)
    {
        if (waitTime > 0) { yield return new WaitForSeconds(waitTime); }
        StartMovementAnimation();
    }

    private void StartMovementAnimation()
    {
        KillCurrentTween();
        Vector3 targetPosition = initialLocalPosition + moveOffset;
        moveTween = transform.DOLocalMove(targetPosition, duration)
                             .SetEase(easeType);
        if (loop)
        {
            moveTween.SetLoops(loops, loopType);
        }
    }
}

