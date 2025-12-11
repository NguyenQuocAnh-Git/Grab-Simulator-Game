using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Collider))]
public class VehicleController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 4f;
    public float maxSpeed = 6f;
    public float rotateSpeedY = 720f;
    public float arriveThreshold = 0.2f;

    [Header("Ground Align")]
    public LayerMask groundLayerMask;
    public float groundSampleStartHeight = 5f;
    public float groundSampleMaxDrop = 10f;
    public float ySmoothSpeed = 5f;
    public float maxStepHeight = 0.6f;

    [Header("Collision Avoidance")]
    public LayerMask vehicleLayerMask;      // CHỈ layer của xe
    public float sensorDistance = 4.0f;
    public float sensorRadius = 0.5f;
    public float stopDistance = 1.0f;
    public float slowDownFactor = 0.4f;

    [Header("Hard Stop")]
    public float frontStopDistance = 2.0f;   // nếu xe khác nằm trong 2m → dừng hẳn

    [Header("Path State")]
    public WaypointNode currentNode;
    public WaypointNode targetNode;

    float currentSpeed;
    float targetY;
    bool initializedGround = false;


    /* ===============================================================
       START
    =============================================================== */
    private void Start()
    {
        currentSpeed = speed;

        float g = SampleGroundHeight(transform.position);
        if (!float.IsNaN(g))
        {
            transform.position = new Vector3(transform.position.x, g, transform.position.z);
            targetY = g;
        }
    }


    /* ===============================================================
       UPDATE LOOP
    =============================================================== */
    private void Update()
    {
        HandleMovement();
    }


    /* ===============================================================
       MOVEMENT + SENSOR
    =============================================================== */
    void HandleMovement()
    {
        float desiredSpeed = maxSpeed;

        // --------------------------
        // SENSOR PHÍA TRƯỚC
        // --------------------------
        Vector3 sensorOrigin = transform.position + Vector3.up * 0.2f;

        RaycastHit[] rawHits = Physics.SphereCastAll(
            sensorOrigin,
            sensorRadius,
            transform.forward,
            sensorDistance,
            vehicleLayerMask,
            QueryTriggerInteraction.Ignore
        );

        float nearest = Mathf.Infinity;
        RaycastHit? nearestHit = null;

        foreach (var hit in rawHits)
        {
            if (hit.collider == null) continue;

            // Ignore self colliders
            if (hit.collider.transform.IsChildOf(this.transform)) continue;

            // Ignore same vehicle rigidbody groups
            var rb = hit.collider.attachedRigidbody;
            if (rb != null)
            {
                var vc = rb.GetComponentInParent<VehicleController>();
                if (vc == this) continue;
            }
            else
            {
                var vc2 = hit.collider.GetComponentInParent<VehicleController>();
                if (vc2 == this) continue;
            }

            // Ensure the hit is in front
            Vector3 dirToHit = (hit.point - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, dirToHit);
            if (dot < 0.3f) continue;

            if (hit.distance < nearest)
            {
                nearest = hit.distance;
                nearestHit = hit;
            }
        }

        // --------------------------
        // SENSOR LOGIC
        // --------------------------
        if (nearestHit.HasValue)
        {
            if (nearest <= frontStopDistance)
            {
                desiredSpeed = 0f;  // hard stop
            }
            else if (nearest <= stopDistance)
            {
                desiredSpeed = 0f;
            }
            else
            {
                desiredSpeed = Mathf.Lerp(
                    speed * slowDownFactor,
                    speed,
                    Mathf.InverseLerp(stopDistance, sensorDistance, nearest)
                );
            }
        }
        else
        {
            desiredSpeed = maxSpeed;
        }

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            desiredSpeed,
            (maxSpeed * 4f) * Time.deltaTime
        );


        // --------------------------
        // PATH MOVEMENT
        // --------------------------
        if (targetNode == null)
        {
            RecoverTarget();
            if (targetNode == null) return;
        }

        Vector3 pos = transform.position;
        Vector3 target = targetNode.transform.position;

        Vector3 dirXZ = new Vector3(target.x - pos.x, 0, target.z - pos.z);
        float distXZ = dirXZ.magnitude;

        if (distXZ <= arriveThreshold)
        {
            ArrivedAtNode();
            return;
        }

        // rotate yaw only
        if (dirXZ.sqrMagnitude > 0.0001f)
        {
            Quaternion desiredRot = Quaternion.LookRotation(dirXZ.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRot, rotateSpeedY * Time.deltaTime);
        }

        // move XZ
        if (currentSpeed > 0.001f)
        {
            Vector3 move = dirXZ.normalized * currentSpeed * Time.deltaTime;
            if (move.magnitude > distXZ) move = dirXZ;

            Vector3 nextPosXZ = new Vector3(pos.x + move.x, pos.y, pos.z + move.z);

            float gY = SampleGroundHeight(nextPosXZ + Vector3.up * groundSampleStartHeight);
            if (float.IsNaN(gY)) gY = targetY;

            float clampedY = Mathf.MoveTowards(pos.y, gY, maxStepHeight * Time.deltaTime * 10f);
            float smoothY = Mathf.Lerp(pos.y, clampedY, Time.deltaTime * ySmoothSpeed);

            transform.position = new Vector3(nextPosXZ.x, smoothY, nextPosXZ.z);

            targetY = gY;
        }
    }


    /* ===============================================================
       WAYPOINT LOGIC
    =============================================================== */
    void ArrivedAtNode()
    {
        currentNode = targetNode;
        targetNode = PickNext(currentNode);

        if (targetNode == null)
        {
            RecoverTarget();
        }
    }

    WaypointNode PickNext(WaypointNode node)
    {
        if (node == null) return null;
        if (node.nextNodes != null && node.nextNodes.Count > 0) return node.GetRandomNext();
        if (node.previousNodes != null && node.previousNodes.Count > 0)
            return node.previousNodes[Random.Range(0, node.previousNodes.Count)];
        return null;
    }

    void RecoverTarget()
    {
        var all = FindObjectsOfType<WaypointNode>();
        if (all.Length > 0)
            targetNode = all[Random.Range(0, all.Length)];
    }


    /* ===============================================================
       INIT FROM SPAWNER
    =============================================================== */
    public void InitializeAtNode(WaypointNode startNode)
    {
        currentNode = startNode;
        targetNode = PickNext(startNode);

        Vector3 p = startNode.transform.position;

        float gY = SampleGroundHeight(p + Vector3.up * groundSampleStartHeight);
        if (!float.IsNaN(gY)) p.y = gY;

        transform.position = p;

        Vector3 f = startNode.transform.forward;
        f.y = 0;
        if (f.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(f, Vector3.up);

        currentSpeed = speed;
    }


    /* ===============================================================
       GROUND SAMPLING
    =============================================================== */
    float SampleGroundHeight(Vector3 startPos)
    {
        Vector3 dropStart = startPos + Vector3.up * groundSampleStartHeight;

        if (Physics.Raycast(dropStart, Vector3.down, out RaycastHit hit, groundSampleStartHeight + groundSampleMaxDrop, groundLayerMask))
            return hit.point.y;

        return float.NaN;
    }
}
