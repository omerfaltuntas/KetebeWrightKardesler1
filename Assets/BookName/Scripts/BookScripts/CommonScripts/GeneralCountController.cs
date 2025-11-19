using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralCountController : MonoBehaviour
{
    public static GeneralCountController instance;

    public int currentCount;

    [Header("Max Page Count")]
    public int maxPageCount;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        currentCount = PlayerPrefs.GetInt("BookPageData");
    }

    private void FixedUpdate()
    {
        currentCount = Mathf.Min(currentCount, maxPageCount);

        // Değeri kaydet
        PlayerPrefs.SetInt("BookPageData", currentCount);
        PlayerPrefs.Save();
    }
}

