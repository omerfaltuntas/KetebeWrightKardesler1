using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DrawingSystem
{
    public class Brush : MonoBehaviour
    {
        [Header("Drawing 2D Tool")]
        public Drawing2DTool drawing2DTool;

        [Header("Brush Settings")]
        public Color brushColor = Color.white;
        public int brushSize;
        public bool isResetOnDisable = true;
        public bool isResetNullCollider = false;

        [Header("Clicked")]
        public bool brushClicked;
        Vector3 startPos;
        bool click = true;

        private void Awake()
        {
            startPos = transform.localPosition;
        }

        private void OnDisable()
        {
            if (isResetOnDisable)
            {
                ResetPosition(); 
            }
        }

        private void OnMouseDown()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero, Mathf.Infinity);
                if (hit.collider != null)
                {
                    if (hit.collider.GetComponent<Brush>() != null)
                    {
                        if (drawing2DTool.activeBrush != null)
                        {
                            drawing2DTool.activeBrush.ResetPosition();

                            hit.collider.GetComponent<BoxCollider2D>().enabled = false;
                            drawing2DTool.activeBrush = hit.collider.GetComponent<Brush>();
                            hit.collider.GetComponent<Brush>().brushClicked = true;
                            brushClicked = true;
                            drawing2DTool.isBrushHand = true;
                            drawing2DTool.SetColor(hit.collider.GetComponent<Brush>().brushColor);
                            drawing2DTool.SetSize(hit.collider.GetComponent<Brush>().brushSize);

                            StartCoroutine(Delay());
                        }
                        else if (drawing2DTool.activeBrush == this)
                        {
                            GetComponent<BoxCollider2D>().enabled = false;
                            drawing2DTool.activeBrush = this;

                            brushClicked = true;
                            drawing2DTool.isBrushHand = true;
                            drawing2DTool.SetColor(brushColor);
                            drawing2DTool.SetSize(brushSize);

                            StartCoroutine(Delay());
                        }
                        else if (drawing2DTool.activeBrush == null)
                        {
                            GetComponent<BoxCollider2D>().enabled = false;
                            drawing2DTool.activeBrush = this;

                            brushClicked = true;
                            drawing2DTool.isBrushHand = true;
                            drawing2DTool.SetColor(brushColor);
                            drawing2DTool.SetSize(brushSize);

                            StartCoroutine(Delay());
                        }
                    }
                }
            }
        }

        IEnumerator Delay()
        {
            click = true;
            yield return new WaitForSeconds(0.1f);
            click = false;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && !click)
            {
                Vector2 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero, Mathf.Infinity);
                if (hit.collider == null)
                {
                    if (isResetNullCollider && brushClicked)
                    {
                        drawing2DTool.activeBrush.brushClicked = false;
                        drawing2DTool.activeBrush.ResetPosition();
                        drawing2DTool.isBrushHand = false;
                        drawing2DTool.activeBrush = null;
                    }
                }
            }

            if (brushClicked)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                mousePosition.z = -1;

                transform.position = new Vector3(mousePosition.x, mousePosition.y, mousePosition.z);
            }
        }

        public void ResetPosition()
        {
            GetComponent<BoxCollider2D>().enabled = true;
            transform.localPosition = startPos;

            brushClicked = false;
        }
    } 
}
