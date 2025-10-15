using Unity.VisualScripting;
using UnityEngine;

public class BikeController : MonoBehaviour
{
    RaycastHit hit;
    private float moveInput, steerInput, rayLenght, curentVelocityOffset;

    [HideInInspector] public Vector3 velocity;
    public GameObject Handle;
    public float maxSpeed, acceleration, steerStrength, gravity, bikeXTileIncrement, zTiltAngle, handleRotVal, handleRotSpeed;

    [Range(1, 10)]
    public float brakingFactory;

    public Rigidbody SphereRB, BikeBody;


    public LayerMask ground;
    private void Start()
    {
        SphereRB.transform.parent = null;
        BikeBody.transform.parent = null;

        rayLenght = SphereRB.GetComponent<SphereCollider>().radius + 0.2f;

    }

    private void Update()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");

        transform.position = SphereRB.transform.position;

        velocity = BikeBody.transform.InverseTransformDirection(BikeBody.velocity);
        curentVelocityOffset = velocity.z / maxSpeed;
        Debug.Log(SphereRB.velocity.magnitude);
    }

    private void FixedUpdate()
    {
        Movement();
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
            Gravity();
        }
        BikeTilt();
    }

    private void Acceleration()
    {
        SphereRB.velocity = Vector3.Lerp(SphereRB.velocity, maxSpeed * moveInput * transform.forward, Time.deltaTime * acceleration);
    }

    private void Rotation()
    {
        if(SphereRB.velocity.magnitude > 1)
        {
        transform.Rotate(0, steerInput * steerStrength * Time.fixedDeltaTime, 0, Space.World);
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
        Quaternion targetRot = Quaternion.Slerp(BikeBody.transform.rotation, Quaternion.Euler(xRot, transform.eulerAngles.y, zRot), bikeXTileIncrement);

        Quaternion newRotation = Quaternion.Euler(targetRot.eulerAngles.x, transform.eulerAngles.y, targetRot.eulerAngles.z);

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

    private void Gravity()
    {
        SphereRB.AddForce(gravity * Vector3.down, ForceMode.Acceleration);
    }
}
