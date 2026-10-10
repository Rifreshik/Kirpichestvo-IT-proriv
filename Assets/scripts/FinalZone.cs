using UnityEngine;

[DisallowMultipleComponent]
public class FinalZone : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string completionMessage = "ПОЗДРАВЛЯЕМ!\nКомпьютер собран. Космический маршрут пройден.";

    private bool completed;
    private GUIStyle completionStyle;

    private void OnTriggerEnter(Collider other)
    {
        if (completed || (!other.CompareTag(playerTag) && !other.transform.root.CompareTag(playerTag)))
            return;

        completed = true;
        GameObject player = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.transform.root.gameObject;

        Movementtag movement = player.GetComponent<Movementtag>();
        if (movement != null) movement.enabled = false;
        WeaponController weapon = player.GetComponent<WeaponController>();
        if (weapon != null) weapon.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnGUI()
    {
        if (!completed) return;

        const float width = 560f;
        const float height = 150f;
        Rect panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        GUI.Box(panel, string.Empty);

        if (completionStyle == null)
        {
            completionStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                wordWrap = true
            };
        }
        GUI.Label(new Rect(panel.x + 20f, panel.y + 20f, panel.width - 40f, panel.height - 40f), completionMessage, completionStyle);
    }
}
