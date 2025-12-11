using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PedestrianNode : MonoBehaviour
{
    [Header("Links (filled by PedestrianNetwork or manually)")]
    public List<PedestrianNode> nextNodes = new List<PedestrianNode>();
    public List<PedestrianNode> previousNodes = new List<PedestrianNode>();

    [Header("Gizmos")]
    public Color gizmoColor = Color.magenta;
    public float gizmoSize = 0.15f;
    public bool drawLabel = true;

    public PedestrianNode GetRandomNext()
    {
        if (nextNodes == null || nextNodes.Count == 0) return null;
        return nextNodes[Random.Range(0, nextNodes.Count)];
    }

    public void ClearLinks()
    {
        nextNodes.Clear();
        previousNodes.Clear();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoSize);

#if UNITY_EDITOR
        if (drawLabel)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * (gizmoSize + 0.02f), gameObject.name);
        }
#endif

        if (nextNodes != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var n in nextNodes)
            {
                if (n == null) continue;
                Gizmos.DrawLine(transform.position, n.transform.position);
            }
        }
    }
}
