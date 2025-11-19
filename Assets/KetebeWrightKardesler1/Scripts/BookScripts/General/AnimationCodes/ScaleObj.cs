using UnityEngine;
using DG.Tweening;
using System.Collections;
using System; // Required for Array.IndexOf
//Objenin scale animasyonun calismasini saglayan kod. X-Y-Z degerinde degisken saglanabilir. Loopa alinabilir.
public class ScaleObj : MonoBehaviour
{
    [Header("Scale Settings")]
    [Tooltip("The target local scale the object will animate to.")]
    public Vector3 targetScale = new Vector3(1.5f, 1.5f, 1.5f);
    [Tooltip("The duration (in seconds) of the scaling animation.")]
    public float duration = 1.0f;
    [Tooltip("Delay AFTER the previous animation in the chain completes, before this one starts.")]
    public float delay = 0f;
    [Tooltip("The easing function for the scaling animation.")]
    public Ease scaleEase = Ease.OutQuad;

    [Header("Looping (Optional)")]
    [Tooltip("Set to true to make the scale animation loop (e.g., pulse). This will prevent the chain from continuing.")]
    public bool loop = false;
    [Tooltip("Type of loop if 'loop' is true (e.g., Yoyo for pulsing, Restart to scale up repeatedly).")]
    public LoopType loopType = LoopType.Yoyo;
    [Tooltip("Number of loops. -1 for infinite. Only active if 'loop' is true.")]
    public int loops = -1;

    [Header("Tutorial Trigger (Optional - ONLY for the FIRST script)")]
    [Tooltip("If checked, the ENTIRE chain of animations will wait for this tutorial. Only the first script's settings are used.")]
    public bool triggerAfterTutorial = false;
    public TutorialItem waitForTutorialItem;

    // --- Private Fields ---
    private Vector3 initialLocalScale;
    private Tween currentScaleTween;
    private Coroutine controlCoroutine; // Renamed from animationCoroutine for clarity

    void Awake()
    {
        initialLocalScale = transform.localScale;

        if (duration <= 0)
        {
            Debug.LogWarning($"KP_ScaleObj on {gameObject.name}: Duration is zero or negative. Defaulting to 1.0s.");
            duration = 1.0f;
        }
        delay = Mathf.Max(0, delay);
    }

    void OnEnable()
    {
        // Reset state for all instances.
        transform.localScale = initialLocalScale;

        if (controlCoroutine != null) { StopCoroutine(controlCoroutine); controlCoroutine = null; }
        KillCurrentTween();

        // --- Chain Logic ---
        var allInstances = GetComponents<ScaleObj>();
        int myIndex = Array.IndexOf(allInstances, this);

        // ONLY the first script (leader) is responsible for starting the chain.
        if (myIndex == 0)
        {
            controlCoroutine = StartCoroutine(InitialChainStart());
        }
        // Follower scripts (myIndex > 0) do nothing on their own. They wait to be triggered.
    }

    void OnDisable()
    {
        if (controlCoroutine != null) { StopCoroutine(controlCoroutine); controlCoroutine = null; }
        KillCurrentTween();
        transform.localScale = initialLocalScale;
    }

    // This is the initial entry point, ONLY called by the leader (first script).
    private IEnumerator InitialChainStart()
    {
        // The whole chain waits for the first script's tutorial settings.
        if (triggerAfterTutorial && waitForTutorialItem != null)
        {
            yield return null;
            if (waitForTutorialItem.IsSkipped) { /* continue */ }
            else
            {
                if (!waitForTutorialItem.IsActive)
                {
                    yield return new WaitUntil(() => waitForTutorialItem.IsActive || waitForTutorialItem.IsSkipped);
                }
                if (waitForTutorialItem.IsSkipped) { /* continue */ }
                else
                {
                    yield return new WaitUntil(() => !waitForTutorialItem.IsActive || waitForTutorialItem.IsSkipped);
                }
            }
        }

        // After tutorial (if any), trigger the first animation in the chain.
        TriggerMyAnimation();
    }

    // Public entry point for an animation instance to be triggered.
    public void TriggerMyAnimation()
    {
        controlCoroutine = StartCoroutine(DelayedStartScaleAnimation(this.delay));
    }

    private IEnumerator DelayedStartScaleAnimation(float waitTime)
    {
        if (waitTime > 0)
        {
            yield return new WaitForSeconds(waitTime);
        }
        StartActualScaleAnimation();
    }

    private void StartActualScaleAnimation()
    {
        KillCurrentTween();

        if (loop)
        {
            // Yoyo ve Restart döngüleri için her tekrar arasına gecikme ekleyen Sequence kullanımı
            var sequence = DOTween.Sequence();
            if (loopType == LoopType.Yoyo)
            {
                // Yoyo: Hedef scale'e git, bekle, başlangıç scale'ine dön, bekle.
                sequence.Append(transform.DOScale(targetScale, duration).SetEase(scaleEase));
                if (delay > 0) sequence.AppendInterval(delay);
                sequence.Append(transform.DOScale(initialLocalScale, duration).SetEase(scaleEase));
                if (delay > 0) sequence.AppendInterval(delay);
            }
            else // Restart ve diğerleri için
            {
                // Restart: Hedef scale'e git, bekle, anında başa dön.
                sequence.Append(transform.DOScale(targetScale, duration).SetEase(scaleEase))
                        .AppendCallback(() => transform.localScale = initialLocalScale); // Anında başa sar
                if (delay > 0) sequence.AppendInterval(delay);
            }

            sequence.SetLoops(loops);
            currentScaleTween = sequence;
        }
        else
        {
            // Döngü yoksa, standart tek seferlik animasyon
            currentScaleTween = transform.DOScale(targetScale, duration).SetEase(scaleEase);
            // Set the callback for when THIS specific tween completes.
            currentScaleTween.OnComplete(OnMyAnimationComplete);
        }
    }

    private void OnMyAnimationComplete()
    {
        // --- Handoff Logic ---
        var allInstances = GetComponents<ScaleObj>();
        int myIndex = Array.IndexOf(allInstances, this);

        // Check if there is a "next" script in the list.
        if (myIndex != -1 && myIndex + 1 < allInstances.Length)
        {
            // Trigger the next script's animation.
            allInstances[myIndex + 1].TriggerMyAnimation();
        }
    }

    void KillCurrentTween()
    {
        if (currentScaleTween != null && currentScaleTween.IsActive())
        {
            currentScaleTween.Kill(false);
        }
        currentScaleTween = null;
    }
}
