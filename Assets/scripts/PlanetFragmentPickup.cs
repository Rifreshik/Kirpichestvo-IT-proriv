using UnityEngine;

[DisallowMultipleComponent]
public class PlanetFragmentPickup : MonoBehaviour
{
    private GameIntroController intro;
    private bool isNearby;
    private GUIStyle promptStyle;
    private Texture2D promptBackground;

    private void OnTriggerEnter(Collider other)
    {
        GameIntroController controller = other.GetComponentInParent<GameIntroController>();
        if (controller == null) return;

        intro = controller;
        isNearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        GameIntroController controller = other.GetComponentInParent<GameIntroController>();
        if (controller == null || controller != intro) return;

        isNearby = false;
        intro = null;
    }

    private void Update()
    {
        if (!isNearby || intro == null || !intro.GameplayActive) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        if (intro.CollectPlanetFragment())
            Destroy(gameObject);
    }

    private void OnGUI()
    {
        if (!isNearby || intro == null || !intro.GameplayActive) return;
        EnsurePromptStyle();

        float width = Mathf.Min(420f, Screen.width - 32f);
        const float height = 58f;
        Rect panel = new Rect((Screen.width - width) * 0.5f, Screen.height - 112f, width, height);
        GUI.DrawTexture(panel, promptBackground);
        GUI.Label(panel, "E   Подобрать осколок и перейти к сборке ПК", promptStyle);
    }

    private void EnsurePromptStyle()
    {
        if (promptStyle != null) return;

        promptBackground = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        promptBackground.SetPixel(0, 0, new Color(0.025f, 0.055f, 0.09f, 0.92f));
        promptBackground.Apply();

        promptStyle = new GUIStyle(GUI.skin.label)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
    }

    private void OnDestroy()
    {
        if (promptBackground != null)
            Destroy(promptBackground);
    }
}
