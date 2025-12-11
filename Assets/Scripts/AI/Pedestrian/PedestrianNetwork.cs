using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PedestrianNetwork : MonoBehaviour
{
    [Header("Auto link settings")]
    public float maxLinkDistance = 6f;      // nodes within this distance are linked
    public bool clearExistingLinksBeforeLinking = true;
    public bool includeInactiveChildren = false;

    [Header("Gizmos")]
    public Color networkColor = Color.magenta;

    [ContextMenu("Auto Link Children")]
    public void AutoLinkChildren()
    {
        List<PedestrianNode> nodes = new List<PedestrianNode>();
        foreach (Transform t in transform)
        {
            if (!includeInactiveChildren && !t.gameObject.activeInHierarchy) continue;
            PedestrianNode pn = t.GetComponent<PedestrianNode>();
            if (pn != null) nodes.Add(pn);
        }

        if (clearExistingLinksBeforeLinking)
        {
            foreach (var n in nodes) n.ClearLinks();
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            var a = nodes[i];
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue;
                var b = nodes[j];
                float d = Vector3.Distance(a.transform.position, b.transform.position);
                if (d <= maxLinkDistance)
                {
                    if (!a.nextNodes.Contains(b)) a.nextNodes.Add(b);
                    if (!b.previousNodes.Contains(a)) b.previousNodes.Add(a);
                }
            }
        }

#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = networkColor;
    }
}
