using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [SerializeField]public float mouseSensitivity = 100f;
    public Transform playerBody;  // tham chiếu đến object Player

    private float xRotation = 0f;
    void Start()
    {
        // Đăng ký sự kiện game state thay đổi
        GameManager.Instance.OnGameStateChanged += OnGamePlaying;
        GameManager.Instance.OnGameStateChanged += OnGameOver;
    }

    void Update()
    {
        // if not playing game => return 
        if (!GameManager.Instance.IsGamePlaying()) return;
        
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

    private void OnGamePlaying(GameState newGameState)
    {
        if (newGameState == GameState.GamePlaying)
        {
            // Ẩn và khóa chuột vào giữa màn hình
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    private void OnGameOver(GameState newGameState)
    {
        if (newGameState == GameState.GameOver)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
