using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapGrid : MonoBehaviour
{
    public Vector2 gridWorldSize = new Vector2(200, 200);
    public float nodeSize = 2f;
    public LayerMask obstacleMask;
    public Node[,] grid;

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
        float percentX = (worldPos.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPos.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }
    void UpdateNearWallNodes()
    {
        foreach (Node n in grid)
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
        }
    }
    void OnDrawGizmos()
    {
        if (grid == null)
            return;

        foreach (Node n in grid)
        {
            // Chọn màu cơ bản
            Color color = n.walkable ? Color.white : Color.black;

            // 🟧 Nếu là node gần tường
            if (n.isNearWall && n.walkable)
                color = new Color(1f, 0.5f, 0f); // cam

            Gizmos.color = color;
            Gizmos.DrawCube(n.worldPosition, Vector3.one * (1 - 0.05f));
        }
    }
}
