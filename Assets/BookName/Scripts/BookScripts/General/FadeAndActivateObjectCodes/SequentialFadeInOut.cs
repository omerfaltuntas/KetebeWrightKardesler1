using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
//Atanan objeleri sirasiyla fade-in veya fade-out yapmaya calısan kod.

public class SequentialFadeInOut : MonoBehaviour
{
    public enum FadeDirection
    {
        FadeIn,
        FadeOut
    }

    [Header("Fade Targets")]
    [Tooltip("Sırayla fade edilecek GameObject gruplarının listesi. Her bir GameObject ve altındaki tüm SpriteRenderer'lar birlikte fade olur.")]
    public List<GameObject> objectGroupsToFade = new List<GameObject>();

    [Header("Fade Settings")]
    [Tooltip("Animasyonun yönü (Görünür yap / Görünmez yap).")]
    public FadeDirection fadeDirection = FadeDirection.FadeIn;
    [Tooltip("Her bir grubun fade olma süresi.")]
    public float individualFadeDuration = 0.5f;

    [Header("Timing")]
    [Tooltip("Dizi başlamadan önceki başlangıç gecikmesi.")]
    public float initialDelay = 0f;
    [Tooltip("Bir grup fade işlemini bitirdikten sonra bir sonraki grubun başlaması için beklenecek süre.")]
    public float intervalBetweenFades = 0.2f;

    [Header("Looping (Optional)")]
    [Tooltip("Eğer işaretlenirse, dizi tamamlandıktan sonra tekrar başlar ve script devre dışı bırakılana kadar devam eder.")]
    public bool loop = false;

    [Header("Tutorial Trigger (Optional)")]
    [Tooltip("İşaretlenirse, dizi yalnızca belirtilen Tutorial Item tamamlandıktan veya atlandıktan sonra başlar.")]
    public bool triggerAfterTutorial = false;
    [Tooltip("Bu dizinin beklemesi gereken TutorialItem'ı atayın.")]
    public TutorialItem waitForTutorialItem;

    // --- Private Fields ---
    private Dictionary<SpriteRenderer, Color> initialColors = new Dictionary<SpriteRenderer, Color>();
    private Coroutine managementCoroutine;
    private List<Tween> activeTweens = new List<Tween>();

    void Awake()
    {
        if (objectGroupsToFade == null || objectGroupsToFade.Count == 0)
        {
            enabled = false;
            return;
        }

        // Tüm sprite'ları ve başlangıç renklerini topla
        initialColors.Clear();
        foreach (var group in objectGroupsToFade)
        {
            if (group != null)
            {
                var renderers = group.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var sr in renderers)
                {
                    if (sr != null && !initialColors.ContainsKey(sr))
                    {
                        initialColors.Add(sr, sr.color);
                    }
                }
            }
        }

        // Zamanlamaları doğrula
        individualFadeDuration = Mathf.Max(0.01f, individualFadeDuration);
        initialDelay = Mathf.Max(0, initialDelay);
        intervalBetweenFades = Mathf.Max(0, intervalBetweenFades);
    }

    void OnEnable()
    {
        if (objectGroupsToFade == null || objectGroupsToFade.Count == 0) return;

        if (managementCoroutine != null) { StopCoroutine(managementCoroutine); managementCoroutine = null; }
        KillActiveTweens();

        ResetSpritesToStartState();

        if (triggerAfterTutorial && waitForTutorialItem != null)
        {
            managementCoroutine = StartCoroutine(WaitForTutorialAndThenStartSequence());
        }
        else
        {
            managementCoroutine = StartCoroutine(DelayedStartSequence(initialDelay));
        }
    }

    void OnDisable()
    {
        if (managementCoroutine != null) { StopCoroutine(managementCoroutine); managementCoroutine = null; }
        KillActiveTweens();

        foreach (var entry in initialColors)
        {
            if (entry.Key != null)
            {
                entry.Key.color = entry.Value;
            }
        }
    }

    private void KillActiveTweens()
    {
        foreach (var tween in activeTweens)
        {
            if (tween != null && tween.IsActive()) tween.Kill(false);
        }
        activeTweens.Clear();
    }

    private void ResetSpritesToStartState()
    {
        foreach (var entry in initialColors)
        {
            if (entry.Key != null)
            {
                Color c = entry.Value;
                c.a = (fadeDirection == FadeDirection.FadeIn) ? 0f : entry.Value.a;
                entry.Key.color = c;
            }
        }
    }

    private IEnumerator DelayedStartSequence(float waitTime)
    {
        if (waitTime > 0) yield return new WaitForSeconds(waitTime);
        yield return StartCoroutine(StartFadeSequence());
    }

    private IEnumerator WaitForTutorialAndThenStartSequence()
    {
        yield return null;
        if (waitForTutorialItem == null || waitForTutorialItem.IsSkipped)
        {
            yield return StartCoroutine(DelayedStartSequence(this.initialDelay));
            yield break;
        }
        if (!waitForTutorialItem.IsActive)
        {
            yield return new WaitUntil(() => waitForTutorialItem.IsActive || waitForTutorialItem.IsSkipped);
        }
        if (waitForTutorialItem.IsSkipped)
        {
            yield return StartCoroutine(DelayedStartSequence(this.initialDelay));
            yield break;
        }
        yield return new WaitUntil(() => !waitForTutorialItem.IsActive || waitForTutorialItem.IsSkipped);
        yield return StartCoroutine(DelayedStartSequence(this.initialDelay));
    }

    private IEnumerator StartFadeSequence()
    {
        while (true)
        {
            // Döngü her başladığında başlangıç gecikmesini uygula.
            if (initialDelay > 0)
            {
                yield return new WaitForSeconds(initialDelay);
            }

            foreach (var group in objectGroupsToFade)
            {
                if (group == null) continue;

                var renderers = group.GetComponentsInChildren<SpriteRenderer>(true);
                if (renderers.Length == 0) continue;

                // Bu grup için bir DOTween Sequence oluştur. Bu, tüm fade'lerin senkronize olmasını sağlar.
                Sequence groupSequence = DOTween.Sequence();

                foreach (var sr in renderers)
                {
                    if (sr != null && initialColors.ContainsKey(sr))
                    {
                        float targetAlpha = (fadeDirection == FadeDirection.FadeIn) ? initialColors[sr].a : 0f;
                        // Hedef alpha 0 ise, orijinal rengin alpha'sını kullan, değilse 1 yap.
                        if (fadeDirection == FadeDirection.FadeIn && targetAlpha < 0.01f)
                        {
                            targetAlpha = 1.0f;
                        }

                        // Sequence'e bu sprite'ın fade animasyonunu ekle. '0' pozisyonu, hepsinin aynı anda başlamasını sağlar.
                        groupSequence.Insert(0, sr.DOFade(targetAlpha, individualFadeDuration));
                    }
                }

                activeTweens.Add(groupSequence);
                yield return groupSequence.WaitForCompletion(); // Grubun animasyonunun bitmesini bekle

                if (intervalBetweenFades > 0)
                    yield return new WaitForSeconds(intervalBetweenFades); // Sonraki grup için bekle
            }

            if (!loop)
                break;

            // Döngü aktifse, bir sonraki döngüden önce sprite'ları başlangıç durumuna sıfırla
            // ve bir sonraki döngüye geçmeden önce bir frame bekle.
            // Bu, Reset'in görsel olarak işlenmesine olanak tanır.
            if (loop)
            {
                ResetSpritesToStartState();
                yield return null;
            }
        }
    }
}

