using UnityEngine;

public enum ComputerPartKind
{
    Case,
    Motherboard,
    Processor,
    Memory,
    GraphicsCard,
    Cooler
}

/// <summary>Runtime interaction marker for a loose computer component.</summary>
public class ComputerPartPickup : MonoBehaviour
{
    public ComputerPartKind kind;
    public string displayName;
    public ComputerAssemblyStation station;

    private Renderer[] cachedRenderers;
    private GUIContent cachedLabel;

    private void Start()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>();
        cachedLabel = new GUIContent(displayName);
    }

    private void OnGUI()
    {
        Camera camera = station != null ? station.InteractionCamera : Camera.main;
        if (camera == null || !gameObject.activeInHierarchy) return;
        if (station != null && !station.IsPlayerOnWorkshopPlatform()) return;
        if (cachedRenderers == null)
        {
            cachedRenderers = GetComponentsInChildren<Renderer>();
            cachedLabel = new GUIContent(displayName);
        }
        if (cachedRenderers.Length == 0) return;

        Bounds bounds = cachedRenderers[0].bounds;
        for (int i = 1; i < cachedRenderers.Length; i++) bounds.Encapsulate(cachedRenderers[i].bounds);
        Vector3 screen = camera.WorldToScreenPoint(new Vector3(bounds.center.x, bounds.max.y + 0.18f, bounds.center.z));
        if (screen.z <= 0f) return;
        GUI.Box(new Rect(screen.x - 85f, Screen.height - screen.y - 15f, 170f, 28f), cachedLabel);
    }
}
