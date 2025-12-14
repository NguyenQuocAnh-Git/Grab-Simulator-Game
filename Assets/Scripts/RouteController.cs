using Cysharp.Threading.Tasks;
using UnityEngine;

public class RouteController : MonoBehaviour
{
    [SerializeField] Transform moto;
    [SerializeField] MiniMapPathfinder pathfinder;
    [SerializeField] MiniMapPathDisplay display;

    [Header("Timing")]
    [SerializeField] float deviationCheckInterval = 0.2f;
    [SerializeField] float recomputeInterval = 1.0f;

    [Header("Thresholds")]
    [SerializeField] float maxDeviation = 3f;
    [SerializeField] float targetMoveForce = 2f;
    [SerializeField] float forceRecomputeDistance = 2f;
    float lastDeviationCheck;
    float lastRecomputeTime;

    Transform target;
    Vector3 lastTargetPos;
    private void Start()
    {
        EventManager.Instance.OnBikeSpawn += OnBikeSpawned;
    }
    private void OnBikeSpawned(GameObject bike)
    {
        if(bike != null)
        {
            moto = bike.transform;
        }
    }
    void Update()
    {
        if (!GameManager.Instance.IsGamePlaying()) return;
        if (!display.HasPath()) return;

        if (Time.time - lastDeviationCheck < deviationCheckInterval)
            return;

        lastDeviationCheck = Time.time;

        // visual realtime
        display.UpdateHeadPosition(moto.position);

        bool needRecompute =
            display.NeedRecomputeByDeviation(moto.position, maxDeviation)
            || display.IsHeadDriftTooFar(moto.position, forceRecomputeDistance);

        // topology
        display.UpdateTrimAndRedraw(moto.position);

        // target moved
        if (target &&
            (target.position - lastTargetPos).sqrMagnitude >
            targetMoveForce * targetMoveForce)
        {
            needRecompute = true;
            lastTargetPos = target.position;
        }

        if (needRecompute &&
            Time.time - lastRecomputeTime >= recomputeInterval)
        {
            Recompute().Forget();
        }
    }

    async UniTaskVoid Recompute()
    {
        if(target == null)
        {
            display.Clear();
            return;
        }
        lastRecomputeTime = Time.time;

        // ✅ MAIN THREAD: snapshot data
        Vector3 start = moto.position;
        Vector3 end = target.position;

        // ✅ THREAD POOL: pure calculation
        await UniTask.SwitchToThreadPool();
        var path = pathfinder.FindPathWorld(start, end);

        // ✅ BACK TO MAIN THREAD
        await UniTask.SwitchToMainThread();

        if (path == null || path.Count == 0)
            display.Clear();
        else
            display.SetPath(path);
    }
    
    public void SetTarget(Transform t)
    {
        target = t;
        lastTargetPos = t.position;
        Recompute().Forget();
    }

    public void ClearPath()
    {
        display.Clear();
        target = null;
    }
}
