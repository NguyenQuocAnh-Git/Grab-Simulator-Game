using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class RouteController : MonoBehaviour
{
    [SerializeField] private PlayerState playerState;
    [SerializeField] private Transform moto;
    [SerializeField] private MiniMapPathfinder pathfinder;   // existing
    [SerializeField] private MiniMapPathDisplay display;     // existing ShowPath(List<Node>)
    [SerializeField] private float arriveThreshold = 1f;
    [SerializeField] private float minimapPlaneY = 1f;
    [SerializeField] private float recomputeDistanceThreshold = 2f; // khi moto lệch so với path quá nhiều
    [SerializeField] private float recomputeInterval = 1.0f; // tối thiểu 1s giữa 2 lần tính A*
    [SerializeField] private float nodeReachThreshold = 0.6f;
    private CancellationTokenSource cts;
    private Vector3 lastRecomputeTarget;
    private float lastRecomputeTime;
    // recompute tuning
    [SerializeField] private float maxDeviation = 3f;           // meters: if player deviates farther => recompute
    [SerializeField] private float forceRecomputeDistance = 2f; // if target moved > this -> recompute immediately

    private Transform target;
    private Vector3 lastTargetPos;
    void OnEnable()
    {
        playerState.OnStateChanged += OnStateChanged;
    }
    void OnDisable()
    {
        playerState.OnStateChanged -= OnStateChanged;
        cts?.Cancel();
    }
    void Update()
    {
        // each frame update line and check deviation
        bool needRecompute = false;
        float nearestSqr;
        needRecompute = display.UpdateAndDraw_CheckDeviation(moto.position, maxDeviation * maxDeviation, out nearestSqr);

        // also check if target moved significantly
        if (target != null && Vector3.SqrMagnitude(target.position - lastTargetPos) > forceRecomputeDistance * forceRecomputeDistance)
        {
            needRecompute = true;
            lastTargetPos = target.position;
        }

        // throttle recompute
        if (needRecompute && Time.unscaledTime - lastRecomputeTime >= recomputeInterval)
        {
            _ = StartPathTo(target.position);
        }
    }

    private void OnStateChanged(EPlayerState state)
    {
        switch (state)
        {
            case EPlayerState.GoingToPickUpFood:
                ComputePathToNearestTag("Pickup").Forget();
                break;
            case EPlayerState.CarryingFood:
                ComputePathToNearestTag("Drop").Forget();
                break;
            case EPlayerState.DeliveredFood:
            case EPlayerState.Available:
                ClearPath();
                break;
        }
    }
    public async UniTaskVoid StartPathTo(Vector3? worldTarget)
    {
        if (worldTarget  == null)
        {
            return;            
        }
        await UniTask.WaitForSeconds(1f); // slight delay to allow scene objects to initialize
        if (pathfinder == null || moto == null) return;
        cts?.Cancel();
        cts = new CancellationTokenSource();
        lastRecomputeTime = Time.unscaledTime;
        lastTargetPos = worldTarget.Value;

        List<Vector3> vpath = pathfinder.FindPathWorld(moto.position, worldTarget.Value);
        vpath[0] = new Vector3(moto.position.x, minimapPlaneY, moto.position.z);
        // back on main
        display.SetPath(vpath);
    }
    async UniTaskVoid ComputePathToNearestTag(string tag)
    {
        await UniTask.WaitForSeconds(1f); // slight delay to allow scene objects to initialize

        var target = FindNearestByTag(tag);
        if (target == null) { ClearPath(); return; }
        this.target = target;
        // run pathfinding off-main (assume pathfinder.FindPath uses no Unity API)
        List<Vector3> vpath = pathfinder.FindPathWorld(moto.position, target.transform.position);
        if(vpath == null || vpath.Count == 0)
        {
            ClearPath();
            Debug.Log("RouteController: no path found to tag " + tag);
            return;
        }
        vpath[0] = new Vector3(moto.position.x, minimapPlaneY, moto.position.z);
        // back on main
        display.SetPath(vpath);
    }

    Transform FindNearestByTag(string tag)
    {
        var objs = GameObject.FindGameObjectsWithTag(tag);
        if (objs == null || objs.Length == 0) return null;
        Transform best = null;
        float bestDist = float.MaxValue;
        for (int i = 0; i < objs.Length; i++)
        {
            float d = Vector3.SqrMagnitude(objs[i].transform.position - moto.position);
            if (d < bestDist) { bestDist = d; best = objs[i].transform; }
        }
        return best;
    }

    void ClearPath()
    {
        display.Clear();
    }
}
