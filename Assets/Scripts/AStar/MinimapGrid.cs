using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapGrid : MonoBehaviour
{
    public Vector2 gridWorldSize = new Vector2(200, 200);
    public float nodeSize = 2f;
    public LayerMask obstacleMask;
    public Node[,] grid;
    public int gridX, gridY; // debug
    int gridSizeX, gridSizeY;

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

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * nodeSize + nodeSize / 2) + Vector3.forward * (y * nodeSize + nodeSize / 2);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeSize / 2, obstacleMask));
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
        UpdateNearWallNodes();
    }
    
    public Node NodeFromWorldPoint(Vector3 worldPos)
    {
        Vector3 bottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2f - Vector3.forward * gridWorldSize.y / 2f;
        float relX = worldPos.x - bottomLeft.x;
        float relZ = worldPos.z - bottomLeft.z;
    
        int x = Mathf.FloorToInt(relX / nodeSize);
        int y = Mathf.FloorToInt(relZ / nodeSize);
    
        x = Mathf.Clamp(x, 0, gridSizeX - 1);
        y = Mathf.Clamp(y, 0, gridSizeY - 1);
    
        return grid[x, y];
    }
    void UpdateNearWallNodes()
    {
        /*foreach (Node n in grid)
        {
            if (!n.walkable)
            {
                n.isNearWall = false;
                continue;
            }

            bool nearWall = false;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    int checkX = n.gridX + x;
                    int checkY = n.gridY + y;
                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        if (!grid[checkX, checkY].walkable)
                        {
                            nearWall = true;
                            break;
                        }
                    }
                }
                if (nearWall) break;
            }

            n.isNearWall = nearWall;
        }*/
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (!grid[x, y].walkable) continue;
            
                // Kiểm tra nhiều vòng xung quanh
                int minDistance = GetDistanceToNearestWall(x, y);
            
                if (minDistance == 1)
                    grid[x, y].wallPenalty = 200;  // Rất gần tường
                else if (minDistance == 2)
                    grid[x, y].wallPenalty = 100;  // Gần tường
                else if (minDistance == 3)
                    grid[x, y].wallPenalty = 50;   // Hơi gần tường
                else
                    grid[x, y].wallPenalty = 0;    // An toàn
            }
        }
    }
    void OnDrawGizmos()
    {
        if (grid == null)
            return;
        // Chọn màu cơ bản
        foreach (Node n in grid)
        {
            Color color = n.walkable ? Color.white : Color.black;
            if (n.isNearWall && n.walkable) color = new Color(1f, 0.5f, 0f);
            Gizmos.color = color;
            Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeSize * 0.9f));
        }
    }
    int GetDistanceToNearestWall(int x, int y)
    {
        // Kiểm tra vòng tròn rộng hơn
        for (int radius = 1; radius <= 3; radius++)
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
