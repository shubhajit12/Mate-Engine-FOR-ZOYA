using Kirurobo;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsHandlerButtons : MonoBehaviour
{
    public Button applyButton;
    public Button resetButton;
    public Button windowSizeButton;
    public Button refreshAppsListButton;

    public SettingsHandlerToggles togglesHandler;
    public SettingsHandlerSliders slidersHandler;
    public SettingsHandlerDropdowns dropdownsHandler;
    public SettingsHandlerAudio audioHandler;
    public SettingsHandlerLights lightsHandler;
    public SettingsHandlerAccessory accessoryHandler;
    public SettingsHandlerBigScreen bigScreenHandler;

    public VRMLoader vrmLoader;
    public GameObject uniWindowControllerObject;
    private UniWindowController uniWindowController;

    private void HideZoyaExcludedSettings()
    {
        // ZOYA keeps Mate's settings/right-click panel and chat system, but
        // removes Mate-only promotional/integration entries from the UI.
        // Do not touch the shared panel roots or any chat objects.
        string[] hiddenLabels =
        {
            "MINECRAFT INTEGRATION",
            "MINECRAFT MESSAGES",
            "STEAM DLCs",
            "STEAM DLCS",
            "DISCORD RICH PRESENCE",
            "DISCORD RPC",
            "DISCORD",
            "FOOD SYSTEM",
            "FOOD"
        };

        foreach (var text in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (text == null) continue;
            var value = text.text?.Trim();
            if (string.IsNullOrEmpty(value)) continue;

            foreach (var label in hiddenLabels)
            {
                if (string.Equals(value, label, System.StringComparison.OrdinalIgnoreCase))
                {
                    // Only hide the immediate UI control containing the label.
                    // Never disable higher-level settings/menu parents.
                    var target = text.transform.parent != null
                        ? text.transform.parent.gameObject
                        : text.gameObject;
                    target.SetActive(false);
                    break;
                }
            }
        }

        // Some integration panels do not expose their title through the same
        // text component. Hide only their named leaf objects, never ancestors.
        string[] hiddenObjectNames =
        {
            "MinecraftPanel",
            "Minecraft Messages",
            "DiscordPresence",
            "Discord RPC",
            "Steam DLCs",
            "STEAM DLCS",
            "FoodPanel",
            "Food System",
            "Food"
        };

        foreach (var transform in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (transform == null) continue;

            foreach (var objectName in hiddenObjectNames)
            {
                if (string.Equals(transform.name, objectName, System.StringComparison.OrdinalIgnoreCase))
                {
                    transform.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }

    private void HideModelManagementButtons()
    {
        // ZOYA uses one fixed companion model: Carlotta.vrm.
        // Hide the original Mate model-management controls without touching
        // the existing VRM loader or Carlotta startup-loading path.
        string[] hiddenLabels = { "MODELS", "VRM/ME", "RESET" };
        foreach (var label in hiddenLabels)
        {
            foreach (var text in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (text == null || !string.Equals(text.text?.Trim(), label, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                var target = text.transform.parent != null ? text.transform.parent.gameObject : text.gameObject;
                target.SetActive(false);
            }
        }
    }

    private void Start()
    {
        HideModelManagementButtons();
        HideZoyaExcludedSettings();
        if (applyButton != null) applyButton.onClick.AddListener(OnApplyClicked);
        if (resetButton != null) resetButton.onClick.AddListener(OnResetClicked);
        if (windowSizeButton != null) windowSizeButton.onClick.AddListener(CycleWindowSize);
        if (refreshAppsListButton != null) refreshAppsListButton.onClick.AddListener(OnRefreshAppsClicked);

        if (uniWindowControllerObject != null)
            uniWindowController = uniWindowControllerObject.GetComponent<UniWindowController>();
        else
            uniWindowController = FindFirstObjectByType<UniWindowController>();
    }

    private void OnApplyClicked()
    {
        togglesHandler?.ApplySettings();
        slidersHandler?.ApplySettings();
        dropdownsHandler?.ApplySettings();
        audioHandler?.ApplySettings();
        lightsHandler?.ApplySettings();
        accessoryHandler?.ApplySettings();
        bigScreenHandler?.ApplySettings();
        SaveLoadHandler.Instance.SaveToDisk();
        SaveLoadHandler.ApplyAllSettingsToAllAvatars();
    }

    private void OnResetClicked()
    {
        togglesHandler?.ResetToDefaults();
        slidersHandler?.ResetToDefaults();
        dropdownsHandler?.ResetToDefaults();
        audioHandler?.ResetToDefaults();
        lightsHandler?.ResetAllLightsToDefault();
        lightsHandler?.ResetAllLightTogglesToDefault();
        accessoryHandler?.ResetToDefaults();
        bigScreenHandler?.ResetToDefaults();
        if (vrmLoader != null) vrmLoader.ResetModel();
        SaveLoadHandler.Instance.SaveToDisk();
    }

    public void CycleWindowSize()
    {
        var data = SaveLoadHandler.Instance.data;
        var controller = uniWindowController ?? UniWindowController.current;
        switch (data.windowSizeState)
        {
            case SaveLoadHandler.SettingsData.WindowSizeState.Normal:
                data.windowSizeState = SaveLoadHandler.SettingsData.WindowSizeState.Big;
                controller.windowSize = new Vector2(2048, 1536); break;
            case SaveLoadHandler.SettingsData.WindowSizeState.Big:
                data.windowSizeState = SaveLoadHandler.SettingsData.WindowSizeState.Small;
                controller.windowSize = new Vector2(768, 512); break;
            case SaveLoadHandler.SettingsData.WindowSizeState.Small:
                data.windowSizeState = SaveLoadHandler.SettingsData.WindowSizeState.Normal;
                controller.windowSize = new Vector2(1536, 1024); break;
        }
        SaveLoadHandler.Instance.SaveToDisk();
    }

    private void OnRefreshAppsClicked()
    {
        var appManager = FindFirstObjectByType<AllowedAppsManager>();
        if (appManager != null) appManager.RefreshUI();
    }
}