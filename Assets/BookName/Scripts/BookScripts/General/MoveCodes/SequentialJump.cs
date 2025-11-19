using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
//Liste icine atadigimiz objeler sirasiyla ziplatmaya calısan kod blogu.
public class SequentialJump : MonoBehaviour
{
    [Header("Jump Targets")]
    [Tooltip("Sırayla zıplatılacak GameObject'lerin listesi.")]
    public List<GameObject> objectsToJump = new List<GameObject>();

    [Header("Jump Settings")]
    [Tooltip("Her bir objenin zıplama yüksekliği.")]
    public float jumpPower = 1f;
    [Tooltip("Her bir objenin zıplama süresi (yukarı ve aşağı).")]
    public float jumpDuration = 0.5f;
    [Tooltip("Zıplama animasyonunun easing tipi.")]
    public Ease jumpEase = Ease.OutQuad;

    [Header("Timing")]
    [Tooltip("Dizi başlamadan önceki başlangıç gecikmesi.")]
    public float initialDelay = 0f;
    [Tooltip("Bir obje zıplamayı bitirdikten sonra bir sonraki objenin zıplamaya başlaması için beklenecek süre.")]
    public float intervalBetweenJumps = 0.2f;

    [Header("Looping (Optional)")]
    [Tooltip("Eğer işaretlenirse, zıplama dizisi tamamlandıktan sonra tekrar başlar ve script devre dışı bırakılana kadar devam eder.")]
    public bool loop = false;

    [Header("Tutorial Trigger (Optional)")]
    [Tooltip("İşaretlenirse, dizi yalnızca belirtilen Tutorial Item tamamlandıktan veya atlandıktan sonra başlar.")]
    public bool triggerAfterTutorial = false;
    [Tooltip("Bu dizinin beklemesi gereken TutorialItem'ı atayın.")]
    public TutorialItem waitForTutorialItem; // TutorialItem sınıfının projenizde tanımlı olduğu varsayılmıştır.

    // --- Private Fields ---
    private List<Vector3> initialLocalPositions = new List<Vector3>();
    private Coroutine managementCoroutine;
    private List<Tween> activeTweens = new List<Tween>();

    void Awake()
    {
        if (objectsToJump == null || objectsToJump.Count == 0)
        {
            Debug.LogWarning($"[{gameObject.name}/OYA_SequentialJump]: Zıplatılacak obje atanmamış. Script devre dışı bırakılıyor.", this);
            enabled = false;
            return;
        }

        // Başlangıç pozisyonlarını sakla
        initialLocalPositions.Clear();
        foreach (var obj in objectsToJump)
        {
            if (obj != null)
            {
                initialLocalPositions.Add(obj.transform.localPosition);
                // Objelerin başlangıç pozisyonlarında olduğundan emin ol (OnEnable da bunu yapacaktır).
                obj.transform.localPosition = initialLocalPositions[initialLocalPositions.Count - 1];
            }
            else
            {
                initialLocalPositions.Add(Vector3.zero); // Null objeler için yer tutucu
            }
        }

        // Zamanlamaları doğrula
        jumpPower = Mathf.Max(0, jumpPower);
        jumpDuration = Mathf.Max(0.01f, jumpDuration); // Süre sıfır olmamalı
        initialDelay = Mathf.Max(0, initialDelay);
        intervalBetweenJumps = Mathf.Max(0, intervalBetweenJumps);
    }

    void OnEnable()
    {
        if (objectsToJump == null || objectsToJump.Count == 0) return;

        // Önceki coroutine'i ve tween'leri durdur
        if (managementCoroutine != null) { StopCoroutine(managementCoroutine); managementCoroutine = null; }
        KillActiveTweens();

        // Tüm objeleri başlangıç pozisyonlarına sıfırla
        for (int i = 0; i < objectsToJump.Count; i++)
        {
            if (objectsToJump[i] != null && i < initialLocalPositions.Count)
            {
                objectsToJump[i].transform.localPosition = initialLocalPositions[i];
            }
        }

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
        KillActiveTweens();

        // Objeleri başlangıç pozisyonlarına sıfırla
        if (objectsToJump != null)
        {
            for (int i = 0; i < objectsToJump.Count; i++)
            {
                if (objectsToJump[i] != null && i < initialLocalPositions.Count)
                {
                    objectsToJump[i].transform.localPosition = initialLocalPositions[i];
                }
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

    private IEnumerator DelayedStartSequence(float waitTime)
    {
        if (waitTime > 0) yield return new WaitForSeconds(waitTime);
        yield return StartCoroutine(StartJumpSequence());
    }

    private IEnumerator WaitForTutorialAndThenStartSequence()
    {
        yield return null; // Bir frame bekle ki TutorialItem'ın durumu güncellenmiş olsun

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

    private IEnumerator StartJumpSequence()
    {
        while (true)
        {
            for (int i = 0; i < objectsToJump.Count; i++)
            {
                GameObject obj = objectsToJump[i];
                if (obj == null) continue;

                Vector3 targetLocalPos = initialLocalPositions[i]; // Zıplama bittiğinde döneceği yer

                // DOLocalJump, objenin mevcut pozisyonundan zıplar ve targetLocalPos'a geri döner.
                // numJumps 1 olduğu için tek bir zıplama yapar.
                Tween jumpTween = obj.transform.DOLocalJump(targetLocalPos, jumpPower, 1, jumpDuration)
                    .SetEase(jumpEase);

                activeTweens.Add(jumpTween);

                yield return jumpTween.WaitForCompletion(); // Zıplama bitene kadar bekle
                yield return new WaitForSeconds(intervalBetweenJumps); // Sonraki zıplama için bekle
            }

            if (!loop)
                break;
        }
    }
}
