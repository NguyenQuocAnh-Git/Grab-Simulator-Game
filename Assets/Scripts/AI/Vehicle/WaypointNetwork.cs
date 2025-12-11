using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class WaypointNetwork : MonoBehaviour
{
    [Header("Auto link settings")]
    public string waypointTag = ""; // nếu muốn dùng tag. để trống nếu không
    public float maxLinkDistance = 10f;
    [Range(0f, 1f)] public float forwardDotThreshold = 0.7f; // tương ứng góc ~ acos(0.7)=~45°
    public bool clearExistingLinksBeforeLinking = true;
    public bool includeInactiveChildren = false;

    [Header("Gizmos")]
    public Color networkColor = Color.green;

    [ContextMenu("Auto Link Children")]
    public void AutoLinkChildren()
    {
        // collect waypoint nodes among children (or by tag if provided)
        List<WaypointNode> nodes = new List<WaypointNode>();
        foreach (Transform t in transform)
        {
            if (!includeInactiveChildren && !t.gameObject.activeInHierarchy) continue;
            WaypointNode wn = t.GetComponent<WaypointNode>();
            if (wn != null)
            {
                if (!string.IsNullOrEmpty(waypointTag))
                {
                    if (t.CompareTag(waypointTag))
                        nodes.Add(wn);
                }
                else nodes.Add(wn);
            }
        }

        if (clearExistingLinksBeforeLinking)
        {
            foreach (var n in nodes) n.ClearLinks();
        }

        // for each node, find nodes ahead within distance and dot threshold
        foreach (var a in nodes)
        {
            Vector3 apos = a.transform.position;
            Vector3 afwd = a.transform.forward.normalized;

            foreach (var b in nodes)
            {
                if (a == b) continue;
                Vector3 toB = (b.transform.position - apos);
                float dist = toB.magnitude;
                if (dist > maxLinkDistance) continue;
                Vector3 dir = toB.normalized;
                float dot = Vector3.Dot(afwd, dir);
                if (dot >= forwardDotThreshold)
                {
                    // b is in front of a => link
                    if (!a.nextNodes.Contains(b)) a.nextNodes.Add(b);
                    if (!b.previousNodes.Contains(a)) b.previousNodes.Add(a);
                }
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    // optional: run automatically on validate in edit mode
    private void OnValidate()
    {
        // don't auto-run to avoid surprises, but keep method available via button/ContextMenu
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = networkColor;
        // draw nothing else here; each WaypointNode draws its own links
    }
}
