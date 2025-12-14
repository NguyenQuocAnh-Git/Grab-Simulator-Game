using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapPathfinder : MonoBehaviour
{
    public MinimapGrid grid;
    public int penaltyNearWall = 10;
    public List<Vector3> FindPathWorld(Vector3 startPos, Vector3 targetPos)
    {
        List<Node> nodePath = FindPathD(startPos, targetPos);

        if (nodePath == null || nodePath.Count == 0)
            return null;

        List<Vector3> result = new List<Vector3>(nodePath.Count);

        for (int i = 0; i < nodePath.Count; i++)
            result.Add(nodePath[i].worldPosition);

        return result;
    }
    public List<Node> FindPathA(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < current.fCost ||
                    (openSet[i].fCost == current.fCost && openSet[i].hCost < current.hCost))
                    current = openSet[i];
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (Node neighbor in GetNeighbours(current))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int newCost = current.gCost + GetDistanceA(current, neighbor);
                if (newCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost = GetDistanceA(neighbor, targetNode);
                    neighbor.parent = current;
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        return null;
    }
    public List<Node> FindPathB(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < current.fCost ||
                    (openSet[i].fCost == current.fCost && openSet[i].hCost < current.hCost))
                    current = openSet[i];
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (Node neighbor in GetNeighbours(current))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int newCost = current.gCost + GetDistanceB(current, neighbor);
                if (newCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost = GetDistanceA(neighbor, targetNode);
                    neighbor.parent = current;
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        return null;
    }
    public List<Node> FindPathC(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < current.fCost ||
                    (openSet[i].fCost == current.fCost && openSet[i].hCost < current.hCost))
                    current = openSet[i];
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (Node neighbor in GetNeighbours(current))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int newCost = current.gCost + GetDistanceA(current, neighbor);
                if (newCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost = GetDistanceC(neighbor, targetNode);
                    neighbor.parent = current;
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        return null;
    }
    public List<Node> FindPathD(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);
        
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < current.fCost ||
                    (openSet[i].fCost == current.fCost && openSet[i].hCost < current.hCost))
                    current = openSet[i];
            }
            
            openSet.Remove(current);
            closedSet.Add(current);
            
            if (current == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (Node neighbor in GetNeighbours(current))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;


                int wallPenalty = neighbor.penalty;  // Dùng giá trị động
                int newCost = current.gCost + GetDistanceC(current, neighbor) 
                                            + wallPenalty;
    
                if (newCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newCost;
                    neighbor.hCost = GetDistanceC(neighbor, targetNode);
                    neighbor.parent = current;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        return null;
    }
    List<Node> RetracePath(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node current = end;

        int safety = 0; // chống loop vô hạn
        while (current != null && current != start)
        {
            path.Add(current);
            current = current.parent;

            if (++safety > 10000)
            {
                Debug.LogError("RetracePath infinite loop detected");
                break;
            }
        }

        path.Reverse();
        return path;
    }

    // A - Manhattan Distance
    int GetDistanceA(Node a, Node b)
    {
        int dx = Mathf.Abs(a.gridX - b.gridX);
        int dy = Mathf.Abs(a.gridY - b.gridY);
        return 10 * (dx + dy);
    }
    // B - Diagonal Distance
    int GetDistanceB(Node a, Node b)
    {
        int dx = Mathf.Abs(a.gridX - b.gridX);
        int dy = Mathf.Abs(a.gridY - b.gridY);
        return 14 * Mathf.Min(dx, dy) + 10 * Mathf.Abs(dx - dy);
    }
    // C - Euclidean Distance
    int GetDistanceC(Node a, Node b)
    {
        /*int dx = Mathf.Abs(a.gridX - b.gridX);
        int dy = Mathf.Abs(a.gridY - b.gridY);
        return Mathf.RoundToInt(10 * Mathf.Sqrt(dx * dx + dy * dy));*/
        
        int dx = Mathf.Abs(a.gridX - b.gridX);
        int dy = Mathf.Abs(a.gridY - b.gridY);
    
        // Thay vì dùng Euclidean, dùng Manhattan hoặc tăng cost chéo
        if (dx != 0 && dy != 0) // Di chuyển chéo
        {
            // Tăng cost chéo lên để ưu tiên đi thẳng
            return (dx + dy) * 20;
        }
        else // Di chuyển thẳng
        {
            return (dx + dy) * 10;
        }
        
    }

    /*List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;
                if (checkX >= 0 && checkX < grid.grid.GetLength(0) && checkY >= 0 && checkY < grid.grid.GetLength(1))
                    neighbours.Add(grid.grid[checkX, checkY]);
            }
        }
        return neighbours;
    }*/
    
    List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
    
        // Chỉ lấy 4 hướng: lên, xuống, trái, phải (không chéo)
        int[,] directions = { {0,1}, {0,-1}, {1,0}, {-1,0} };
    
        for (int i = 0; i < 4; i++)
        {
            int checkX = node.gridX + directions[i,0];
            int checkY = node.gridY + directions[i,1];
        
            if (checkX >= 0 && checkX < grid.grid.GetLength(0) && 
                checkY >= 0 && checkY < grid.grid.GetLength(1))
            {
                neighbours.Add(grid.grid[checkX, checkY]);
            }
        }
    
        return neighbours;
    }
    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        return FindPathD(startPos, targetPos);
    }
}
