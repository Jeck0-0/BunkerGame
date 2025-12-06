using UnityEngine;

public class ScaleTextureWithScreen : MonoBehaviour
{
    [SerializeField] RenderTexture textureToScale;
    [SerializeField] float scaleFactor = 0.7f;

    private int lastWidth;
    private int lastHeight;

    void Update()
    {
        int targetWidth = Mathf.RoundToInt(Screen.width * scaleFactor);
        int targetHeight = Mathf.RoundToInt(Screen.height * scaleFactor);

        if (targetWidth == lastWidth && targetHeight == lastHeight) return;

        lastWidth = targetWidth;
        lastHeight = targetHeight;

        textureToScale.Release();
        textureToScale.width = targetWidth;
        textureToScale.height = targetHeight;
        textureToScale.Create();
    }
}