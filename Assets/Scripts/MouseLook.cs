using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [SerializeField]public float mouseSensitivity = 100f;
    public Transform playerBody;  // tham chiếu đến object Player

    private float xRotation = 0f;

    void Start()
    {
        // Ẩn và khóa chuột vào giữa màn hình
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Lấy input chuột
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Xoay dọc (camera) - giới hạn từ -90 đến +90
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -60f, 70f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Xoay ngang (toàn bộ Player)
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
