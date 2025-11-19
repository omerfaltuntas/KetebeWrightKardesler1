/*
 * ImageSwipper sayfalari degistirmeye yarayan scriptimiz
 */
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
//Namespace degistirilecek

public class ImageSwiper : MonoBehaviour
{
    public List<GameObject> pages;

    public static int choosenPage = 0;
    private Vector3 mousePos;
    private Vector3 pagePos;
    public bool isDragging;
    public bool isDraggable;
    //private SoundsManager _soundsManager;
    //private ReadingsManager _readingsManager;

    void Start()
    {
        //_soundsManager = GetComponent<SoundsManager>();
        //_readingsManager = GetComponent<ReadingsManager>();

        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].SetActive(false);
        }
        choosenPage = 0;
        pages[choosenPage].SetActive(true);

        //if (MagazineManager.instance.isreadedBefore && MagazineManager.instance.wantToContinue)
        //{
        //    for (int i = 0; i < pages.Count; i++)
        //    {
        //        pages[i].SetActive(false);
        //    }
        //    choosenPage = BookVariables.bookVariables.Get_WhichPage();
        //    pages[choosenPage].SetActive(true);
        //    MagazineManager.instance.wantToContinue = false;
        //}
    }

    void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        { // sol tıklandı mı kontrol ediliyor
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // tıklama noktasından bir ışın oluşturuluyor
            RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction); // ışınla çarpışan tüm nesneleri bulmak için RaycastAll kullanılıyor
            isDraggable = true;
            if (hits.Length > 0)
            { // en az bir nesne bulunduysa
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.GetComponent<Types>() != null)
                    {
                        if (hit.collider.GetComponent<Types>().BookPage != Pages.BookPages)
                        {
                            isDraggable = false;

                        }
                    }
                }
                foreach (RaycastHit2D hit in hits)
                { // tüm nesneleri döngüye sokuyoruz
                    Debug.Log("Object name: " + hit.collider.gameObject.name);

                    // nesnenin adını yazdırıyoruz

                    if (hit.collider.GetComponent<Types>() != null)
                    {
                        if (hits[0].collider.GetComponent<Types>() != null)
                        {

                            if (hit.collider.GetComponent<Types>().BookPage == Pages.BookPages && hits[0].collider.GetComponent<Types>().BookPage == Pages.BookPages && isDraggable)
                            {
                                if (Camera.main.ScreenToWorldPoint(Input.mousePosition).x > 3 || Camera.main.ScreenToWorldPoint(Input.mousePosition).x < -3)
                                {
                                    choosenPage = pages.IndexOf(hit.collider.gameObject);
                                    pagePos = hit.collider.gameObject.transform.position;
                                    mousePos = hit.collider.gameObject.transform.position -
                                               Camera.main.ScreenToWorldPoint(Input.mousePosition);

                                    isDragging = true;
                                }
                            }
                        }
                    }
                }
            }
        }


        if (isDragging)
        {
            Vector3 farePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + mousePos;

            if (choosenPage == 0 && farePos.x > 2)
            {
                farePos.x = 2f;
            }
            else if (choosenPage == pages.Count - 1 && farePos.x < -2f)
            {
                farePos.x = -2f;
            }

            pages[choosenPage].transform.position = new Vector3(farePos.x, pagePos.y, pagePos.z);

            if (pages[choosenPage].transform.position.x < 0 && pages.Count - choosenPage != 1)
            {
                pages[choosenPage + 1].SetActive(true);
                pages[choosenPage + 1].transform.position = new Vector3(pages[choosenPage].transform.position.x + pages[choosenPage].transform.GetChild(0).GetComponent<SpriteRenderer>().bounds.size.x, pages[choosenPage].transform.position.y);
                //pages[choosenPage + 1].transform.SetParent(pages[choosenPage].transform);
            }
            else
            {
                //pages[choosenPage + 1].SetActive(false);
            }

            if (choosenPage != 0)
            {
                if (pages[choosenPage].transform.position.x > 0)
                {
                    pages[choosenPage - 1].SetActive(true);
                    pages[choosenPage - 1].transform.position = new Vector3(pages[choosenPage].transform.position.x - pages[choosenPage].transform.GetChild(0).GetComponent<SpriteRenderer>().bounds.size.x, pages[choosenPage].transform.position.y);
                    //pages[choosenPage - 1].transform.SetParent(pages[choosenPage].transform);
                }
                else
                {
                    //pages[choosenPage - 1].SetActive(false);
                }
            }
        }


        if (Input.GetMouseButtonUp(0))
        {
            if (choosenPage != -1)
            {
                if (pages[choosenPage].transform.position.x < -2 && pages.Count - choosenPage != 1)
                {
                    if (choosenPage < pages.Count - 1)
                    {
                        choosenPage++;
                    }
                    else
                    {
                        choosenPage = 0;
                    }
                }
                else if (pages[choosenPage].transform.position.x > 2 && choosenPage != 0)
                {
                    if (choosenPage > 0)
                    {
                        choosenPage--;
                    }
                    else
                    {
                        choosenPage = pages.Count - 1;
                    }
                }

                // Sayfa geçişlerini daha yumuşak hale getiriyoruz
                pages[choosenPage].transform.SetParent(gameObject.transform);
                pages[choosenPage].SetActive(true);
                pages[choosenPage].transform.DOMove(new Vector3(0, 0, 0), 0.3f).SetEase(Ease.OutQuart);

                if (choosenPage != 0)
                {
                    pages[choosenPage - 1].transform.DOMove(new Vector3(-pages[choosenPage].transform.GetChild(0).GetComponent<SpriteRenderer>().bounds.size.x, 0, 0), 0.3f).SetEase(Ease.OutQuart);
                }

                if (pages.Count - choosenPage != 1)
                {
                    pages[choosenPage + 1].transform.DOMove(new Vector3(+pages[choosenPage].transform.GetChild(0).GetComponent<SpriteRenderer>().bounds.size.x, 0, 0), 0.3f).SetEase(Ease.OutQuart);
                }

                DOTween.Sequence().AppendInterval(0.2f).OnComplete(() => PagesFixer());
            }
            isDragging = false;
        }

    }

    void PagesFixer()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i] != pages[choosenPage])
            {
                pages[i].SetActive(false);
            }
        }

        //hangi sayfada kaldigimizi sunucuya yazdiriyoruz
        //BookVariables.bookVariables.Set_WhichPage(choosenPage);
    }

    public void RestartBook()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


}
