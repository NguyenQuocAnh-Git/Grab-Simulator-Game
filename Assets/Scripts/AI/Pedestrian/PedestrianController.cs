using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Collider))]
public class PedestrianController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 2.5f;
    public float arriveThreshold = 0.15f;
    public float rotateSpeedY = 360f;

    [Header("Ground & sampling")]
    public LayerMask groundLayerMask;
    public float groundSampleStartHeight = 2f;
    public float groundSampleMaxDrop = 5f;
    public float ySmoothSpeed = 10f;
    public float maxStepHeight = 0.4f;

    [Header("Collision / avoidance")]
    public LayerMask pedestrianLayerMask;   // layer for other pedestrians
    public LayerMask obstacleLayerMask;     // layer for vehicles/other obstacles (can include vehicle layer)
    public float sensorDistance = 1.5f;     // how far pedestrian senses ahead
    public float sensorRadius = 0.3f;
    public float frontStopDistance = 0.6f;  // if obstacle within this -> stop
    public float slowDownFactor = 0.5f;     // speed multiplier when slowing

    [Header("Path state")]
    public PedestrianNode currentNode;
    public PedestrianNode targetNode;

    [Header("Animation")]
    public Animator animator; // expect bool "isWalking"

    float currentSpeed;
    float targetY;

    private void Start()
    {
        currentSpeed = walkSpeed;
        float g = SampleGroundHeight(transform.position);
        if (!float.IsNaN(g)) transform.position = new Vector3(transform.position.x, g, transform.position.z);
    }

    private void Update()
    {
        HandleSensorsAndMovement();
        SyncAnimation();
    }

    void HandleSensorsAndMovement()
    {
        // sensor collects hits from both pedestrianLayerMask and obstacleLayerMask
        float nearest = Mathf.Infinity;
        RaycastHit? nearestHit = null;

        Vector3 origin = transform.position + Vector3.up * 0.1f;

        // check pedestrians
        var hits1 = Physics.SphereCastAll(origin, sensorRadius, transform.forward, sensorDistance, pedestrianLayerMask, QueryTriggerInteraction.Ignore);
        // check obstacles (vehicles, etc.)
        var hits2 = Physics.SphereCastAll(origin, sensorRadius, transform.forward, sensorDistance, obstacleLayerMask, QueryTriggerInteraction.Ignore);

        var allHits = hits1.Concat(hits2).ToArray();

        foreach (var h in allHits)
        {
            if (h.collider == null) continue;

            // ignore self colliders
            if (h.collider.transform.IsChildOf(transform)) continue;

            // ignore if it's part of same pedestrian (nested)
            var pc = h.collider.GetComponentInParent<PedestrianController>();
            if (pc == this) continue;

            // direction check: ensure in front (dot positive)
            Vector3 dirToHit = (h.point - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, dirToHit);
            if (dot < 0.2f) continue;

            if (h.distance < nearest)
            {
                nearest = h.distance;
                nearestHit = h;
            }
        }

        float desiredSpeed = walkSpeed;
        if (nearestHit.HasValue)
        {
            if (nearest <= frontStopDistance)
            {
                desiredSpeed = 0f;
            }
            else
            {
                desiredSpeed = walkSpeed * slowDownFactor;
            }
        }

        currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, (runSpeed * 4f) * Time.deltaTime);

        // path movement (line between nodes)
        if (targetNode == null)
        {
            RecoverTarget();
            if (targetNode == null) return;
        }

        Vector3 pos = transform.position;
        Vector3 tgt = targetNode.transform.position;
        Vector3 dirXZ = new Vector3(tgt.x - pos.x, 0f, tgt.z - pos.z);
        float distXZ = dirXZ.magnitude;

        if (distXZ <= arriveThreshold)
        {
            ArrivedAtNode();
            return;
        }

        // rotate yaw only toward movement direction
        if (dirXZ.sqrMagnitude > 0.0001f)
        {
            Quaternion desired = Quaternion.LookRotation(dirXZ.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, rotateSpeedY * Time.deltaTime);
        }

        if (currentSpeed > 0.001f)
        {
            Vector3 move = dirXZ.normalized * currentSpeed * Time.deltaTime;
            if (move.magnitude > distXZ) move = dirXZ;

            Vector3 nextXZ = new Vector3(pos.x + move.x, pos.y, pos.z + move.z);

            float gY = SampleGroundHeight(nextXZ + Vector3.up * groundSampleStartHeight);
            if (float.IsNaN(gY)) gY = pos.y;

            float clampedY = Mathf.MoveTowards(pos.y, gY, maxStepHeight * Time.deltaTime * 10f);
            float smoothY = Mathf.Lerp(pos.y, clampedY, Time.deltaTime * ySmoothSpeed);

            transform.position = new Vector3(nextXZ.x, smoothY, nextXZ.z);
            targetY = gY;
        }
    }

    void ArrivedAtNode()
    {
        currentNode = targetNode;
        if (currentNode == null) { RecoverTarget(); return; }
        // choose any connected next node (random). If none, pick random global node.
        if (currentNode.nextNodes != null && currentNode.nextNodes.Count > 0)
        {
            targetNode = currentNode.GetRandomNext();
        }
        else
        {
            // if no link, fallback to previous or global random
            if (currentNode.previousNodes != null && currentNode.previousNodes.Count > 0)
                targetNode = currentNode.previousNodes[Random.Range(0, currentNode.previousNodes.Count)];
            else RecoverTarget();
        }
    }

    void RecoverTarget()
    {
        var all = FindObjectsOfType<PedestrianNode>();
        if (all != null && all.Length > 0) targetNode = all[Random.Range(0, all.Length)];
    }

    float SampleGroundHeight(Vector3 startPos)
    {
        Vector3 dropStart = startPos + Vector3.up * groundSampleStartHeight;
        if (Physics.Raycast(dropStart, Vector3.down, out RaycastHit hit, groundSampleStartHeight + groundSampleMaxDrop, groundLayerMask, QueryTriggerInteraction.Ignore))
            return hit.point.y;
        return float.NaN;
    }

    void SyncAnimation()
    {
        if (animator == null) return;
        bool walking = currentSpeed > 0.05f;
        animator.SetBool("isWalking", walking);
        // optionally set speed float:
        // animator.SetFloat("speed", currentSpeed);
    }

    // Called by spawner to initialize position & set initial target
    public void InitializeAtNode(PedestrianNode startNode)
    {
        currentNode = startNode;
        if (startNode == null) { RecoverTarget(); }
        targetNode = currentNode != null ? currentNode.GetRandomNext() : null;

        Vector3 p = startNode != null ? startNode.transform.position : transform.position;
        float gY = SampleGroundHeight(p + Vector3.up * groundSampleStartHeight);
        if (!float.IsNaN(gY)) p.y = gY;
        transform.position = p;

        if (animator != null) animator.SetBool("isWalking", true);

        currentSpeed = walkSpeed;
    }
}
