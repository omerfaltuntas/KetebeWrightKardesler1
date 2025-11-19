using UnityEngine;

namespace DrawingSystem
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(PolygonCollider2D))]
    public class Drawing2DTool : MonoBehaviour
    {
        [Header("Sprite Settings")]
        public int spriteWidth = 1024;
        public int spriteHeight = 1024;
        public Color backgroundColor = new Color(0, 0, 0, 0);

        [Header("Brush Settings")]
        public int brushSize = 5;
        public Color brushColor = Color.black;

        public bool brushMod = true;
        public bool isBrushHand;
        public Brush activeBrush;

        [Header("Reset Sprite")]
        public bool isResetOnDisable = true;

        //Private Values
        private Texture2D drawingTexture;
        private SpriteRenderer spriteRenderer;
        private Sprite drawingSprite;

        private Vector2Int? lastPixelCoord = null;

        private bool exitCollider;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            drawingTexture = new Texture2D(spriteWidth, spriteHeight, TextureFormat.RGBA32, false);
            drawingTexture.filterMode = FilterMode.Point;

            Color[] pixels = new Color[spriteWidth * spriteHeight];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = backgroundColor;
            }
            drawingTexture.SetPixels(pixels);
            drawingTexture.Apply();

            drawingSprite = Sprite.Create(drawingTexture, new Rect(0, 0, spriteWidth, spriteHeight), new Vector2(0.5f, 0.5f), 100f);
            spriteRenderer.sprite = drawingSprite;
        }

        private void OnDisable()
        {
            if (isResetOnDisable)
            {
                ClearSprite();
            }
        }

        /// <summary>
        /// Changes the brush color to the specified color.
        /// This method is used to set the brush color to be used when drawing.
        /// </summary>
        /// <param name="color">New brush color.</param>
        public void SetColor(Color color)
        {
            brushColor = color;
        }

        /// <summary>
        /// Adjusts the brush size according to the specified value.
        /// This method is used to change the size of the brush to be used when drawing.
        /// </summary>
        /// <param name="size">New brush size (in pixels).</param>
        public void SetSize(int size)
        {
            brushSize = size;
        }

        /// <summary>
        /// Draws in brush size at the specified (x,y) coordinates.
        /// The updateTexture parameter determines whether Texture.Apply() is called at the end of this method.
        /// </summary>
        /// <param name="x">Pixel x coordinate</param>
        /// <param name="y">Pixel y coordinate</param>
        /// <param name="updateTexture">If True, the Texture is updated after drawing.</param>
        public void DrawAt(int x, int y, bool updateTexture = true)
        {
            for (int i = -brushSize / 2; i <= brushSize / 2; i++)
            {
                for (int j = -brushSize / 2; j <= brushSize / 2; j++)
                {
                    int drawX = x + i;
                    int drawY = y + j;

                    if (drawX >= 0 && drawX < spriteWidth && drawY >= 0 && drawY < spriteHeight)
                    {
                        drawingTexture.SetPixel(drawX, drawY, brushColor);
                    }
                }
            }
            if (updateTexture)
            {
                drawingTexture.Apply();
            }
        }

        /// <summary>
        /// Draws the line between two points with the Bresenham algorithm and applies a brush to all points in between.
        /// </summary>
        /// <param name="start">Initial pixel coordinate</param>
        /// <param name="end">End pixel coordinate</param>
        public void DrawLine(Vector2Int start, Vector2Int end)
        {
            int dx = Mathf.Abs(end.x - start.x);
            int dy = Mathf.Abs(end.y - start.y);
            int sx = start.x < end.x ? 1 : -1;
            int sy = start.y < end.y ? 1 : -1;
            int err = dx - dy;

            int x = start.x;
            int y = start.y;

            while (true)
            {
                DrawAt(x, y, false);
                if (x == end.x && y == end.y) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }
            }
            drawingTexture.Apply();
        }

        /// <summary>
        /// Clears the drawings on the sprite.
        /// </summary>
        public void ClearSprite()
        {
            Color[] clearPixels = new Color[spriteWidth * spriteHeight];
            for (int i = 0; i < clearPixels.Length; i++)
            {
                clearPixels[i] = backgroundColor;
            }
            drawingTexture.SetPixels(clearPixels);
            drawingTexture.Apply();
        }

        void OnMouseDrag()
        {
            if (Input.GetMouseButton(0) && !exitCollider)
            {
                if (brushMod && !isBrushHand) return;

                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 localPos = transform.InverseTransformPoint(worldPos);

                float spriteWorldWidth = drawingSprite.bounds.size.x;
                float spriteWorldHeight = drawingSprite.bounds.size.y;
                float pixelsPerUnitX = spriteWidth / spriteWorldWidth;
                float pixelsPerUnitY = spriteHeight / spriteWorldHeight;
                int pixelX = (int)((localPos.x + spriteWorldWidth / 2) * pixelsPerUnitX);
                int pixelY = (int)((localPos.y + spriteWorldHeight / 2) * pixelsPerUnitY);
                Vector2Int currentPixel = new Vector2Int(pixelX, pixelY);

                if (lastPixelCoord == null)
                {
                    DrawAt(currentPixel.x, currentPixel.y);
                    lastPixelCoord = currentPixel;
                }
                else if (lastPixelCoord != currentPixel)
                {
                    DrawLine(lastPixelCoord.Value, currentPixel);
                    lastPixelCoord = currentPixel;
                }
            }
            else
            {
                lastPixelCoord = null;
            }
        }

        private void OnMouseUp()
        {
            if (!Input.GetMouseButton(0))
            {
                if (brushMod && !isBrushHand) return;

                lastPixelCoord = null;
                exitCollider = false;
            }
        }

        private void OnMouseExit()
        {
            if (Input.GetMouseButton(0))
            {
                if (brushMod && !isBrushHand) return;

                lastPixelCoord = null;
                exitCollider = true;
            }
        }
    }

}