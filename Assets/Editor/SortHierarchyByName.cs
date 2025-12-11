using UnityEngine;
using UnityEditor;
using System.Linq;

public class SortHierarchyByName : MonoBehaviour
{
    [MenuItem("Tools/Sort Selected Objects By Name")]
    static void SortSelectedObjects()
    {
        var selected = Selection.transforms;

        if (selected.Length == 0)
        {
            Debug.Log("Không có GameObject nào được chọn!");
            return;
        }

        foreach (var parent in selected)
        {
            SortChildren(parent);
        }

        Debug.Log("Đã sort theo tên!");
    }

    static void SortChildren(Transform parent)
    {
        var children = parent.Cast<Transform>()
                             .OrderBy(t => t.name)
                             .ToList();

        for (int i = 0; i < children.Count; i++)
            children[i].SetSiblingIndex(i);
    }
}
