using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIThemeApplier : MonoBehaviour
{
    // ZOYA dark + amber palette (#F27D26).
    private static readonly Color Panel = new Color(0.035f, 0.020f, 0.012f, 0.98f);
    private static readonly Color PanelAlt = new Color(0.075f, 0.035f, 0.015f, 0.96f);
    private static readonly Color Accent = new Color(0.949f, 0.490f, 0.149f, 1f);
    private static readonly Color AccentHover = new Color(1f, 0.690f, 0.420f, 1f);
    private static readonly Color AccentPressed = new Color(0.898f, 0.290f, 0.047f, 1f);
    private static readonly Color Text = new Color(0.98f, 0.98f, 1f, 1f);
    private static readonly Color MutedText = new Color(0.72f, 0.70f, 0.68f, 1f);
    private static readonly Color Disabled = new Color(0.35f, 0.35f, 0.36f, 0.55f);
    private static readonly Color Track = new Color(0.12f, 0.060f, 0.025f, 1f);

    public GameObject menuPanel;
    public GameObject titleTextObject;
    private static bool bootstrapped;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (bootstrapped) return;
        bootstrapped = true;
        var go = new GameObject("ZOYA UI Theme");
        DontDestroyOnLoad(go);
        go.AddComponent<UIThemeApplier>();
    }

    void Awake() => ApplyAllRuntimeUI();

    void OnEnable() => Invoke(nameof(ApplyAllRuntimeUI), 0.15f);

    void ApplyAllRuntimeUI()
    {
        ApplyLegacyTarget();
        foreach (Canvas canvas in FindObjectsOfType<Canvas>(true))
            if (canvas != null) StyleHierarchy(canvas.gameObject);
    }

    void ApplyLegacyTarget()
    {
        if (menuPanel == null) return;
        StyleHierarchy(menuPanel);
        if (titleTextObject != null)
        {
            var title = titleTextObject.GetComponent<TextMeshProUGUI>();
            if (title != null) title.color = Text;
        }
    }

    void StyleHierarchy(GameObject root)
    {
        if (root == null) return;

        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            if (image == null) continue;
            string n = image.gameObject.name.ToLowerInvariant();
            // Keep avatar thumbnails/icons/artwork intact; recolour structural UI.
            if (n.Contains("background") || n.Contains("panel") || n.Contains("window") ||
                n.Contains("container") || n.Contains("content") || n.Contains("header"))
                image.color = n.Contains("content") ? PanelAlt : Panel;
        }

        foreach (Selectable selectable in root.GetComponentsInChildren<Selectable>(true))
        {
            if (selectable == null) continue;
            var colors = selectable.colors;
            colors.normalColor = selectable is Button ? PanelAlt : Accent;
            colors.highlightedColor = AccentHover;
            colors.pressedColor = AccentPressed;
            colors.selectedColor = Accent;
            colors.disabledColor = Disabled;
            colors.colorMultiplier = 1f;
            selectable.colors = colors;

            if (selectable is Slider slider)
            {
                var bg = slider.transform.Find("Background")?.GetComponent<Image>();
                if (bg != null) bg.color = Track;
                var fill = slider.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
                if (fill != null) fill.color = Accent;
            }

            if (selectable is TMP_Dropdown dropdown)
            {
                var bg = dropdown.GetComponent<Image>();
                if (bg != null) bg.color = PanelAlt;
                var arrow = dropdown.transform.Find("Arrow")?.GetComponent<Image>();
                if (arrow != null) arrow.color = Accent;
            }
        }

        foreach (TextMeshProUGUI text in root.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text == null) continue;
            string n = text.gameObject.name.ToLowerInvariant();
            text.color = (n.Contains("description") || n.Contains("subtitle") || n.Contains("hint"))
                ? MutedText : Text;
        }
    }

    [ContextMenu("Apply ZOYA Theme")]
    public void ApplyTheme() => ApplyAllRuntimeUI();
}
