using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class PickablePrompt : MonoBehaviour
{
    [Header("ラベル表示")]
    public float labelYOffset = 0.2f;   // コライダー上端からのオフセット
    public float labelScale = 0.0022f;  // ワールド空間Canvasのスケール
    public int fontSize = 28;

    [Header("フォント設定")]
    [SerializeField] TMP_FontAsset fontAsset; // ← Inspectorで割り当て推奨

    Canvas worldCanvas;
    TextMeshProUGUI tmp;
    Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
        CreateLabel();
        HidePrompt();
        UpdateLabelPosition();
    }

    void LateUpdate()
    {
        UpdateLabelPosition();
        BillboardToCamera();
    }

    void CreateLabel()
    {
        var cObj = new GameObject("PickableLabelCanvas");
        cObj.transform.SetParent(transform, false);

        worldCanvas = cObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.sortingOrder = 5000;

        var scaler = cObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;

        cObj.AddComponent<GraphicRaycaster>();
        worldCanvas.transform.localScale = Vector3.one * labelScale;

        // 背景
        var bg = new GameObject("BG");
        var bgRect = bg.AddComponent<RectTransform>();
        var bgImg = bg.AddComponent<Image>();
        bg.transform.SetParent(cObj.transform, false);
        bgRect.sizeDelta = new Vector2(160, 48);
        bgImg.color = new Color(0f, 0f, 0f, 0.6f);

        // テキスト
        var tObj = new GameObject("Text");
        tObj.transform.SetParent(bg.transform, false);
        var tRect = tObj.AddComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = Vector2.zero;
        tRect.offsetMax = Vector2.zero;

        tmp = tObj.AddComponent<TextMeshProUGUI>();

        // フォント設定（Inspector指定 > TMP_Settings デフォルト）
        if (fontAsset != null)
        {
            tmp.font = fontAsset;
        }
        else if (TMP_Settings.defaultFontAsset != null)
        {
            tmp.font = TMP_Settings.defaultFontAsset;
        }
        else
        {
            Debug.LogError("TextMeshPro フォントが設定されていません。Inspectorで指定してください。");
        }

        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;

        // fontSharedMaterial があるときだけアウトラインを設定
        if (tmp.fontSharedMaterial != null)
        {
            tmp.outlineWidth = 0.2f;
            tmp.outlineColor = new Color(0, 0, 0, 0.9f);
        }
    }

    void BillboardToCamera()
    {
        if (!worldCanvas) return;
        var cam = Camera.main;
        if (!cam) return;
        var toCam = worldCanvas.transform.position - cam.transform.position;
        if (toCam.sqrMagnitude > 0.0001f)
            worldCanvas.transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
    }

    void UpdateLabelPosition()
    {
        if (!col || !worldCanvas) return;
        var b = col.bounds;
        var top = new Vector3(b.center.x, b.max.y + labelYOffset, b.center.z);
        worldCanvas.transform.position = top;
    }

    public void ShowPrompt(string text)
    {
        if (!tmp || !worldCanvas) return;
        tmp.text = text;
        worldCanvas.enabled = true;
    }

    public void HidePrompt()
    {
        if (worldCanvas) worldCanvas.enabled = false;
    }
}
