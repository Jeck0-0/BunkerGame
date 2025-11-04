using UnityEngine;
using UnityEngine.UI;

public class SignatureDrawer : MonoBehaviour
{
    [SerializeField] private RawImage signatureDisplay;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Color drawColor = Color.black;
    [SerializeField] private int textureSize = 512;
    [SerializeField] private int brushSize = 4;

    private Texture2D texture;
    private bool isSigning;
    private Vector2 prevUV;
    private RectTransform rectTransform;
    private Plane drawPlane;

    void Start()
    {
        rectTransform = signatureDisplay.rectTransform;
        texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        ClearSignature();
        signatureDisplay.texture = texture;

        Vector3 planeOrigin = rectTransform.TransformPoint(rectTransform.rect.center);
        drawPlane = new Plane(rectTransform.forward, planeOrigin);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (TryGetUV(out var uv))
            {
                prevUV = uv;
                isSigning = true;
            }
        }

        if (isSigning && Input.GetMouseButton(0))
        {
            if (TryGetUV(out var uv))
            {
                DrawLine(prevUV, uv);
                prevUV = uv;
            }
        }

        if (Input.GetMouseButtonUp(0) && isSigning)
        {
            isSigning = false;
            OnSignatureComplete();
        }
    }

    bool TryGetUV(out Vector2 uv)
    {
        uv = Vector2.zero;
        Ray ray = uiCamera.ScreenPointToRay(Input.mousePosition);
        if (drawPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 local = rectTransform.InverseTransformPoint(hitPoint);

            float u = (local.x / rectTransform.rect.width) + rectTransform.pivot.x;
            float v = (local.y / rectTransform.rect.height) + rectTransform.pivot.y;

            if (u >= 0 && v >= 0 && u <= 1 && v <= 1)
            {
                uv = new Vector2(u, v);
                return true;
            }
        }
        return false;
    }


    void DrawLine(Vector2 startUV, Vector2 endUV)
    {
        Vector2 start = startUV * textureSize;
        Vector2 end = endUV * textureSize;

        int steps = (int)Vector2.Distance(start, end);
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            int x = Mathf.RoundToInt(Mathf.Lerp(start.x, end.x, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(start.y, end.y, t));
            DrawBrush(x, y);
        }
        texture.Apply(false);
    }

    void DrawBrush(int x, int y)
    {
        for (int i = -brushSize; i <= brushSize; i++)
            for (int j = -brushSize; j <= brushSize; j++)
            {
                if (i * i + j * j > brushSize * brushSize) continue;
                int px = x + i;
                int py = y + j;
                if (px >= 0 && py >= 0 && px < texture.width && py < texture.height)
                    texture.SetPixel(px, py, drawColor);
            }
    }

    public void ClearSignature()
    {
        Color[] clearPixels = new Color[texture.width * texture.height];
        for (int i = 0; i < clearPixels.Length; i++) clearPixels[i] = Color.white;
        texture.SetPixels(clearPixels);
        texture.Apply();
    }

    void OnSignatureComplete()
    {
        Debug.Log("Signed!");
    }
}