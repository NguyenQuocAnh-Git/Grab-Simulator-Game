using System.Collections.Generic;
using UnityEngine;

public static class TagRegistry
{
    private static readonly Dictionary<string, List<GameObject>> map = new();

    public static void Register(string tag, GameObject obj)
    {
        if (!map.TryGetValue(tag, out var list))
        {
            list = new List<GameObject>();
            map[tag] = list;
        }
        list.Add(obj);
    }

    public static void Unregister(string tag, GameObject obj)
    {
        if (map.TryGetValue(tag, out var list))
            list.Remove(obj);
    }

    public static IReadOnlyList<GameObject> Get(string tag)
    {
        return map.TryGetValue(tag, out var list) ? list : System.Array.Empty<GameObject>();
    }
}