using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
//Tikladiğimiz objede, atadigimiz renkler arası gecis yapmayi saglayan kod.

[RequireComponent(typeof(SpriteRenderer))]
public class ColorController : MonoBehaviour
{
    [Header("Color Settings")]
    [Tooltip("Sırayla geçiş yapılacak renklerin listesi. İlk renk olarak objenin başlangıç rengi kullanılır.")]
    public List<Color> colorsToCycle = new List<Color>();

    [Header("Animation Settings")]
    [Tooltip("Bir renkten diğerine geçişin süresi.")]
    public float transitionDuration = 0.5f;
    [Tooltip("Bir sonraki renk geçişinden önce mevcut renkte bekleme süresi.")]
    public float intervalBetweenColors = 1.0f;
    [Tooltip("Renk geçişi için kullanılacak easing fonksiyonu.")]
    public Ease colorEase = Ease.Linear;

    [Header("Timing")]
    [Tooltip("Animasyon başlamadan önceki başlangıç gecikmesi (varsa tutorial beklemesinden sonra).")]
    public float initialDelay = 0f;

    [Header("Looping")]
    [Tooltip("İşaretlenirse, renk döngüsü tamamlandıktan sonra tekrar başlar.")]
    public bool loop = true;

    [Header("Tutorial Trigger (Optional)")]
    [Tooltip("İşaretlenirse, animasyon yalnızca belirtilen Tutorial Item tamamlandıktan veya atlandıktan sonra başlar.")]
    public bool triggerAfterTutorial = false;
    [Tooltip("Bu animasyonun beklemesi gereken TutorialItem'ı atayın.")]
    public TutorialItem waitForTutorialItem;

    // --- Private Fields ---
    private SpriteRenderer spriteRenderer;
    private Color initialColor;
    private Coroutine managementCoroutine;
    private Sequence colorSequence;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"[{gameObject.name}/{nameof(ColorController)}]: SpriteRenderer bulunamadı. Script devre dışı bırakılıyor.", this);
            enabled = false;
            return;
        }

        initialColor = spriteRenderer.color;

        if (colorsToCycle == null || colorsToCycle.Count == 0)
        {
            Debug.LogWarning($"[{gameObject.name}/{nameof(ColorController)}]: Renk listesi boş. Script devre dışı bırakılıyor.", this);
            enabled = false;
            return;
        }

        // Zamanlamaları doğrula
        transitionDuration = Mathf.Max(0, transitionDuration);
        intervalBetweenColors = Mathf.Max(0, intervalBetweenColors);
        initialDelay = Mathf.Max(0, initialDelay);
    }

    void OnEnable()
    {
        if (spriteRenderer == null) return;

        // Önceki coroutine ve tween'leri durdur
        if (managementCoroutine != null) { StopCoroutine(managementCoroutine); managementCoroutine = null; }
        KillCurrentSequence();

        // Rengi başlangıç durumuna sıfırla
        spriteRenderer.color = initialColor;

        // Tetikleme mantığını başlat
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
        KillCurrentSequence();

        // Script devre dışı kaldığında rengi başlangıç değerine döndür
        if (spriteRenderer != null)
        {
            spriteRenderer.color = initialColor;
        }
    }

    private void KillCurrentSequence()
    {
        if (colorSequence != null && colorSequence.IsActive())
        {
            colorSequence.Kill(false);
        }
        colorSequence = null;
    }

    private IEnumerator DelayedStartSequence(float waitTime)
    {
        if (waitTime > 0) yield return new WaitForSeconds(waitTime);
        StartColorSequence();
    }

    private IEnumerator WaitForTutorialAndThenStartSequence()
    {
        yield return null; // TutorialItem durumunun güncellenmesi için bir frame bekle

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

    private void StartColorSequence()
    {
        KillCurrentSequence();

        colorSequence = DOTween.Sequence();

        // Başlangıç renginden listedeki ilk renge geçiş
        colorSequence.Append(spriteRenderer.DOColor(colorsToCycle[0], transitionDuration).SetEase(colorEase));
        colorSequence.AppendInterval(intervalBetweenColors);

        // Listedeki diğer renkler arasında geçiş
        for (int i = 1; i < colorsToCycle.Count; i++)
        {
            colorSequence.Append(spriteRenderer.DOColor(colorsToCycle[i], transitionDuration).SetEase(colorEase));
            colorSequence.AppendInterval(intervalBetweenColors);
        }

        // Döngü aktifse, son renkten başlangıç rengine geri dön
        if (loop)
        {
            colorSequence.Append(spriteRenderer.DOColor(initialColor, transitionDuration).SetEase(colorEase));
            colorSequence.AppendInterval(intervalBetweenColors);
            colorSequence.SetLoops(-1, LoopType.Restart);
        }
    }
}
