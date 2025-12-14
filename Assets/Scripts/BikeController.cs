using Unity.VisualScripting;
using UnityEngine;

public class BikeController : MonoBehaviour
{
    RaycastHit hit;
    private float moveInput, steerInput, rayLenght, curentVelocityOffset;

    [HideInInspector] public Vector3 velocity;
    public float currentSpeed = 0;
    public GameObject Handle;
    public Rigidbody SphereRB, BikeBody;
    public TrailRenderer skinMarks;

    [Header("Audio")]
    public AudioSource engineSound;
    public AudioSource skidSound;

    public float maxSpeed, acceleration, steerStrength, gravity, bikeXTileIncrement, zTiltAngle, handleRotVal, handleRotSpeed, skinWight, minSkinVelocity;

    [Header("Slider")]
    [Range(1, 10)] public float brakingFactory;
    [Range(0, 1)] public float minPitch;
    [Range(1, 5)] public float maxPitch;


    public LayerMask ground;
    [SerializeField] private bool isUsingBike = false;
    [SerializeField] private Transform playerSeat;
    [SerializeField] Transform BikeParent;
    private void Start()
    {
        SphereRB.transform.parent = null;
        BikeBody.transform.parent = null;

        rayLenght = SphereRB.GetComponent<SphereCollider>().radius + 0.2f;


        skinMarks.startWidth = skinWight;
        skinMarks.emitting = false;

    }
    // for collider
    private void OnDisable()
    {
        curentVelocityOffset = 0;
    }
    private void Update()
    {
        // if not playing game => return 
        if (!GameManager.Instance.IsGamePlaying()) return;
        
        if(isUsingBike)
        {
            moveInput = Input.GetAxis("Vertical");
            steerInput = Input.GetAxis("Horizontal");
        }

        // BikeParent.position = SphereRB.transform.position;

        velocity = BikeBody.transform.InverseTransformDirection(BikeBody.velocity);
        curentVelocityOffset = velocity.z / maxSpeed;

        currentSpeed = SphereRB.velocity.magnitude;
    }
    private void FixedUpdate() 
    {
        // if not playing game => return 
        if (!GameManager.Instance.IsGamePlaying()) return;
        if(!isUsingBike) return;
        Movement();

        SkinMarks();

        EngineSound();
    }
    private void LateUpdate()
    {
        BikeParent.position = SphereRB.position;
    }
    private void Movement()
    {
        if (Grounded())
        {
            if (!Input.GetKey(KeyCode.Space))
            {
                Acceleration();
                Rotation();
            }

            Brake();
        }
        else
        {
            // Gravity();
        }
        BikeTilt();
    }

    private void Acceleration()
    {
        // SphereRB.velocity = Vector3.Lerp(SphereRB.velocity, maxSpeed * moveInput * BikeParent.forward, Time.deltaTime * acceleration);
        Vector3 target = maxSpeed * moveInput * BikeParent.forward;
        Vector3 v = SphereRB.velocity;

        v.x = Mathf.Lerp(v.x, target.x, acceleration * Time.fixedDeltaTime);
        v.z = Mathf.Lerp(v.z, target.z, acceleration * Time.fixedDeltaTime);

        SphereRB.velocity = v;

    }

    private void Rotation()
    {
        if(SphereRB.velocity.magnitude > 1)
        {
        BikeParent.Rotate(0, steerInput * steerStrength * Time.fixedDeltaTime, 0, Space.World);
        }
        Handle.transform.localRotation = Quaternion.Slerp(Handle.transform.localRotation, Quaternion.Euler(Handle.transform.localRotation.eulerAngles.x, handleRotVal * steerInput, Handle.transform.localRotation.eulerAngles.z), handleRotSpeed);
    }

    private void Brake()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            SphereRB.velocity *= brakingFactory / 10;
        }
    }

    private void BikeTilt()
    {
        float xRot = (Quaternion.FromToRotation(BikeBody.transform.transform.up, hit.normal) * BikeBody.transform.rotation).eulerAngles.x;
        float zRot = 0;

        if(curentVelocityOffset > 0)
        {
            zRot = -zTiltAngle * steerInput * curentVelocityOffset;
        }
        Quaternion targetRot = Quaternion.Slerp(BikeBody.transform.rotation, Quaternion.Euler(xRot, BikeParent.eulerAngles.y, zRot), bikeXTileIncrement);

        Quaternion newRotation = Quaternion.Euler(targetRot.eulerAngles.x, BikeParent.eulerAngles.y, targetRot.eulerAngles.z);

        BikeBody.MoveRotation(newRotation);
    }

    private bool Grounded()
    {
        if(Physics.Raycast(SphereRB.position, Vector3.down, out hit, rayLenght, ground))
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    // private void Gravity()
    // {
    //     SphereRB.AddForce(gravity * Vector3.down, ForceMode.Acceleration);
    // }

    private void SkinMarks()
    {
        if (Grounded() && Mathf.Abs(velocity.x) > minSkinVelocity)
        {
            skinMarks.emitting = true;
            skidSound.mute = false;
        }
        else
        {
            skinMarks.emitting = false;
            skidSound.mute = true;
        }
    }

    private void EngineSound()
    {
        if (enabled)
        {
            engineSound.mute = false; 
            engineSound.pitch = Mathf.Lerp(minPitch, maxPitch, Mathf.Abs(curentVelocityOffset));
        }
        else
        {
            engineSound.mute = true;
        }
    }
    public void EnteredBike() // đã lên xe
    {
        isUsingBike = true;
        engineSound.Play();
        skidSound.Play();
    }
    public void ExittedBike()
    {
        isUsingBike = false;
        engineSound.Stop();
        skidSound.Stop();
    }
    public Transform GetSeat() => playerSeat;
}
