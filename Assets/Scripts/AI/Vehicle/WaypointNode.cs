using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class WaypointNode : MonoBehaviour
{
    [Header("Links (filled by WaypointNetwork or manually)")]
    public List<WaypointNode> nextNodes = new List<WaypointNode>();
    public List<WaypointNode> previousNodes = new List<WaypointNode>();

    [Header("Gizmos")]
    public Color gizmoColor = Color.cyan;
    public float gizmoSize = 0.3f;
    public bool drawLabel = true;

    // Utility to pick a random next node (or null if none)
    public WaypointNode GetRandomNext()
    {
        if (nextNodes == null || nextNodes.Count == 0) return null;
        return nextNodes[Random.Range(0, nextNodes.Count)];
    }

    // helper for reset links
    public void ClearLinks()
    {
        nextNodes.Clear();
        previousNodes.Clear();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoSize);

        // draw forward direction
        Gizmos.color = Color.yellow;
        Vector3 fwd = transform.position + transform.forward * (gizmoSize * 2f);
        Gizmos.DrawLine(transform.position, fwd);
        // arrow head
        Vector3 right = Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0, 150, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0, 210, 0) * Vector3.forward;
        Gizmos.DrawLine(fwd, transform.position + right * gizmoSize);
        Gizmos.DrawLine(fwd, transform.position + left * gizmoSize);

        // draw links to next nodes
        if (nextNodes != null)
        {
            Gizmos.color = Color.green;
            foreach (var n in nextNodes)
            {
                if (n == null) continue;
                Gizmos.DrawLine(transform.position, n.transform.position);
                // small arrow at midpoint pointing to next
                Vector3 mid = Vector3.Lerp(transform.position, n.transform.position, 0.6f);
                Vector3 dir = (n.transform.position - transform.position).normalized;
                Vector3 a1 = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 140, 0) * Vector3.forward;
                Vector3 a2 = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 220, 0) * Vector3.forward;
                Gizmos.DrawLine(mid, mid + a1 * gizmoSize * 0.6f);
                Gizmos.DrawLine(mid, mid + a2 * gizmoSize * 0.6f);
            }
        }

#if UNITY_EDITOR
        if (drawLabel)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * (gizmoSize + 0.05f), gameObject.name);
        }
#endif
    }
}
