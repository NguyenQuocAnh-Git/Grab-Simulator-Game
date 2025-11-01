using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX, gridY;
    public int gCost, hCost;
    public Node parent;
    public int fCost => gCost + hCost;

    public bool isNearWall;
    public int wallLevel = 0;
    public Node(bool w, Vector3 pos, int x, int y)
    {
        walkable = w;
        worldPosition = pos;
        gridX = x;
        gridY = y;
    }
}
