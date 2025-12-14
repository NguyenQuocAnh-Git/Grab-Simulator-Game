using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MinimapGrid : MonoBehaviour
{
    public Vector2 gridWorldSize = new Vector2(200, 200);
    public float nodeSize = 1f;
    public LayerMask obstacleMask;
    public LayerMask sideWalk;
    public Node[,] grid;
    public int gridX, gridY; // debug
    int gridSizeX, gridSizeY;
    public Vector3 BottomLeft { get; private set; }

    void Awake()
    {
        BottomLeft =
            transform.position
            - Vector3.right * gridWorldSize.x * 0.5f
            - Vector3.forward * gridWorldSize.y * 0.5f;
    }

    void Start()
    {
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeSize);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeSize);
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 bottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        float nodeSizeX = 1f;
        float nodeSizeY = 1f;
        float checkHeight = 1f;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint =
                    bottomLeft
                    + Vector3.right   * (x * nodeSizeX + nodeSizeX * 0.5f)
                    + Vector3.forward * (y * nodeSizeY + nodeSizeY * 0.5f);

                Vector3 halfExtents = new Vector3(
                    nodeSizeX * 0.5f,   // 150
                    checkHeight,        // 0.5–1.0
                    nodeSizeY * 0.5f    // 140
                );

                bool walkable = !Physics.CheckBox(
                    worldPoint,
                    halfExtents,
                    Quaternion.identity,
                    obstacleMask
                );
                bool isSidewalk = Physics.CheckBox(
                    worldPoint,
                    halfExtents,
                    Quaternion.identity,
                    sideWalk      
                );

                grid[x, y] = new Node(walkable, worldPoint, x, y, isSidewalk);
            }
        }
        // UpdateNearWallNodes();
        UpdateNearSideWalk();
    }
    
    public Node NodeFromWorldPoint(Vector3 worldPos)
    {
        float relX = worldPos.x - BottomLeft.x;
        float relZ = worldPos.z - BottomLeft.z;

        int x = Mathf.FloorToInt(relX / nodeSize);
        int y = Mathf.FloorToInt(relZ / nodeSize);

        x = Mathf.Clamp(x, 0, gridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSizeY - 1);

        return grid[x, y];
    }
    private void UpdateNearSideWalk()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (!grid[x, y].walkable) continue;
                // Kiểm tra nhiều vòng xung quanh
                int minDistance = GetDistanceToNearestSideWalk(x, y);
                if (minDistance == 1 || minDistance == 2)
                    grid[x, y].penalty = 200;  // Rất gần vĩa hè
                else if (minDistance == 3 || minDistance == 4)
                    grid[x, y].penalty = 100;  // Gần vĩa hè
                else if (minDistance == 5 || minDistance == 6)
                    grid[x, y].penalty = 50;   // Hơi gần tường
                else
                    grid[x, y].penalty = 0;    // An toàn
            
            }
        }
    }
    void UpdateNearWallNodes()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (!grid[x, y].walkable) continue;
            
                // Kiểm tra nhiều vòng xung quanh
                int minDistance = GetDistanceToNearestWall(x, y);
                if (minDistance == 1)
                    grid[x, y].penalty = 200;  // Rất gần tường
                else if (minDistance == 2)
                    grid[x, y].penalty = 100;  // Gần tường
                else if (minDistance == 3)
                    grid[x, y].penalty = 100;   // Hơi gần tường
                else if(minDistance == 4)
                    grid[x, y].penalty = 50;    // hơi hơi gần tường
                else if(minDistance == 5)
                    grid[x, y].penalty = 50;
                else if(minDistance == 6)
                    grid[x, y].penalty = 25;
                else
                    grid[x, y].penalty = 0;    // An toàn
            }
        }
    }
    void OnDrawGizmos()
    {
        if (grid == null)
            return;
        
        foreach(Node n in grid)
        {
            if(n.isSidewalk)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeSize * 0.9f));
                continue;
            }
            if(!n.walkable)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeSize * 0.9f));
                continue;
            }
            if(n.penalty == 200)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeSize * 0.9f));
            }else if(n.penalty == 100)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeSize * 0.9f));
            }else if(n.penalty == 50)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeSize * 0.9f));
            }
            else
            {
                Gizmos.color = Color.white;
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeSize * 0.9f));
            }

        }
    }
    int GetDistanceToNearestSideWalk(int x, int y)
    {
         // Kiểm tra vòng tròn rộng hơn
        for (int radius = 1; radius <= 10; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int checkX = x + dx;
                    int checkY = y + dy;
                
                    if (checkX >= 0 && checkX < gridSizeX && 
                        checkY >= 0 && checkY < gridSizeY)
                    {
                        if (grid[checkX, checkY].isSidewalk)
                            return radius;
                    }
                }
            }
        }
        return 999; // Xa near
    }
    int GetDistanceToNearestWall(int x, int y)
    {
        // Kiểm tra vòng tròn rộng hơn
        for (int radius = 1; radius <= 10; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int checkX = x + dx;
                    int checkY = y + dy;
                
                    if (checkX >= 0 && checkX < gridSizeX && 
                        checkY >= 0 && checkY < gridSizeY)
                    {
                        if (!grid[checkX, checkY].walkable)
                            return radius;
                    }
                }
            }
        }
        return 999; // Xa tường
    }
}
