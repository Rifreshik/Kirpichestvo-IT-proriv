using UnityEngine;

public static class LunarExhibitionBuilder
{
    private const string RootName = "Лунная экспозиция (3D)";

    public static void Build(GameObject platform)
    {
        if (platform == null || GameObject.Find(RootName) != null) return;

        Collider platformCollider = platform.GetComponent<Collider>();
        float surfaceY = platformCollider != null
            ? platformCollider.bounds.max.y + 0.04f
            : platform.transform.position.y + 0.1f;
        var rootObject = new GameObject(RootName);
        rootObject.transform.position = new Vector3(
            platform.transform.position.x,
            surfaceY,
            platform.transform.position.z);
        Transform root = rootObject.transform;

        Material moon = MakeMaterial("Лунный реголит", new Color(0.26f, 0.29f, 0.34f), 0.05f);
        Material moonLight = MakeMaterial("Светлый лунный грунт", new Color(0.46f, 0.49f, 0.53f), 0.08f);
        Material crater = MakeMaterial("Тени кратеров", new Color(0.12f, 0.15f, 0.19f));
        Material metal = MakeMaterial("Светлый титан", new Color(0.76f, 0.81f, 0.86f), 0.72f);
        Material darkMetal = MakeMaterial("Графитовый металл", new Color(0.09f, 0.13f, 0.18f), 0.68f);
        Material gold = MakeMaterial("Золотистая изоляция", new Color(0.91f, 0.56f, 0.16f), 0.52f);
        Material glass = MakeMaterial("Солнечные панели", new Color(0.11f, 0.46f, 0.72f), 0.42f, new Color(0.04f, 0.16f, 0.3f));
        Material rail = MakeMaterial("Ограждение галереи", new Color(0.56f, 0.68f, 0.77f), 0.82f);
        Material night = MakeMaterial("Ночное небо экспозиции", new Color(0.015f, 0.025f, 0.055f), 0.12f);
        Material star = MakeMaterial("Звёздный свет", new Color(0.73f, 0.88f, 1f), 0f, new Color(0.62f, 0.82f, 1f));
        Material visitor = MakeMaterial("Силуэты посетителей", new Color(0.18f, 0.24f, 0.32f), 0.1f);

        CreatePiece(root, PrimitiveType.Cube, "Площадка из лунного грунта", Vector3.zero,
            new Vector3(27.8f, 0.1f, 24.2f), moon);
        CreatePiece(root, PrimitiveType.Cube, "Подиум экспозиции", new Vector3(0f, 0.11f, 0f),
            new Vector3(24.8f, 0.12f, 21.5f), moonLight);

        CreateCrater(root, new Vector3(-10.5f, 0.19f, -5.2f), new Vector2(2.4f, 1.5f), crater, moonLight);
        CreateCrater(root, new Vector3(10.4f, 0.19f, -5.7f), new Vector2(2.2f, 1.4f), crater, moonLight);
        CreateCrater(root, new Vector3(-10.7f, 0.19f, 3.8f), new Vector2(1.7f, 1.2f), crater, moonLight);
        CreateCrater(root, new Vector3(10.5f, 0.19f, 3.6f), new Vector2(1.9f, 1.35f), crater, moonLight);
        CreateRock(root, new Vector3(-12f, 0.28f, 0.5f), new Vector3(1.1f, 0.7f, 0.85f), moonLight);
        CreateRock(root, new Vector3(12.1f, 0.32f, -1.9f), new Vector3(1.3f, 0.9f, 1.1f), moonLight);
        CreateRock(root, new Vector3(8.9f, 0.25f, -10.1f), new Vector3(0.9f, 0.6f, 0.7f), moonLight);

        BuildLandingModule(root, metal, darkMetal, gold, glass);
        BuildRover(root, metal, darkMetal, gold, glass);
        BuildObservationGallery(root, rail, night, star, visitor);
        StylePlanetTargets(root);
        AddDisplayLighting(root);
    }

    private static void BuildLandingModule(Transform root, Material metal, Material darkMetal, Material gold, Material glass)
    {
        Vector3 center = new Vector3(-5.3f, 0f, -7.2f);
        CreatePiece(root, PrimitiveType.Cylinder, "Основание лунного модуля", center + new Vector3(0f, 0.2f, 0f),
            new Vector3(4.6f, 0.14f, 3.4f), darkMetal);
        CreatePiece(root, PrimitiveType.Cube, "Посадочная ступень", center + new Vector3(0f, 1.25f, 0f),
            new Vector3(1.55f, 1.35f, 1.55f), gold);
        CreatePiece(root, PrimitiveType.Cube, "Верхний отсек модуля", center + new Vector3(0f, 2.35f, 0f),
            new Vector3(1.15f, 0.82f, 1.15f), metal);
        CreatePiece(root, PrimitiveType.Sphere, "Купол посадочного модуля", center + new Vector3(0f, 2.86f, 0f),
            new Vector3(0.92f, 0.63f, 0.92f), glass);

        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f + 45f;
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            Vector3 legPosition = center + direction * 1.36f + new Vector3(0f, 0.68f, 0f);
            CreatePiece(root, PrimitiveType.Cube, "Опора посадочного модуля", legPosition,
                new Vector3(0.16f, 1.25f, 0.16f), metal,
                Quaternion.Euler(0f, angle, -18f));
            CreatePiece(root, PrimitiveType.Cube, "Опорная пятка модуля", center + direction * 1.56f + new Vector3(0f, 0.1f, 0f),
                new Vector3(0.7f, 0.14f, 0.58f), darkMetal,
                Quaternion.Euler(0f, angle, 0f));
        }

        CreatePiece(root, PrimitiveType.Cylinder, "Антенна лунного модуля", center + new Vector3(0.9f, 2.9f, 0.65f),
            new Vector3(0.08f, 0.95f, 0.08f), metal);
        CreatePiece(root, PrimitiveType.Sphere, "Антенна модуля", center + new Vector3(0.9f, 3.38f, 0.65f),
            new Vector3(0.36f, 0.22f, 0.36f), metal);
        CreatePiece(root, PrimitiveType.Cube, "Левая солнечная панель модуля", center + new Vector3(-2.2f, 1.7f, 0f),
            new Vector3(2.7f, 0.08f, 1.4f), glass);
        CreatePiece(root, PrimitiveType.Cube, "Правая солнечная панель модуля", center + new Vector3(2.2f, 1.7f, 0f),
            new Vector3(2.7f, 0.08f, 1.4f), glass);
    }

    private static void BuildRover(Transform root, Material metal, Material darkMetal, Material gold, Material glass)
    {
        Vector3 center = new Vector3(5.6f, 0f, -7.3f);
        CreatePiece(root, PrimitiveType.Cube, "Корпус лунохода", center + new Vector3(0f, 1.05f, 0f),
            new Vector3(2.1f, 0.72f, 1.65f), metal);
        CreatePiece(root, PrimitiveType.Cube, "Крышка лунохода", center + new Vector3(0f, 1.48f, 0f),
            new Vector3(1.72f, 0.12f, 1.35f), gold);
        CreatePiece(root, PrimitiveType.Cube, "Панель лунохода", center + new Vector3(0f, 1.78f, 0f),
            new Vector3(2.6f, 0.08f, 1.75f), glass);
        for (int side = -1; side <= 1; side++)
        {
            for (int xSide = -1; xSide <= 1; xSide += 2)
            {
                Vector3 wheelPosition = center + new Vector3(xSide * 1.12f, 0.55f, side * 0.58f);
                CreatePiece(root, PrimitiveType.Cylinder, "Колесо лунохода", wheelPosition,
                    new Vector3(0.75f, 0.18f, 0.75f), darkMetal,
                    Quaternion.Euler(90f, 0f, 0f));
                CreatePiece(root, PrimitiveType.Cylinder, "Ступица лунохода", wheelPosition,
                    new Vector3(0.38f, 0.2f, 0.38f), gold,
                    Quaternion.Euler(90f, 0f, 0f));
            }
        }

        CreatePiece(root, PrimitiveType.Cylinder, "Мачта лунохода", center + new Vector3(0f, 2.4f, 0.48f),
            new Vector3(0.09f, 0.76f, 0.09f), metal);
        CreatePiece(root, PrimitiveType.Sphere, "Камера лунохода", center + new Vector3(0f, 2.82f, 0.48f),
            new Vector3(0.42f, 0.35f, 0.4f), gold);
        CreatePiece(root, PrimitiveType.Cube, "Маркировка лунохода", center + new Vector3(0f, 1.14f, 0.84f),
            new Vector3(0.58f, 0.32f, 0.08f), darkMetal);
    }

    private static void BuildObservationGallery(Transform root, Material rail, Material night, Material star, Material visitor)
    {
        const int wallSegments = 48;
        const float roomRadius = 12f;
        const float roomHeight = 12f;
        float segmentWidth = 2f * roomRadius * Mathf.Sin(Mathf.PI / wallSegments) + 0.12f;

        for (int i = 0; i < wallSegments; i++)
        {
            float angle = i * Mathf.PI * 2f / wallSegments;
            Vector3 radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 wallPosition = radial * roomRadius + Vector3.up * (roomHeight * 0.5f);
            Quaternion wallRotation = Quaternion.Euler(0f, 90f - angle * Mathf.Rad2Deg, 0f);
            CreatePiece(root, PrimitiveType.Cube, "Замкнутая стена зала", wallPosition,
                new Vector3(segmentWidth, roomHeight, 0.85f), night, wallRotation, true);

            Vector3 innerTrimPosition = radial * (roomRadius - 0.42f);
            CreatePiece(root, PrimitiveType.Cube, "Световая полоса павильона",
                innerTrimPosition + Vector3.up * 2.25f,
                new Vector3(segmentWidth, 0.08f, 0.08f), rail, wallRotation);
            CreatePiece(root, PrimitiveType.Cube, "Световая полоса павильона",
                innerTrimPosition + Vector3.up * 9.15f,
                new Vector3(segmentWidth, 0.08f, 0.08f), rail, wallRotation);

            if (i % 4 == 0)
            {
                Vector3 supportPosition = radial * (roomRadius - 0.43f) + Vector3.up * 6f;
                CreatePiece(root, PrimitiveType.Cube, "Ребро павильона", supportPosition,
                    new Vector3(0.14f, 11.4f, 0.12f), rail, wallRotation);
            }

            for (int band = 0; band < 3; band++)
            {
                float y = 2.8f + band * 2.25f;
                Vector3 starPosition = radial * (roomRadius - 0.46f) + Vector3.up * y;
                starPosition += Vector3.up * ((i % 3) * 0.13f);
                CreatePiece(root, PrimitiveType.Sphere, "Звезда на стене зала", starPosition,
                    Vector3.one * (i % 5 == 0 ? 0.12f : 0.075f), star);
            }
        }

        CreatePiece(root, PrimitiveType.Cube, "Потолок лунного павильона", new Vector3(0f, roomHeight, 0f),
            new Vector3(25f, 0.7f, 25f), night, null, true);
        for (int i = 0; i < 48; i++)
        {
            float angle = i * 2.3999632f;
            float radius = 1.5f + (i % 7) * 1.35f;
            Vector3 starPosition = new Vector3(Mathf.Cos(angle) * radius, roomHeight - 0.42f, Mathf.Sin(angle) * radius);
            CreatePiece(root, PrimitiveType.Sphere, "Звезда на потолке", starPosition,
                Vector3.one * (i % 6 == 0 ? 0.13f : 0.075f), star);
        }

        BuildGalleryRing(root, rail, visitor);
        GameObject sign = CreatePiece(root, PrimitiveType.Cube, "Вывеска лунной экспозиции",
            new Vector3(0f, 8.15f, roomRadius - 0.42f), new Vector3(9f, 1.35f, 0.18f), rail,
            Quaternion.Euler(0f, 180f, 0f));
        var textObject = new GameObject("Текст: Лунная экспедиция");
        textObject.transform.SetParent(root, false);
        textObject.transform.localPosition = sign.transform.localPosition +
            sign.transform.localRotation * Vector3.forward * 0.1f;
        textObject.transform.localRotation = sign.transform.localRotation;
        var label = textObject.AddComponent<TextMesh>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = "ЛУННАЯ ЭКСПЕДИЦИЯ";
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.fontSize = 80;
        label.characterSize = 0.055f;
        label.color = new Color(0.58f, 0.88f, 1f);
    }

    private static void BuildGalleryRing(Transform root, Material rail, Material visitor)
    {
        const int sections = 24;
        const float radius = 10.45f;
        const float railRadius = 9.4f;
        float sectionLength = 2f * Mathf.PI * radius / sections + 0.18f;

        for (int i = 0; i < sections; i++)
        {
            float angle = i * Mathf.PI * 2f / sections;
            Vector3 radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Quaternion tangent = Quaternion.Euler(0f, 90f - angle * Mathf.Rad2Deg, 0f);
            Vector3 deckPosition = radial * radius + Vector3.up * 4.15f;
            CreatePiece(root, PrimitiveType.Cube, "Секция обзорной галереи", deckPosition,
                new Vector3(sectionLength, 0.18f, 1.65f), rail, tangent);

            Vector3 innerRail = radial * railRadius;
            CreatePiece(root, PrimitiveType.Cube, "Верх ограждения галереи", innerRail + Vector3.up * 5.35f,
                new Vector3(sectionLength, 0.1f, 0.1f), rail, tangent);
            CreatePiece(root, PrimitiveType.Cube, "Низ ограждения галереи", innerRail + Vector3.up * 4.65f,
                new Vector3(sectionLength, 0.08f, 0.08f), rail, tangent);

            Vector3 visitorPosition = radial * (radius + 0.25f) + Vector3.up * 4.92f;
            Quaternion facingCenter = Quaternion.LookRotation(-radial, Vector3.up);
            CreatePiece(root, PrimitiveType.Capsule, "Посетитель обзорной галереи", visitorPosition,
                new Vector3(0.42f, 0.76f, 0.34f), visitor, facingCenter);
            CreatePiece(root, PrimitiveType.Sphere, "Голова посетителя галереи", visitorPosition + Vector3.up * 0.59f,
                new Vector3(0.32f, 0.34f, 0.32f), visitor);

            Vector3 postPosition = radial * railRadius + Vector3.up * 5f;
            CreatePiece(root, PrimitiveType.Cube, "Стойка ограждения галереи", postPosition,
                new Vector3(0.09f, 0.82f, 0.09f), rail);
        }
    }
    private static void StylePlanetTargets(Transform root)
    {
        Material[] planetMaterials =
        {
            MakeMaterial("Планета-мишень: Марс", new Color(0.92f, 0.25f, 0.16f), 0.08f, new Color(0.28f, 0.04f, 0.02f)),
            MakeMaterial("Планета-мишень: Лёд", new Color(0.26f, 0.7f, 0.92f), 0.18f, new Color(0.04f, 0.18f, 0.32f)),
            MakeMaterial("Планета-мишень: Газовый гигант", new Color(0.8f, 0.58f, 0.24f), 0.12f, new Color(0.2f, 0.08f, 0.01f)),
            MakeMaterial("Планета-мишень: Фиолетовая", new Color(0.56f, 0.36f, 0.88f), 0.1f, new Color(0.12f, 0.04f, 0.26f))
        };
        Material orbitMaterial = MakeMaterial("Свет орбит планет", new Color(0.4f, 0.88f, 1f), 0.1f, new Color(0.12f, 0.44f, 0.62f));
        DisappearOnShot[] targets = Object.FindObjectsOfType<DisappearOnShot>(true);
        Vector3[] positions =
        {
            new Vector3(-9f, 2.3f, -1.2f), new Vector3(-6.1f, 3.2f, 4.8f),
            new Vector3(-3f, 2.55f, 1.2f), new Vector3(0f, 3.35f, 6.4f),
            new Vector3(3f, 2.35f, 1.2f), new Vector3(6f, 3.1f, 5f),
            new Vector3(9f, 2.55f, -0.8f), new Vector3(0f, 2.45f, -4.5f)
        };

        for (int i = 0; i < targets.Length; i++)
        {
            DisappearOnShot target = targets[i];
            if (target.gameObject.scene != root.gameObject.scene) continue;

            target.gameObject.name = "Планета-мишень " + (i + 1);
            target.transform.position = root.position + positions[i % positions.Length];
            target.transform.localScale = Vector3.one * (i % 3 == 0 ? 1.65f : 1.35f);

            Rigidbody body = target.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.isKinematic = true;
                body.useGravity = false;
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>())
                renderer.material = planetMaterials[i % planetMaterials.Length];

            AddOrbit(target.transform, orbitMaterial, 0.82f + (i % 2) * 0.12f);
        }
    }

    private static void AddOrbit(Transform target, Material material, float radius)
    {
        if (target.Find("Орбита планеты") != null) return;

        var orbitObject = new GameObject("Орбита планеты");
        orbitObject.transform.SetParent(target, false);
        orbitObject.transform.localRotation = Quaternion.Euler(65f, 0f, 0f);
        var line = orbitObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 49;
        line.widthMultiplier = 0.035f;
        line.sharedMaterial = material;
        line.startColor = Color.white;
        line.endColor = Color.white;
        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / (line.positionCount - 1);
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
        }
    }

    private static void CreateCrater(Transform root, Vector3 position, Vector2 size, Material shadow, Material rim)
    {
        CreatePiece(root, PrimitiveType.Cylinder, "Кратер — тень", position,
            new Vector3(size.x, 0.035f, size.y), shadow);
        CreatePiece(root, PrimitiveType.Cylinder, "Кратер — вал", position + Vector3.up * 0.03f,
            new Vector3(size.x * 0.72f, 0.03f, size.y * 0.72f), rim);
    }

    private static void CreateRock(Transform root, Vector3 position, Vector3 scale, Material material)
    {
        GameObject rock = CreatePiece(root, PrimitiveType.Sphere, "Лунный камень", position, scale, material,
            Quaternion.Euler(8f, position.x * 5f, 14f));
        rock.transform.localScale = scale;
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

    private static void AddDisplayLighting(Transform root)
    {
        CreateSpotlight(root, new Vector3(-4f, 9f, -1f), new Color(0.55f, 0.78f, 1f), 2.6f, 18f);
        CreateSpotlight(root, new Vector3(5f, 8f, -4f), new Color(1f, 0.7f, 0.38f), 1.8f, 15f);
    }

    private static void CreateSpotlight(Transform root, Vector3 position, Color color, float intensity, float range)
    {
        var lightObject = new GameObject("Прожектор экспозиции");
        lightObject.transform.SetParent(root, false);
        lightObject.transform.localPosition = position;
        lightObject.transform.localRotation = Quaternion.Euler(62f, 0f, 0f);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Spot;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.spotAngle = 62f;
        light.shadows = LightShadows.None;
    }
}
