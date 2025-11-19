using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PageCountController : MonoBehaviour
{
    public static PageCountController instance;

    [Header("Page Count")]
    public int pageCount;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void OnEnable()
    {
        if (GeneralCountController.instance.currentCount < pageCount)
            GeneralCountController.instance.currentCount = pageCount;
    }

}


