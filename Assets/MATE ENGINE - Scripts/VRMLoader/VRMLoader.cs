using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VRM;
using UniGLTF;
using UniVRM10;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ZOYA companion model loader.
/// The desktop companion is intentionally locked to Carlotta.vrm.
/// The original Mate custom-avatar/model-import surface is retained only for
/// scene compatibility; all non-Carlotta model loading is rejected.
/// </summary>
public class VRMLoader : MonoBehaviour
{
    public Button loadVRMButton;
    public GameObject mainModel;
    public GameObject customModelOutput;
    public RuntimeAnimatorController animatorController;
    public GameObject componentTemplatePrefab;

    private GameObject currentModel;
    private RuntimeGltfInstance currentGltf;
    private AssetBundle currentBundle;
    private bool isLoading;

    private const string CarlottaFileName = "Carlotta.vrm";

    private void Start()
    {
        HideLegacyModelUi();

        // Never expose or activate the original built-in Mate avatar.
        if (mainModel != null)
            mainModel.SetActive(false);

        string carlottaPath = FindCarlottaPath();
        if (!string.IsNullOrEmpty(carlottaPath))
        {
            LoadVRM(carlottaPath);
        }
        else
        {
            Debug.LogError("[ZOYA] Carlotta.vrm was not found. The companion will remain without a model.");
        }
    }

    /// <summary>
    /// Kept for scene compatibility. Custom model importing is disabled.
    /// </summary>
    public void OpenFileDialogAndLoadVRM()
    {
        HideLegacyModelUi();
        Debug.Log("[ZOYA] Custom VRM/model importing is disabled. Only Carlotta.vrm is supported.");
    }

    /// <summary>
    /// Loads Carlotta only. Any other path is rejected.
    /// </summary>
    public async void LoadVRM(string path)
    {
        if (isLoading)
            return;

        if (!IsCarlottaPath(path))
        {
            Debug.LogWarning("[ZOYA] Rejected non-Carlotta model load: " + path);
            return;
        }

        if (!File.Exists(path))
        {
            Debug.LogError("[ZOYA] Carlotta.vrm does not exist: " + path);
            return;
        }

        isLoading = true;

        try
        {
            byte[] fileData = await Task.Run(() => File.ReadAllBytes(path));
            if (fileData == null || fileData.Length == 0)
                throw new InvalidDataException("Carlotta.vrm is empty.");

            GameObject loadedModel = null;

            // Try VRM 1.x first.
            try
            {
                var glbData = new GlbFileParser(path).Parse();
                var vrm10Data = Vrm10Data.Parse(glbData);
                if (vrm10Data != null)
                {
                    using var importer10 = new Vrm10Importer(vrm10Data);
                    var instance10 = await importer10.LoadAsync(new ImmediateCaller());
                    if (instance10.Root != null)
                    {
                        loadedModel = instance10.Root;
                        currentGltf = instance10;
                        loadedModel.AddComponent<GltfInstanceDisposer>().Bind(instance10);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ZOYA] VRM 1.x import failed; trying VRM 0.x: " + ex.Message);
            }

            // Carlotta is currently a VRM 0.x asset, so retain the legacy importer.
            if (loadedModel == null)
            {
                using var gltfData = new GlbBinaryParser(fileData, path).Parse();
                VRMImporterContext importer = null;
                try
                {
                    importer = new VRMImporterContext(new VRMData(gltfData));
                    var instance = await importer.LoadAsync(new ImmediateCaller());
                    if (instance.Root != null)
                    {
                        loadedModel = instance.Root;
                        currentGltf = instance;
                        loadedModel.AddComponent<GltfInstanceDisposer>().Bind(instance);
                    }
                }
                finally
                {
                    importer?.Dispose();
                }
            }

            if (loadedModel == null)
                throw new InvalidOperationException("Carlotta.vrm could not be imported.");

            FinalizeLoadedModel(loadedModel);
        }
        catch (Exception ex)
        {
            Debug.LogError("[ZOYA] Failed to load Carlotta.vrm: " + ex);
        }
        finally
        {
            isLoading = false;
        }
    }

    private void FinalizeLoadedModel(GameObject loadedModel)
    {
        DisableMainModel();
        ClearPreviousCustomModel();

        loadedModel.transform.SetParent(customModelOutput != null ? customModelOutput.transform : transform, false);
        loadedModel.transform.localPosition = Vector3.zero;
        loadedModel.transform.localRotation = Quaternion.identity;
        loadedModel.transform.localScale = Vector3.one;
        currentModel = loadedModel;

        EnableSkinnedMeshRenderers(currentModel);
        AssignAnimatorController(currentModel);
        InjectComponentsFromPrefab(componentTemplatePrefab, currentModel);

        if (MEModLoader.Instance != null)
            MEModLoader.Instance.AssignHandlersForCurrentAvatar(loadedModel);

        StartCoroutine(DelayedRefreshStats());
        StartCoroutine(ReleaseRamAndUnloadAssetsCo());

        if (SaveLoadHandler.Instance != null)
        {
            SaveLoadHandler.Instance.data.selectedModelPath = GetCarlottaPathForSave();
            SaveLoadHandler.Instance.data.enableRandomAvatar = false;
            SaveLoadHandler.Instance.SaveToDisk();
        }

        SettingsHandlerUtility.ReloadAllSettingsHandlers();
        Debug.Log("[ZOYA] Carlotta.vrm loaded. Custom avatar/model loading is locked.");
    }

    public void ResetModel()
    {
        // Reset means reload Carlotta, never restore another/default model.
        ClearPreviousCustomModel(skipRawImageCleanup: true);

        if (SaveLoadHandler.Instance != null)
        {
            SaveLoadHandler.Instance.data.selectedModelPath = GetCarlottaPathForSave();
            SaveLoadHandler.Instance.data.enableRandomAvatar = false;
            SaveLoadHandler.Instance.SaveToDisk();
        }

        string carlottaPath = FindCarlottaPath();
        if (!string.IsNullOrEmpty(carlottaPath))
            LoadVRM(carlottaPath);
    }

    public void ActivateDefaultModel()
    {
        // Compatibility shim: ZOYA has no selectable default avatar.
        ResetModel();
    }

    public GameObject GetCurrentModel()
    {
        return currentModel;
    }

    private string FindCarlottaPath()
    {
        if (SaveLoadHandler.Instance != null)
        {
            string saved = SaveLoadHandler.Instance.data.selectedModelPath;
            if (IsCarlottaPath(saved) && File.Exists(saved))
                return saved;
        }

        string[] candidates =
        {
            Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? string.Empty, CarlottaFileName),
            Path.Combine(Application.persistentDataPath, CarlottaFileName),
            Path.Combine(Application.streamingAssetsPath, CarlottaFileName)
        };

        return candidates.FirstOrDefault(p => !string.IsNullOrEmpty(p) && File.Exists(p));
    }

    private string GetCarlottaPathForSave()
    {
        return FindCarlottaPath() ?? CarlottaFileName;
    }

    private bool IsCarlottaPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        return string.Equals(
            Path.GetFileName(path.Trim()),
            CarlottaFileName,
            StringComparison.OrdinalIgnoreCase);
    }

    private void HideLegacyModelUi()
    {
        if (loadVRMButton != null)
            loadVRMButton.gameObject.SetActive(false);

        // Hide any old buttons whose serialized UnityEvent still points at the
        // legacy model importer/library. This avoids requiring a risky manual
        // scene rewrite while preserving all unrelated UI.
        foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (button == null)
                continue;

            bool legacy = false;
            int count = button.onClick.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                string method = button.onClick.GetPersistentMethodName(i);
                if (method == nameof(OpenFileDialogAndLoadVRM) ||
                    method == "OpenLibrary" ||
                    method == "LoadAvatar" ||
                    method == "LoadVRM")
                {
                    legacy = true;
                    break;
                }
            }

            if (legacy)
                button.gameObject.SetActive(false);
        }

        // Also hide the library panel itself if it is present.
        var library = FindFirstObjectByType<AvatarLibraryMenu>();
        if (library != null)
            library.HideForZoya();
    }

    private void DisableMainModel()
    {
        if (mainModel != null)
            mainModel.SetActive(false);
    }

    private void ClearPreviousCustomModel(bool skipRawImageCleanup = false)
    {
        if (customModelOutput != null)
        {
            foreach (Transform child in customModelOutput.transform)
            {
                CleanupRawImages(child.gameObject);
                Destroy(child.gameObject);
            }
        }

        if (currentBundle != null)
        {
            currentBundle.Unload(true);
            currentBundle = null;
        }

        currentGltf = null;

        if (!skipRawImageCleanup)
            CleanupAllRawImagesInScene();
    }

    private void EnableSkinnedMeshRenderers(GameObject model)
    {
        foreach (var skinnedMesh in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            skinnedMesh.enabled = true;
    }

    private void AssignAnimatorController(GameObject model)
    {
        var animator = model.GetComponentInChildren<Animator>();
        if (animator != null && animatorController != null)
            animator.runtimeAnimatorController = animatorController;
    }

    private void InjectComponentsFromPrefab(GameObject prefabTemplate, GameObject targetModel)
    {
        if (prefabTemplate == null || targetModel == null)
            return;

        var templateObj = Instantiate(prefabTemplate);
        var animator = targetModel.GetComponentInChildren<Animator>();

        foreach (var templateComp in templateObj.GetComponents<MonoBehaviour>())
        {
            var type = templateComp.GetType();
            if (targetModel.GetComponent(type) != null)
                continue;

            var newComp = targetModel.AddComponent(type);
            CopyComponentValues(templateComp, newComp);

            if (animator != null)
            {
                var setAnimMethod = type.GetMethod("SetAnimator", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (setAnimMethod != null)
                    setAnimMethod.Invoke(newComp, new object[] { animator });

                var animatorField = type.GetField("animator", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (animatorField != null && animatorField.FieldType == typeof(Animator))
                    animatorField.SetValue(newComp, animator);
            }
        }

        Destroy(templateObj);
    }

    private void CopyComponentValues(Component source, Component destination)
    {
        var type = source.GetType();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (field.IsDefined(typeof(SerializeField), true) || field.IsPublic)
                field.SetValue(destination, field.GetValue(source));
        }

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.GetSetMethod(true) != null);

        foreach (var prop in props)
        {
            try { prop.SetValue(destination, prop.GetValue(source)); }
            catch { }
        }
    }

    private System.Collections.IEnumerator DelayedRefreshStats()
    {
        yield return null;
        var stats = FindFirstObjectByType<RuntimeModelStats>();
        if (stats != null)
            stats.RefreshNow();
    }

    private System.Collections.IEnumerator ReleaseRamAndUnloadAssetsCo()
    {
        yield return Resources.UnloadUnusedAssets();
        yield return null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private void CleanupRawImages(GameObject obj)
    {
        if (obj == null)
            return;

        foreach (var rawImage in obj.GetComponentsInChildren<RawImage>(true))
            rawImage.texture = null;
    }

    private void CleanupAllRawImagesInScene()
    {
        foreach (var rawImage in FindObjectsByType<RawImage>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            rawImage.texture = null;
    }

    private void ClearPreviousCustomModelForLegacy()
    {
        ClearPreviousCustomModel();
    }
}

public sealed class GltfInstanceDisposer : MonoBehaviour
{
    private UniGLTF.RuntimeGltfInstance inst;

    public void Bind(UniGLTF.RuntimeGltfInstance i)
    {
        inst = i;
    }

    private void OnDestroy()
    {
        try { inst?.Dispose(); } catch { }
    }
}
