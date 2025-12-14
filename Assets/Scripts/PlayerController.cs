using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    private Animator animator;

    [SerializeField] private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private bool isUsing = true;
    [SerializeField] private BikeController bikeController;
    [SerializeField] private Transform playerSeat;
    void Start()
    {
        transform.position = GameManager.Instance.GetBroOriginalPos().position;
        controller = GetComponentInChildren<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        playerSeat = bikeController.GetSeat();
        EventManager.Instance.OnBikeSpawn += OnBikeSpawned;
    }

    void Update()
    {
        // if not playing game => return 
        if (!GameManager.Instance.IsGamePlaying()) return;

        if(!isUsing) return;

        ApplyMovemet();
        AnimatorController();
    }

    private void AnimatorController()
    {
        float xVelocity = Vector3.Dot(velocity, transform.right);
        float zVelocity = Vector3.Dot(velocity, transform.forward);

        animator.SetFloat("xVelocity", xVelocity);
        animator.SetFloat("zVelocity", zVelocity);

    }

    public void SetMouseLook(bool isActive)
    {
        mouseLook.enabled = isActive;
    }

    public void SetAnimationRiding(bool isRide)
    {
        animator.SetBool("isRiding", isRide);
    }

    public void EnableCollider()
    {
        controller.enabled = true;
    }

    public void DisableCollider()
    {
        controller.enabled = false;
    }

    private void ApplyMovemet()
    {
        // Kiểm tra có chạm đất không
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // giữ nhân vật dính mặt đất
        }

        // Nhận input di chuyển
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        velocity.x = move.x;
        velocity.z = move.z;


        // Nhảy
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Áp dụng trọng lực
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    public void SetUsingController(bool value)
    {
        isUsing = value;
    }
    
    public void EnteredBike() // đã lên xe
    {
        controller.enabled = false;
        SetMouseLook(false);
        SetUsingController(false);
        SetAnimationRiding(true);
        transform.SetParent(playerSeat);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
    public void ExittedBike()
    {
        SetMouseLook(true);
        SetAnimationRiding(false);
        SetUsingController(true);
        transform.SetParent(null);
        transform.position = bikeController.transform.position;
        transform.rotation = Quaternion.LookRotation(transform.forward);
        controller.enabled = true;
    }
    public void OnBikeSpawned(GameObject bike)
    {
        var controller = bike.GetComponentInChildren<BikeController>();
        if(controller != null)
        {
            bikeController = controller;
            playerSeat = bikeController.GetSeat();
        }else
        {
            Debug.Log("Bike controller is null");
        }
    }
    public void ResetBroState()
    {
        EnableCollider();
        SetMouseLook(true);
        SetAnimationRiding(false);
        SetUsingController(true);
        transform.SetParent(null);
    }
}
