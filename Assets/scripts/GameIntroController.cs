using UnityEngine;

[DisallowMultipleComponent]
public class GameIntroController : MonoBehaviour
{
    private enum FlowState
    {
        AskAboutGoldenRing,
        GoldenRingStory,
        Playing
    }

    private const string TargetPlatformName = "Cube (6)";
    private const string AssemblyPlatformName = "Cube (9)";

    private FlowState state = FlowState.AskAboutGoldenRing;
    private Movementtag movement;
    private WeaponController weapon;
    private ShotTargetGoal targetGoal;
    private bool assemblyStageStarted;
    private GUIStyle titleStyle;
    private GUIStyle kickerStyle;
    private GUIStyle bodyStyle;
    private GUIStyle buttonStyle;
    private GUIStyle secondaryButtonStyle;
    private Texture2D panelTexture;
    private Texture2D shadowTexture;
    private Texture2D accentTexture;
    private Texture2D primaryButtonTexture;
    private Texture2D primaryHoverTexture;
    private Texture2D secondaryButtonTexture;

    public bool GameplayActive => state == FlowState.Playing;

    private void Start()
    {
        movement = GetComponent<Movementtag>();
        weapon = GetComponent<WeaponController>();
        targetGoal = GetComponent<ShotTargetGoal>();

        if (movement != null) movement.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }

    private void OnDestroy()
    {

        DestroyRuntimeTextures();
    }

    private void OnGUI()
    {
        EnsureStyles();

        if (state == FlowState.Playing)
        {
            if (!assemblyStageStarted)
                DrawMissionStatus();
            return;
        }

        if (state == FlowState.AskAboutGoldenRing)
        {
            DrawPanel(
                "ПУТЕШЕСТВИЕ НАЧИНАЕТСЯ",
                "Хочешь узнать о Золотом кольце России перед стартом?",
                "Да, расскажи",
                ShowGoldenRingStory,
                "Сразу начать игру",
                BeginAdventure);
            return;
        }

        if (state == FlowState.GoldenRingStory)
        {
            DrawPanel(
                "ЗОЛОТОЕ КОЛЬЦО",
                "Это маршрут по древним городам России. Каждая остановка открывает новую историю. В нашем приключении первая игровая остановка — лунная экспозиция.",
                "Начать путешествие",
                BeginAdventure);
            return;
        }

    }

    private void ShowGoldenRingStory()
    {
        state = FlowState.GoldenRingStory;
    }

    private void BeginAdventure()
    {
        GameObject platform = GameObject.Find(TargetPlatformName);
        if (!PlacePlayerOnPlatform(platform)) return;

        if (weapon != null) weapon.GiveWeapon();
        state = FlowState.Playing;
        SetCursorLocked(true);
    }


    public bool CollectPlanetFragment()
    {
        if (state != FlowState.Playing) return false;

        GameObject platform = GameObject.Find(AssemblyPlatformName);
        if (!PlacePlayerOnPlatform(platform)) return false;

        assemblyStageStarted = true;
        state = FlowState.Playing;
        if (movement != null) movement.enabled = true;
        SetCursorLocked(true);
        return true;
    }


    private bool PlacePlayerOnPlatform(GameObject platform)
    {
        if (platform == null)
        {
            Debug.LogError("GameIntroController: не найдена платформа для перехода.", this);
            return false;
        }

        Vector3 position = platform.transform.position;
        Collider platformCollider = platform.GetComponent<Collider>();
        position.y = platformCollider != null
            ? platformCollider.bounds.max.y + 1.15f
            : position.y + 1.15f;
        Quaternion facing = platform.transform.rotation;

        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.position = position;
            body.rotation = facing;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        else
        {
            transform.SetPositionAndRotation(position, facing);
        }

        if (movement != null)
        {
            movement.SetFacing(facing);
            movement.enabled = true;
        }

        return true;
    }

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void DrawMissionStatus()
    {
        float scale = GetUiScale();
        float width = Mathf.Min(410f * scale, Screen.width - 32f);
        float height = 94f * scale;
        Rect panel = new Rect(20f, 20f, width, height);
        DrawCard(panel);

        GUI.Label(new Rect(panel.x + 20f * scale, panel.y + 10f * scale, panel.width - 40f * scale, 26f * scale),
            "ЛУННАЯ ЭКСПОЗИЦИЯ", kickerStyle);
        int remaining = targetGoal != null ? targetGoal.RemainingTargetCount : 0;
        string objective = remaining > 0
            ? "Осталось планет: " + remaining
            : "Подойди к осколку и нажми E";
        GUI.Label(new Rect(panel.x + 20f * scale, panel.y + 42f * scale, panel.width - 40f * scale, 38f * scale),
            objective, bodyStyle);
    }

    private void DrawPanel(string title, string body, string primaryLabel, System.Action primaryAction,
        string secondaryLabel = null, System.Action secondaryAction = null)
    {
        float scale = GetUiScale();
        float width = Mathf.Min(820f * scale, Screen.width - 36f);
        float desiredHeight = (secondaryAction != null ? 460f : 420f) * scale;
        float height = Mathf.Min(desiredHeight, Screen.height - 36f);
        Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), shadowTexture);
        DrawCard(panel);

        float padding = 34f * scale;
        float contentWidth = panel.width - padding * 2f;
        GUI.Label(new Rect(panel.x + padding, panel.y + 28f * scale, contentWidth, 24f * scale),
            "ЗОЛОТОЕ КОЛЬЦО  /  КОСМИЧЕСКИЙ МАРШРУТ", kickerStyle);
        GUI.Label(new Rect(panel.x + padding, panel.y + 62f * scale, contentWidth, 64f * scale), title, titleStyle);

        float buttonHeight = 56f * scale;
        float buttonGap = 12f * scale;
        float buttonsBottom = panel.yMax - 28f * scale;
        float buttonsHeight = secondaryAction != null
            ? 2f * buttonHeight + buttonGap
            : buttonHeight;

        if (secondaryAction != null)
        {
            Rect secondary = new Rect(panel.x + padding, buttonsBottom - buttonHeight, contentWidth, buttonHeight);
            Rect primary = new Rect(panel.x + padding, secondary.y - buttonGap - buttonHeight, contentWidth, buttonHeight);
            if (GUI.Button(primary, primaryLabel, buttonStyle)) primaryAction?.Invoke();
            if (GUI.Button(secondary, secondaryLabel, secondaryButtonStyle)) secondaryAction.Invoke();
        }
        else
        {
            Rect primary = new Rect(panel.x + padding, buttonsBottom - buttonHeight, contentWidth, buttonHeight);
            if (GUI.Button(primary, primaryLabel, buttonStyle)) primaryAction?.Invoke();
        }

        float bodyTop = panel.y + 142f * scale;
        float bodyBottom = buttonsBottom - buttonsHeight - 18f * scale;
        float bodyHeight = Mathf.Max(60f * scale, bodyBottom - bodyTop);
        GUI.Label(new Rect(panel.x + padding, bodyTop, contentWidth, bodyHeight), body, bodyStyle);
    }

    private void EnsureStyles()
    {
        if (panelTexture == null)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            panelTexture = MakeTexture(new Color(0.035f, 0.065f, 0.105f, 0.98f));
            shadowTexture = MakeTexture(new Color(0.005f, 0.012f, 0.025f, 0.78f));
            accentTexture = MakeTexture(new Color(0.20f, 0.76f, 0.95f, 1f));
            primaryButtonTexture = MakeTexture(new Color(0.04f, 0.48f, 0.68f, 1f));
            primaryHoverTexture = MakeTexture(new Color(0.06f, 0.62f, 0.82f, 1f));
            secondaryButtonTexture = MakeTexture(new Color(0.10f, 0.16f, 0.23f, 1f));

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                font = font,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(0.96f, 0.98f, 1f) }
            };
            kickerStyle = new GUIStyle(GUI.skin.label)
            {
                font = font,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.36f, 0.84f, 1f) }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                font = font,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                padding = new RectOffset(0, 0, 2, 0),
                normal = { textColor = new Color(0.86f, 0.91f, 0.97f) }
            };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                font = font,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { background = primaryButtonTexture, textColor = Color.white },
                hover = { background = primaryHoverTexture, textColor = Color.white },
                active = { background = primaryHoverTexture, textColor = Color.white },
                focused = { background = primaryHoverTexture, textColor = Color.white },
                border = new RectOffset(8, 8, 8, 8),
                padding = new RectOffset(16, 16, 8, 8)
            };
            secondaryButtonStyle = new GUIStyle(buttonStyle)
            {
                normal = { background = secondaryButtonTexture, textColor = new Color(0.86f, 0.92f, 0.98f) },
                hover = { background = primaryButtonTexture, textColor = Color.white },
                active = { background = primaryHoverTexture, textColor = Color.white },
                focused = { background = primaryButtonTexture, textColor = Color.white }
            };
        }

        float scale = GetUiScale();
        titleStyle.fontSize = Mathf.RoundToInt(30f * scale);
        kickerStyle.fontSize = Mathf.RoundToInt(13f * scale);
        bodyStyle.fontSize = Mathf.RoundToInt(19f * scale);
        buttonStyle.fontSize = Mathf.RoundToInt(18f * scale);
        secondaryButtonStyle.fontSize = Mathf.RoundToInt(18f * scale);
    }

    private void DrawCard(Rect rect)
    {
        GUI.DrawTexture(new Rect(rect.x + 5f, rect.y + 8f, rect.width, rect.height), shadowTexture);
        GUI.DrawTexture(rect, panelTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 4f), accentTexture);
    }

    private static float GetUiScale()
    {
        return Mathf.Clamp(Mathf.Min(Screen.width / 1280f, Screen.height / 720f), 0.72f, 1.15f);
    }

    private Texture2D MakeTexture(Color color)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear
        };
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private void DestroyRuntimeTextures()
    {
        Destroy(panelTexture);
        Destroy(shadowTexture);
        Destroy(accentTexture);
        Destroy(primaryButtonTexture);
        Destroy(primaryHoverTexture);
        Destroy(secondaryButtonTexture);
    }
}
