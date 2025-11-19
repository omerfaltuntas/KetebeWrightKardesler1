using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//Namespace degistirilecek
public class PinchToZoomManager : MonoBehaviour, IPointerDownHandler
{
    public Image zoomBtnImage;
    public Sprite[] zoomSprites;
    private bool zoom;

    public void OnPointerDown(PointerEventData eventData)
    {
        PinchToZoomCamera.instance.ZoomActiverDeactiver();

        zoom = !zoom;
        if (zoom)
        {
            zoomBtnImage.sprite = zoomSprites[1];
        }
        else
        {
            zoomBtnImage.sprite = zoomSprites[0];
        }
    }
}

