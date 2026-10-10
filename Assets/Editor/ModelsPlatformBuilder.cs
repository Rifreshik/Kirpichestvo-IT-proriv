using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ModelsPlatformBuilder
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string BoardPath = "Assets/ImportedModels/bio_motherboard.fbx";
    private const string TargetPlatformName = "Cube (6)";
    private const string AssemblyPlatformName = "Cube (9)";
    private const string FinalPlatformName = "Cube (10)";
    private const float TargetZoneX = 250f;
    private const float AssemblyZoneX = 500f;
    private const float FinalZoneX = 750f;
    private const float CameraViewDistance = 150f;

    private static readonly string[] ModelPaths =
    {
        "Assets/ImportedModels/planet_fragment.fbx"
    };

    private static readonly HashSet<string> AssemblyDisplayModelPaths = new HashSet<string>
    {
        BoardPath,
        "Assets/ImportedModels/vials.fbx",
        "Assets/ImportedModels/small_rack.fbx",
        "Assets/ImportedModels/tubaretka.fbx"
    };

    [MenuItem("Tools/Build Four-Zone Scene")]
    public static void BuildFourZoneScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        var sources = new Dictionary<string, GameObject>();
        foreach (string path in ModelPaths)
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null)
            {
                Debug.LogError("Модель не найдена: " + path);
                return;
            }
            sources[path] = source;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.path != ScenePath && activeScene.isDirty)
        {
            Debug.LogError("Сначала сохраните текущую сцену; затем откройте SampleScene и повторите настройку.");
            return;
        }

        Scene scene = activeScene.path == ScenePath
            ? activeScene
            : EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject targetPlatform = GameObject.Find(TargetPlatformName);
        GameObject assemblyPlatform = GameObject.Find(AssemblyPlatformName);
        GameObject finalPlatform = GameObject.Find(FinalPlatformName);
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (targetPlatform == null || assemblyPlatform == null || finalPlatform == null || player == null)
        {
            Debug.LogError("Четырёхзонная сцена требует платформы Cube (6), Cube (9), Cube (10) и игрока с тегом Player.");
            return;
        }

        MoveTargetZone(targetPlatform);
        PlacePlatform(assemblyPlatform.transform, AssemblyZoneX, 0f);
        PlacePlatform(finalPlatform.transform, FinalZoneX, 0f);
        ConfigurePlayerCamera(player);
        ConfigureFinalZone(finalPlatform);
        RemoveAssemblyDisplayModels(scene);
        PlaceModels(scene, sources, targetPlatform, assemblyPlatform);
        EnsureSceneInBuildSettings();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorSceneManager.SetActiveScene(scene);
        Debug.Log("Сцена настроена: старт → стрельба → сборка компьютера → финал.");
    }

    private static void MoveTargetZone(GameObject targetPlatform)
    {
        float offsetX = TargetZoneX - targetPlatform.transform.position.x;
        Vector3 offset = Vector3.right * offsetX;
        targetPlatform.transform.position += offset;

        GameObject entryPlatform = GameObject.Find("Cube (8)");
        if (entryPlatform != null)
            entryPlatform.transform.position += offset;

        foreach (DisappearOnShot target in Object.FindObjectsOfType<DisappearOnShot>(true))
        {
            if (target.gameObject.scene == targetPlatform.scene)
                target.transform.position += offset;
        }
    }

    private static void PlacePlatform(Transform platform, float x, float z)
    {
        Vector3 position = platform.position;
        position.x = x;
        position.z = z;
        platform.position = position;
    }

    private static void ConfigurePlayerCamera(GameObject player)
    {
        Movementtag movement = player.GetComponent<Movementtag>();
        Camera camera = movement != null && movement.cameraTransform != null
            ? movement.cameraTransform.GetComponent<Camera>()
            : player.GetComponentInChildren<Camera>();

        if (camera == null)
        {
            Debug.LogError("У игрока не найдена камера.", player);
            return;
        }

        camera.transform.SetParent(player.transform, false);
        camera.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        camera.transform.localRotation = Quaternion.identity;
        camera.gameObject.tag = "MainCamera";
        camera.farClipPlane = CameraViewDistance;
    }

    private static void ConfigureFinalZone(GameObject finalPlatform)
    {
        Transform triggerTransform = finalPlatform.transform.Find("Final Zone Trigger");
        if (triggerTransform == null)
        {
            GameObject triggerObject = new GameObject("Final Zone Trigger");
            triggerTransform = triggerObject.transform;
            triggerTransform.SetParent(finalPlatform.transform, false);
        }

        triggerTransform.localPosition = new Vector3(0f, 2.5f, 0f);
        triggerTransform.localRotation = Quaternion.identity;
        triggerTransform.localScale = Vector3.one;

        BoxCollider trigger = triggerTransform.GetComponent<BoxCollider>();
        if (trigger == null) trigger = triggerTransform.gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(8f, 5f, 8f);

        if (triggerTransform.GetComponent<FinalZone>() == null)
            triggerTransform.gameObject.AddComponent<FinalZone>();

    }

    private static void RemoveAssemblyDisplayModels(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (!AssemblyDisplayModelPaths.Contains(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root)))
                continue;

            Object.DestroyImmediate(root);
        }
    }

    private static void PlaceModels(
        Scene scene,
        Dictionary<string, GameObject> sources,
        GameObject targetPlatform,
        GameObject assemblyPlatform)
    {
        var instances = new Dictionary<string, GameObject>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
            if (!string.IsNullOrEmpty(path) && sources.ContainsKey(path))
                instances[path] = root;
        }

        float targetFloor = PlatformTop(targetPlatform);
        float assemblyFloor = PlatformTop(assemblyPlatform);
        var positions = new Dictionary<string, Vector3>
        {
            { "Assets/ImportedModels/planet_fragment.fbx", new Vector3(TargetZoneX, targetFloor, 42f) }
        };

        foreach (string path in ModelPaths)
        {
            GameObject model;
            if (!instances.TryGetValue(path, out model))
            {
                model = (GameObject)PrefabUtility.InstantiatePrefab(sources[path], scene);
                model.name = Path.GetFileNameWithoutExtension(path);
            }

            model.transform.position = positions[path];
            float groundY = path == "Assets/ImportedModels/planet_fragment.fbx" ? targetFloor : assemblyFloor;
            PlaceOnSurface(model, groundY);
        }
    }

    private static float PlatformTop(GameObject platform)
    {
        Collider collider = platform.GetComponent<Collider>();
        return collider != null ? collider.bounds.max.y + 0.03f : platform.transform.position.y + 1f;
    }

    private static void PlaceOnSurface(GameObject model, float groundY)
    {
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        if (bounds.min.y < groundY)
            model.transform.position += Vector3.up * (groundY - bounds.min.y);
    }

    private static void EnsureSceneInBuildSettings()
    {
        EditorBuildSettingsScene[] current = EditorBuildSettings.scenes;
        foreach (EditorBuildSettingsScene entry in current)
        {
            if (entry.path == ScenePath)
            {
                entry.enabled = true;
                EditorBuildSettings.scenes = current;
                return;
            }
        }

        var scenes = new EditorBuildSettingsScene[current.Length + 1];
        current.CopyTo(scenes, 0);
        scenes[current.Length] = new EditorBuildSettingsScene(ScenePath, true);
        EditorBuildSettings.scenes = scenes;
    }
}
