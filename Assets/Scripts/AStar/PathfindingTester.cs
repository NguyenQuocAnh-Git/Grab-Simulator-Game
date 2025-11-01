using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PathfindingTester : MonoBehaviour
{
    public MiniMapPathfinder pathfinder;
    public Transform player;
    public Transform target;
    public GameObject node;
    public GameObject nodeB;
    public GameObject nodeC;
    public GameObject nodeD;
    public List<GameObject> nodesA;
    public List<GameObject> nodesB;
    public List<GameObject> nodesC;
    public List<GameObject> nodesD;
    void Start()
    {
        nodesA = new List<GameObject>();
        nodesB = new List<GameObject>();
        nodesC = new List<GameObject>();
        nodesD = new List<GameObject>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            RemoveListA();
            StartCoroutine(TestPathCorotineA());
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            RemoveListB();
            StartCoroutine(TestPathCorotineB());
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            RemoveListC();
            StartCoroutine(TestPathCorotineC());
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            RemoveListD();
            StartCoroutine(TestPathCorotineD());
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            RemoveListA();
            RemoveListB();
            RemoveListC();
            RemoveListD();
        }
    }
    IEnumerator TestPathCorotineA()
    {
        if (pathfinder == null || player == null || target == null)
        {
            Debug.LogError("Thiếu tham chiếu (pathfinder/player/target)!");
            yield break;
        }

        Debug.Log($"Bắt đầu tìm đường từ {player.position} → {target.position}");

        List<Node> path = pathfinder.FindPathA(player.position, target.position);

        if (path == null || path.Count == 0)
        {
            Debug.Log("Không tìm thấy đường đi!");
            yield break;
        }

        Debug.Log($"Tìm được {path.Count} node trong đường đi:");

        for (int i = 0; i < path.Count; i++)
        {
            GameObject go = Instantiate(node, path[i].worldPosition + Vector3.up * 0.5f, Quaternion.identity);
            nodesA.Add(go);
            Debug.Log($"Node {i}: {path[i].worldPosition}");
        }
    }
    IEnumerator TestPathCorotineB()
    {
        if (pathfinder == null || player == null || target == null)
        {
            Debug.LogError("Thiếu tham chiếu (pathfinder/player/target)!");
            yield break;
        }

        Debug.Log($"Bắt đầu tìm đường từ {player.position} → {target.position}");

        List<Node> path = pathfinder.FindPathB(player.position, target.position);

        if (path == null || path.Count == 0)
        {
            Debug.Log("Không tìm thấy đường đi!");
            yield break;
        }

        Debug.Log($"Tìm được {path.Count} node trong đường đi:");

        for (int i = 0; i < path.Count; i++)
        {
            GameObject go = Instantiate(nodeB, path[i].worldPosition + Vector3.up * 0.5f, Quaternion.identity);
            nodesB.Add(go);
            Debug.Log($"Node {i}: {path[i].worldPosition}");
        }
    }
    IEnumerator TestPathCorotineC()
    {
        if (pathfinder == null || player == null || target == null)
        {
            Debug.LogError("Thiếu tham chiếu (pathfinder/player/target)!");
            yield break;
        }

        Debug.Log($"Bắt đầu tìm đường từ {player.position} → {target.position}");

        List<Node> path = pathfinder.FindPathC(player.position, target.position);

        if (path == null || path.Count == 0)
        {
            Debug.Log("Không tìm thấy đường đi!");
            yield break;
        }

        Debug.Log($"Tìm được {path.Count} node trong đường đi:");

        for (int i = 0; i < path.Count; i++)
        {
            GameObject go = Instantiate(nodeC, path[i].worldPosition + Vector3.up * 0.5f, Quaternion.identity);
            nodesC.Add(go);
            Debug.Log($"Node {i}: {path[i].worldPosition}");
        }
    }
    IEnumerator TestPathCorotineD()
    {
        if (pathfinder == null || player == null || target == null)
        {
            Debug.LogError("Thiếu tham chiếu (pathfinder/player/target)!");
            yield break;
        }

        Debug.Log($"Bắt đầu tìm đường từ {player.position} → {target.position}");

        List<Node> path = pathfinder.FindPathD(player.position, target.position);

        if (path == null || path.Count == 0)
        {
            Debug.Log("Không tìm thấy đường đi!");
            yield break;
        }

        Debug.Log($"Tìm được {path.Count} node trong đường đi:");

        for (int i = 0; i < path.Count; i++)
        {
            GameObject go = Instantiate(nodeD, path[i].worldPosition + Vector3.up * 0.5f, Quaternion.identity);
            nodesD.Add(go);
            Debug.Log($"Node {i}: {path[i].worldPosition}");
        }
    }
    void RemoveListA()
    {
        foreach (var item in nodesA)
        {
            Destroy(item);
        }
        nodesA.Clear();
    }
    void RemoveListB()
    {
        foreach (var item in nodesB)
        {
            Destroy(item);
        }
        nodesB.Clear();
    }
    void RemoveListC()
    {
        foreach (var item in nodesC)
        {
            Destroy(item);
        }
        nodesC.Clear();
    }
    void RemoveListD()
    {
        foreach (var item in nodesD)
        {
            Destroy(item);
        }
        nodesD.Clear();
    }
    async Task TestPath()
    {
        if (pathfinder == null || player == null || target == null)
        {
            Debug.LogError("Thiếu tham chiếu (pathfinder/player/target)!");
            return;
        }

        Debug.Log($"Bắt đầu tìm đường từ {player.position} → {target.position}");

        List<Node> path = pathfinder.FindPathA(player.position, target.position);

        if (path == null || path.Count == 0)
        {
            Debug.Log("Không tìm thấy đường đi!");
            return;
        }

        Debug.Log($"Tìm được {path.Count} node trong đường đi:");

        for (int i = 0; i < path.Count; i++)
        {
            Instantiate(node, path[i].worldPosition + Vector3.up * 0.5f, Quaternion.identity);
            await Task.Delay(TimeSpan.FromSeconds(0.5f));
            Debug.Log($"Node {i}: {path[i].worldPosition}");
        }
    }
}
