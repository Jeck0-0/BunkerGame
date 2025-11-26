using UnityEngine;
using UnityEngine.UI;
using System;

public class SignatureDrawer : MonoBehaviour
{
    [SerializeField] private RawImage signatureDisplay;
    [SerializeField] private Camera cam;
    [SerializeField] private Color drawColor = Color.black;
    [SerializeField] private int textureSize = 512;
    [SerializeField] private int brushSize = 4;

    private Texture2D texture;
    private bool isSigning;
    private Vector2 prevUV;
    private RectTransform rectTransform;
    private bool signed = false;
    private bool blocked = false;
    private float inputTime;
    private float audioClipsSpace = 0.15f;

    public event Action OnSigned;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        rectTransform = signatureDisplay.rectTransform;
        texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        ClearSignature();
        signatureDisplay.texture = texture;
    }

    void Update()
    {
        if (blocked) return;

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
                inputTime += Time.deltaTime;

                if (inputTime >= audioClipsSpace)
                {
                    AudioManager.Instance.PlayRandomSound(AudioStorage.Instance.GetWritingClips(), 0.9f);
                    inputTime = 0f;
                }

                DrawLine(prevUV, uv);
                prevUV = uv;
            }
        }

        if (Input.GetMouseButtonUp(0) && isSigning)
        {
            isSigning = false;
            signed = true;
            OnSigned?.Invoke();
        }
    }

    bool TryGetUV(out Vector2 uv)
    {
        uv = Vector2.zero;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, cam, out Vector2 localPoint)) return false;

        float u = (localPoint.x / rectTransform.rect.width) + rectTransform.pivot.x;
        float v = (localPoint.y / rectTransform.rect.height) + rectTransform.pivot.y;

        if (u >= 0 && v >= 0 && u <= 1 && v <= 1)
        {
            uv = new Vector2(u, v);
            return true;
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
        signed = false;
        blocked = false;
    }

    public void Block(bool blocking) => blocked = blocking;

    public bool OnSignatureComplete() => signed;
}