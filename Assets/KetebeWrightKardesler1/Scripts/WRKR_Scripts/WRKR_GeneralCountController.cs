using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KetebeWrightKa
{
    public class WRKR_GeneralCountController : MonoBehaviour
    {
        public static WRKR_GeneralCountController instance;

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
            currentCount = PlayerPrefs.GetInt("WRKRPageData");
        }

        private void FixedUpdate()
        {
            currentCount = Mathf.Min(currentCount, maxPageCount);

            // Değeri kaydet
            PlayerPrefs.SetInt("WRKRPageData", currentCount);
            PlayerPrefs.Save();
        }
    }

}

