using System.Collections.Generic;
using UnityEngine;

public class MiniMapPathDisplay : MonoBehaviour
{
    [SerializeField] LineRenderer line;
    [SerializeField] float minimapPlaneY = 1f;
    [SerializeField] float lift = 0.02f;

    [Header("Trim Params")]
    [SerializeField] float nodeReachRadius = 0.6f;
    [SerializeField] float segProgressT = 0.75f;
    [SerializeField] float segCloseRadius = 0.5f;

    [Header("Render Limit")]
    [SerializeField] int maxDrawNodes = 9999;

    readonly List<Vector3> path = new();
    Vector3[] buffer = new Vector3[0];
    int head;

    void Awake()
    {
        if (!line) line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
    }

    public bool HasPath() {
        bool result = path.Count - head > 0;
        return result;
    }

    /* ================= SET / CLEAR ================= */

    public void SetPath(List<Vector3> worldPath)
    {
        Clear();
        if (worldPath == null || worldPath.Count == 0) return;

        path.AddRange(worldPath);
        head = 0;
        RedrawImmediate(worldPath[0]);
    }

    public void Clear()
    {
        path.Clear();
        head = 0;
        line.positionCount = 0;
    }

    /* ================= CORE UPDATE ================= */

    public bool CheckDeviation(Vector3 pos, float maxDeviationSqr)
    {
        if (!HasPath())
        {
            Debug.Log("Has not path -> return false");
            return false;
        }else
        {
            Debug.Log("Has path");
        }

        TrimPassedNodes(pos);

        Vector2 p = ToXZ(pos);
        float nearest = float.MaxValue;

        int remaining = Mathf.Min(path.Count - head, maxDrawNodes);
        for (int i = 1; i < remaining; i++)
        {
            Vector2 a = ToXZ(path[head + i - 1]);
            Vector2 b = ToXZ(path[head + i]);
            float d = DistancePointToSegmentSqr(p, a, b);
            if (d < nearest) nearest = d;
        }

        return nearest > maxDeviationSqr;
    }
    public void UpdateHeadPosition(Vector3 pos)
    {
        if (!HasPath() || line.positionCount == 0) return;

        line.SetPosition(0, new Vector3(
            pos.x,
            minimapPlaneY + lift,
            pos.z
        ));
    }

    public bool UpdateTrimAndRedraw(Vector3 pos)
    {
        if (!HasPath()) return false;

        int oldHead = head;
        TrimPassedNodes(pos);

        if (head != oldHead)
        {
            RedrawImmediate(pos);
            return true;
        }
        return false;
    }

    /* ================= INTERNAL ================= */

    void TrimPassedNodes(Vector3 pos)
{
    Vector2 p = ToXZ(pos);
    float nodeReachSqr = nodeReachRadius * nodeReachRadius;
    float segCloseSqr  = segCloseRadius * segCloseRadius;

    while (head < path.Count - 1)
    {
        Vector2 a = ToXZ(path[head]);
        Vector2 b = ToXZ(path[head + 1]);

        // 1) Tới gần node kế tiếp
        if ((b - p).sqrMagnitude <= nodeReachSqr)
        {
            head++; 
            continue;
        }

        Vector2 ab = b - a;
        float len = ab.sqrMagnitude;
        if (len < 1e-4f)
        {
            head++; 
            continue;
        }

        float t = Vector2.Dot(p - a, ab) / len; // KHÔNG clamp
        Vector2 proj = a + ab * Mathf.Clamp01(t);

        // 2) Đi qua node theo hướng đoạn (fix đi thẳng không mất đoạn)
        if (t > 1f)
        {
            head++;
            continue;
        }

        // 3) Đi qua phần lớn đoạn hoặc đi sát đoạn
        if (t >= segProgressT || (p - proj).sqrMagnitude <= segCloseSqr)
        {
            head++;
            continue;
        }

        break;
        }

        if (head > 64)
        {
            path.RemoveRange(0, head);
            head = 0;
        }
    }


    void RedrawImmediate(Vector3 pos)
    {
        if (!HasPath())
        {
            line.positionCount = 0;
            return;
        }

        int count = Mathf.Min(path.Count - head, maxDrawNodes);
        EnsureBuffer(count);

        buffer[0] = Project(pos);
        for (int i = 1; i < count; i++)
            buffer[i] = Project(path[head + i]);

        line.positionCount = count;
        line.SetPositions(buffer);
    }

    void EnsureBuffer(int size)
    {
        if (buffer.Length < size)
            buffer = new Vector3[size];
    }

    static Vector3 Project(Vector3 v)
        => new(v.x, 1f + 0.02f, v.z);

    static Vector2 ToXZ(Vector3 v)
        => new(v.x, v.z);

    static float DistancePointToSegmentSqr(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float len = ab.sqrMagnitude;
        if (len < 0.0001f) return (p - a).sqrMagnitude;

        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / len);
        Vector2 proj = a + ab * t;
        return (p - proj).sqrMagnitude;
    }
    void OnDrawGizmos()
    {
        if (!HasPath() || path == null || path.Count - head < 2) return;

        Vector3 pos = Application.isPlaying
            ? buffer != null && buffer.Length > 0 ? buffer[0] : path[head]
            : path[head];

        Vector2 p = new(pos.x, pos.z);

        Vector3 a3 = path[head];
        Vector3 b3 = path[head + 1];

        Vector2 a = new(a3.x, a3.z);
        Vector2 b = new(b3.x, b3.z);

        // 1️⃣ nodeReachRadius (vòng tròn node kế tiếp)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(b3, nodeReachRadius);

        // 2️⃣ segCloseRadius (khoảng cách tới đoạn)
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        Vector2 proj = a + ab * t;
        Vector3 proj3 = new(proj.x, minimapPlaneY, proj.y);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(proj3, segCloseRadius);

        // 3️⃣ segProgressT (điểm tiến độ trên đoạn)
        Vector3 progressPoint = Vector3.Lerp(a3, b3, segProgressT);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(progressPoint, 0.15f);

        // Đoạn path
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(a3, b3);
    }
    public bool IsHeadDriftTooFar(Vector3 pos, float maxDist)
    {
        if (!HasPath() || path.Count - head <= 0)
            return false;

        return Vector3.Distance(pos, path[head]) > maxDist;
    }

    public bool NeedRecomputeByDeviation(Vector3 pos, float maxDev)
    {
        if (path.Count - head < 2) return false;

        Vector2 p = ToXZ(pos);
        Vector2 a = ToXZ(path[head]);
        Vector2 b = ToXZ(path[head + 1]);

        float d = Mathf.Sqrt(DistancePointToSegmentSqr(p, a, b));
        return d > maxDev;
    }

}
