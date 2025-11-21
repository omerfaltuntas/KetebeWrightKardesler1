using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KetebeWrightKa
{
    public class WRKR_PageCountController : MonoBehaviour
    {
        public static WRKR_PageCountController instance;

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
            if (WRKR_GeneralCountController.instance.currentCount < pageCount)
                WRKR_GeneralCountController.instance.currentCount = pageCount;
        }

    }

}


