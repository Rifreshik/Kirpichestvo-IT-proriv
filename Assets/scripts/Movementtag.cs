using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class Movementtag : MonoBehaviour
{
    [Min(0f)] public float moveSpeed = 5f;
    [Min(0f)] public float sprintSpeed = 8f;
    [Min(0f)] public float mouseSensitivity = 3f;
    public Transform cameraTransform;

    private const float CameraHeight = 0.65f;
    private const float ZoneViewDistance = 150f;

    private Rigidbody body;
    private Camera playerCamera;
    private Vector2 moveInput;
    private float currentSpeed;
    private float cameraRotationX;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        if (body == null)
        {
            Debug.LogError("Movementtag требует Rigidbody на объекте игрока.", this);
            enabled = false;
            return;
        }

        if (cameraTransform == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null) playerCamera = Camera.main;
            if (playerCamera == null) playerCamera = FindObjectOfType<Camera>();
            if (playerCamera != null) cameraTransform = playerCamera.transform;
        }
        else
        {
            playerCamera = cameraTransform.GetComponent<Camera>();
        }

        if (cameraTransform == null || playerCamera == null)
        {
            Debug.LogError("Movementtag не нашёл камеру игрока. Назначьте Camera в поле Camera Transform.", this);
            enabled = false;
            return;
        }

        if (cameraTransform.parent != transform)
        {
            cameraTransform.SetParent(transform, false);
            cameraTransform.localPosition = new Vector3(0f, CameraHeight, 0f);
            cameraTransform.localRotation = Quaternion.identity;
        }

        playerCamera.gameObject.tag = "MainCamera";
        playerCamera.farClipPlane = ZoneViewDistance;
        body.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        moveInput = Vector2.ClampMagnitude(new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")), 1f);
        currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        RotateCamera();
    }

    private void FixedUpdate()
    {
        if (body == null) return;

        Vector3 movement = transform.right * moveInput.x + transform.forward * moveInput.y;
        body.MovePosition(body.position + movement * currentSpeed * Time.fixedDeltaTime);
    }

    private void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        cameraRotationX = Mathf.Clamp(cameraRotationX - mouseY, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(cameraRotationX, 0f, 0f);
    }

    public void SetFacing(Quaternion rotation)
    {
        transform.rotation = rotation;
        cameraRotationX = 0f;
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.identity;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
