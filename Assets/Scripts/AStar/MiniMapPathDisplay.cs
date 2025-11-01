using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapPathDisplay : MonoBehaviour
{
    public LineRenderer line;
    public MiniMapDrawer mapDrawer;
    public MiniMapPathfinder pathfinder;
    public Transform player;
    public Transform target;

    void Update()
    {
        var path = pathfinder.FindPathA(player.position, target.position);
        if (path == null) return;

        line.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            Vector2 mapPos = mapDrawer.WorldToMap(path[i].worldPosition);
            line.SetPosition(i, new Vector3(mapPos.x, mapPos.y, 0));
        }
    }
}
