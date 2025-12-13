using System.Collections.Generic;
using UnityEngine;

public class MiniMapPathDisplay : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    [SerializeField] private float minimapPlaneY = 1f;
    [SerializeField] private float lift = 0.02f;

    // trimming/projection params
    [SerializeField] private float nodeReachRadius = 0.6f;      // meters
    [SerializeField] private float segProgressT = 0.75f;        // projection threshold
    [SerializeField] private float segCloseRadius = 0.5f;       // close to segment considered passed

    private List<Vector3> pathNodes = new List<Vector3>();
    private Vector3[] buffer = new Vector3[0];
    private int headIndex = 0;

    void Awake()
    {
        if (line == null) line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
    }

    public void SetPath(List<Vector3> worldPath)
    {
        if (worldPath == null || worldPath.Count == 0) { Clear(); return; }
        pathNodes.Clear();
        pathNodes.AddRange(worldPath);
        headIndex = 0;
        EnsureBuffer(pathNodes.Count);
        RefreshBufferImmediate();
    }

    void EnsureBuffer(int required)
    {
        if (buffer.Length < required) buffer = new Vector3[required];
    }

    // gọi mỗi frame từ controller, trả về true nếu player quá lệch -> cần recompute
    public bool UpdateAndDraw_CheckDeviation(Vector3 motoPos, float maxAllowedDeviationSqr, out float nearestSqr)
    {
        nearestSqr = float.MaxValue;
        if (IsEmpty)
        {
            line.positionCount = 0;
            return false;
        }

        // trim visited using segment-projection logic
        TrimVisitedByPosition(motoPos);

        int remaining = pathNodes.Count - headIndex;
        if (remaining <= 0) { Clear(); return false; }

        EnsureBuffer(remaining);

        // set first point to current moto position (project to minimap plane height)
        buffer[0] = new Vector3(motoPos.x, minimapPlaneY + lift, motoPos.z);

        // fill buffer and compute nearest distance to any segment
        nearestSqr = float.MaxValue;
        Vector2 p = new Vector2(motoPos.x, motoPos.z);

        for (int i = 1; i < remaining; i++)
        {
            Vector3 wp = pathNodes[headIndex + i];
            Vector3 wpp = new Vector3(wp.x, minimapPlaneY + lift, wp.z);
            buffer[i] = wpp;

            // compute distance to segment (i-1 -> i) in XZ
            Vector2 a = new Vector2((i == 1 ? motoPos.x : pathNodes[headIndex + i - 1].x),
                                      (i == 1 ? motoPos.z : pathNodes[headIndex + i - 1].z));
            Vector2 b = new Vector2(pathNodes[headIndex + i].x, pathNodes[headIndex + i].z);
            float distSegSqr = DistancePointToSegmentSqr(p, a, b);
            if (distSegSqr < nearestSqr) nearestSqr = distSegSqr;
        }

        line.positionCount = remaining;
        line.SetPositions(buffer);

        // if nearest distance bigger than threshold -> recompute recommended
        return nearestSqr > maxAllowedDeviationSqr;
    }
    
    void TrimVisitedByPosition(Vector3 motoPos)
    {
        if (IsEmpty) return;

        Vector2 p = new Vector2(motoPos.x, motoPos.z);
        float nodeReachSqr = nodeReachRadius * nodeReachRadius;
        float segCloseSqr = segCloseRadius * segCloseRadius;

        while (headIndex < pathNodes.Count - 1)
        {
            Vector2 a = new Vector2(pathNodes[headIndex].x, pathNodes[headIndex].z);
            Vector2 b = new Vector2(pathNodes[headIndex + 1].x, pathNodes[headIndex + 1].z);

            // if already very close to next node -> advance
            if ((b - p).sqrMagnitude <= nodeReachSqr) { headIndex++; continue; }

            Vector2 ab = b - a;
            float abLenSqr = ab.sqrMagnitude;
            if (abLenSqr <= Mathf.Epsilon) { headIndex++; continue; }

            float t = Vector2.Dot(p - a, ab) / abLenSqr;
            if (t >= 1f) { headIndex++; continue; }

            t = Mathf.Clamp01(t);
            Vector2 closest = a + ab * t;
            float distSqr = (p - closest).sqrMagnitude;

            if (t >= segProgressT || distSqr <= segCloseSqr) { headIndex++; continue; }

            break;
        }

        // compact when headIndex grows
        if (headIndex > 64)
        {
            pathNodes.RemoveRange(0, headIndex);
            headIndex = 0;
        }
    }

    static float DistancePointToSegmentSqr(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float abLenSqr = ab.sqrMagnitude;
        if (abLenSqr <= Mathf.Epsilon) return (p - a).sqrMagnitude;
        float t = Vector2.Dot(p - a, ab) / abLenSqr;
        t = Mathf.Clamp01(t);
        Vector2 proj = a + ab * t;
        return (p - proj).sqrMagnitude;
    }

    void RefreshBufferImmediate()
    {
        if (IsEmpty) { line.positionCount = 0; return; }
        int remaining = pathNodes.Count - headIndex;
        EnsureBuffer(remaining);
        for (int i = 0; i < remaining; i++)
        {
            Vector3 wp = pathNodes[headIndex + i];
            buffer[i] = new Vector3(wp.x, minimapPlaneY + lift, wp.z);
        }
        line.positionCount = remaining;
        line.SetPositions(buffer);
    }

    public void Clear()
    {
        pathNodes.Clear();
        headIndex = 0;
        line.positionCount = 0;
    }

    public bool IsEmpty => pathNodes.Count - headIndex <= 0;
}
