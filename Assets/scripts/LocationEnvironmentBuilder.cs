using UnityEngine;

public static class LocationEnvironmentBuilder
{
    private const string StartSignName = "Табличка начальной локации";
    private const string AssemblyRoomName = "Интерьер мастерской сборки";
    private const string FinalRoomName = "Интерьер финального зала";
    private static readonly string[] AssemblyDisplayObjects = { "bio_motherboard", "vials", "small_rack", "tubaretka" };

    public static void Build()
    {
        RemoveAssemblyDisplayObjects();
        EnsureFinalZone();
        BuildStartingSign();
        BuildEnclosedRoom(GameObject.Find("Cube (9)"), AssemblyRoomName, false);
        BuildEnclosedRoom(GameObject.Find("Cube (10)"), FinalRoomName, true);
    }

    private static void RemoveAssemblyDisplayObjects()
    {
        foreach (string objectName in AssemblyDisplayObjects)
        {
            GameObject displayObject = GameObject.Find(objectName);
            if (displayObject != null) Object.Destroy(displayObject);
        }
    }

    private static void EnsureFinalZone()
    {
        GameObject platform = GameObject.Find("Cube (10)");
        if (platform == null) return;

        Transform triggerTransform = platform.transform.Find("Final Zone Trigger");
        if (triggerTransform == null)
        {
            GameObject triggerObject = new GameObject("Final Zone Trigger");
            triggerTransform = triggerObject.transform;
            triggerTransform.SetParent(platform.transform, false);
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

    private static void BuildStartingSign()
    {
        if (GameObject.Find(StartSignName) != null) return;

        GameObject rootObject = new GameObject(StartSignName);
        Transform root = rootObject.transform;
        Material panel = MakeMaterial("Табличка — тёмно-синий металл", new Color(0.035f, 0.09f, 0.15f), 0.55f);
        Material frame = MakeMaterial("Рамка — холодный металл", new Color(0.28f, 0.72f, 0.86f), 0.7f,
            new Color(0.04f, 0.22f, 0.3f));

        GameObject board = CreatePiece(root, PrimitiveType.Cube, "Панель Золотого кольца",
            new Vector3(2.5f, 1.9f, 5.66f), new Vector3(9f, 1.15f, 0.16f), panel,
            Quaternion.Euler(0f, 180f, 0f));
        CreatePiece(root, PrimitiveType.Cube, "Акцент вывески",
            new Vector3(2.5f, 2.49f, 5.56f), new Vector3(8.6f, 0.045f, 0.05f), frame);

        var labelObject = new GameObject("Надпись: Золотое кольцо");
        labelObject.transform.SetParent(root, false);
        labelObject.transform.localPosition = board.transform.localPosition +
            board.transform.localRotation * Vector3.forward * 0.095f;
        labelObject.transform.localRotation = board.transform.localRotation;
        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = "ЗОЛОТОЕ КОЛЬЦО";
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 72;
        label.fontStyle = FontStyle.Bold;
        label.characterSize = 0.055f;
        label.color = new Color(0.87f, 0.96f, 1f);
    }

    private static void BuildEnclosedRoom(GameObject platform, string rootName, bool isFinal)
    {
        if (platform == null || GameObject.Find(rootName) != null) return;

        Collider floor = platform.GetComponent<Collider>();
        Bounds floorBounds = floor != null
            ? floor.bounds
            : new Bounds(platform.transform.position, new Vector3(20f, 1f, 20f));
        float surfaceY = floorBounds.max.y + 0.035f;
        float halfWidth = Mathf.Max(7f, floorBounds.extents.x - 0.35f);
        float halfDepth = Mathf.Max(7f, floorBounds.extents.z - 0.35f);
        float height = isFinal ? 13f : 11f;

        var rootObject = new GameObject(rootName);
        rootObject.transform.position = new Vector3(platform.transform.position.x, surfaceY, platform.transform.position.z);
        Transform root = rootObject.transform;

        Color wallColor = isFinal
            ? new Color(0.025f, 0.045f, 0.075f)
            : new Color(0.11f, 0.19f, 0.25f);
        Color roofColor = isFinal
            ? new Color(0.012f, 0.022f, 0.04f)
            : new Color(0.055f, 0.095f, 0.13f);
        Color accentColor = isFinal
            ? new Color(0.95f, 0.63f, 0.2f)
            : new Color(0.25f, 0.77f, 0.9f);
        Material wall = MakeMaterial(rootName + " — стены", wallColor, 0.18f);
        Material roof = MakeMaterial(rootName + " — потолок", roofColor, 0.24f);
        Material accent = MakeMaterial(rootName + " — отделка", accentColor, 0.62f,
            isFinal ? new Color(0.24f, 0.11f, 0.015f) : new Color(0.035f, 0.18f, 0.24f));

        const float thickness = 0.7f;
        CreatePiece(root, PrimitiveType.Cube, "Стена зала — север",
            new Vector3(0f, height * 0.5f, halfDepth - thickness * 0.5f),
            new Vector3(halfWidth * 2f, height, thickness), wall, null, true);
        CreatePiece(root, PrimitiveType.Cube, "Стена зала — юг",
            new Vector3(0f, height * 0.5f, -halfDepth + thickness * 0.5f),
            new Vector3(halfWidth * 2f, height, thickness), wall, null, true);
        CreatePiece(root, PrimitiveType.Cube, "Стена зала — восток",
            new Vector3(halfWidth - thickness * 0.5f, height * 0.5f, 0f),
            new Vector3(thickness, height, halfDepth * 2f), wall, null, true);
        CreatePiece(root, PrimitiveType.Cube, "Стена зала — запад",
            new Vector3(-halfWidth + thickness * 0.5f, height * 0.5f, 0f),
            new Vector3(thickness, height, halfDepth * 2f), wall, null, true);
        CreatePiece(root, PrimitiveType.Cube, "Потолок зала",
            new Vector3(0f, height - 0.1f, 0f),
            new Vector3(halfWidth * 2f + 0.8f, 0.7f, halfDepth * 2f + 0.8f), roof, null, true);

        float pillarOffsetX = halfWidth - 0.55f;
        float pillarOffsetZ = halfDepth - 0.55f;
        for (int xSide = -1; xSide <= 1; xSide += 2)
        for (int zSide = -1; zSide <= 1; zSide += 2)
        {
            CreatePiece(root, PrimitiveType.Cube, "Световая колонна зала",
                new Vector3(xSide * pillarOffsetX, height * 0.5f, zSide * pillarOffsetZ),
                new Vector3(0.32f, height - 0.35f, 0.32f), accent);
        }

        CreateSign(root, isFinal ? "ФИНАЛЬНЫЙ ЗАЛ" : "ЛАБОРАТОРИЯ СБОРКИ",
            new Vector3(0f, height - 1.8f, halfDepth - 0.42f),
            new Vector3(Mathf.Min(14f, halfWidth * 1.45f), 1.35f, 0.18f), accent,
            Quaternion.Euler(0f, 180f, 0f));

        if (isFinal)
            BuildFinalDisplay(root, accent);
        else
            BuildWorkshopDetails(root, accent, halfWidth, halfDepth);

        AddRoomLights(root, isFinal, halfWidth, halfDepth);
    }

    private static void BuildWorkshopDetails(Transform root, Material accent, float halfWidth, float halfDepth)
    {
        CreatePiece(root, PrimitiveType.Cube, "Информационная панель мастерской",
            new Vector3(0f, 3.4f, -halfDepth + 0.48f),
            new Vector3(Mathf.Min(10f, halfWidth), 2.1f, 0.16f), accent);
        CreatePiece(root, PrimitiveType.Cube, "Световая линия мастерской",
            new Vector3(0f, 0.08f, 0f),
            new Vector3(halfWidth * 2f - 1.4f, 0.035f, 0.08f), accent);
    }

    private static void BuildFinalDisplay(Transform root, Material accent)
    {
        CreatePiece(root, PrimitiveType.Cylinder, "Пьедестал финала", new Vector3(0f, 0.18f, -4.2f),
            new Vector3(2.4f, 0.18f, 2.4f), accent);
        CreatePiece(root, PrimitiveType.Cube, "Основание награды", new Vector3(0f, 0.82f, -4.2f),
            new Vector3(0.9f, 1.25f, 0.9f), accent);
        CreatePiece(root, PrimitiveType.Sphere, "Светящаяся награда", new Vector3(0f, 1.82f, -4.2f),
            Vector3.one * 0.85f, accent);
    }

    private static void AddRoomLights(Transform root, bool isFinal, float halfWidth, float halfDepth)
    {
        Color lightColor = isFinal
            ? new Color(1f, 0.76f, 0.42f)
            : new Color(0.59f, 0.86f, 1f);
        Vector3[] positions =
        {
            new Vector3(-halfWidth * 0.35f, 8.8f, -halfDepth * 0.35f),
            new Vector3(halfWidth * 0.35f, 8.8f, halfDepth * 0.35f)
        };

        foreach (Vector3 position in positions)
        {
            var lightObject = new GameObject("Светильник зала");
            lightObject.transform.SetParent(root, false);
            lightObject.transform.localPosition = position;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = lightColor;
            light.intensity = isFinal ? 2.1f : 1.7f;
            light.range = 13f;
            light.shadows = LightShadows.None;
        }
    }

    private static void CreateSign(Transform root, string text, Vector3 position, Vector3 size,
        Material material, Quaternion rotation)
    {
        GameObject board = CreatePiece(root, PrimitiveType.Cube, "Вывеска зала: " + text,
            position, size, material, rotation);
        var textObject = new GameObject("Надпись: " + text);
        textObject.transform.SetParent(root, false);
        textObject.transform.localPosition = board.transform.localPosition +
            board.transform.localRotation * Vector3.forward * 0.1f;
        textObject.transform.localRotation = board.transform.localRotation;
        TextMesh label = textObject.AddComponent<TextMesh>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = text;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 70;
        label.fontStyle = FontStyle.Bold;
        label.characterSize = 0.052f;
        label.color = new Color(1f, 0.97f, 0.88f);
    }

    private static GameObject CreatePiece(Transform root, PrimitiveType type, string name, Vector3 position,
        Vector3 scale, Material material, Quaternion? rotation = null, bool keepCollider = false)
    {
        GameObject piece = GameObject.CreatePrimitive(type);
        piece.name = name;
        piece.transform.SetParent(root, false);
        piece.transform.localPosition = position;
        piece.transform.localRotation = rotation ?? Quaternion.identity;
        piece.transform.localScale = scale;

        Renderer renderer = piece.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        Collider collider = piece.GetComponent<Collider>();
        if (collider != null && !keepCollider) Object.Destroy(collider);
        return piece;
    }

    private static Material MakeMaterial(string name, Color color, float metallic = 0f, Color emission = default)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        var material = new Material(shader) { name = name, color = color };
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (emission.maxColorComponent > 0f)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
        }
        return material;
    }
}
