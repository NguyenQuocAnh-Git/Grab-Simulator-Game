using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class VehicleSpawner : MonoBehaviour
{
    [Header("Spawn immediately on Play")]
    public bool spawnOnStart = true;

    [Header("What to spawn")]
    [Tooltip("List of different vehicle prefabs — spawner will pick one random prefab per spawned vehicle.")]
    public List<GameObject> vehiclePrefabs = new List<GameObject>();

    [Tooltip("If empty, spawner will use all WaypointNode in scene.")]
    public List<WaypointNode> spawnNodes = new List<WaypointNode>();

    [Header("Quantity")]
    [Tooltip("Total number of vehicles to create once at start")]
    public int totalToSpawn = 10;
    [Tooltip("If true, each vehicle will choose a random spawn node from spawnNodes")]
    public bool pickRandomSpawnNodeEachVehicle = true;

    [Header("Spacing / safety")]
    [Tooltip("min distance from other vehicles to allow spawn")]
    public float minSpacing = 2f;
    public LayerMask vehicleLayerMask; // layer(s) used by vehicles
    [Tooltip("How many attempts to try finding a free place for each vehicle before giving up")]
    public int maxSpawnAttemptsPerVehicle = 8;

    [Header("Behavior for avoiding spawning on same node")]
    [Tooltip("Prefer to not spawn more than one vehicle exactly at same node position if there are enough free nodes")]
    public bool avoidSameNodeWhenPossible = true;

    [Header("Pooling (optional)")]
    public bool usePooling = false;
    public int poolSize = 20;

    // internal
    private List<GameObject> pool = new List<GameObject>();

    private void Start()
    {
        if (usePooling) WarmPool();
        if (spawnOnStart) SpawnAllNow();
    }

    public void SpawnAllNow()
    {
        if (vehiclePrefabs == null || vehiclePrefabs.Count == 0)
        {
            Debug.LogWarning("[VehicleSpawner] No vehiclePrefabs provided.");
            return;
        }

        if (spawnNodes == null || spawnNodes.Count == 0)
        {
            var all = FindObjectsOfType<WaypointNode>();
            if (all != null && all.Length > 0)
                spawnNodes = new List<WaypointNode>(all);
        }

        if (spawnNodes == null || spawnNodes.Count == 0)
        {
            Debug.LogWarning("[VehicleSpawner] No spawn nodes available.");
            return;
        }

        int spawnedCount = 0;
        int linearIndex = 0;

        // track occupied node positions to avoid spawning multiple on same node
        HashSet<WaypointNode> occupiedNodes = new HashSet<WaypointNode>();

        for (int i = 0; i < totalToSpawn; i++)
        {
            bool spawned = false;
            int attempts = 0;

            while (!spawned && attempts < maxSpawnAttemptsPerVehicle)
            {
                WaypointNode chosenNode;

                if (pickRandomSpawnNodeEachVehicle)
                {
                    chosenNode = spawnNodes[Random.Range(0, spawnNodes.Count)];
                }
                else
                {
                    chosenNode = spawnNodes[linearIndex % spawnNodes.Count];
                    linearIndex++;
                }

                if (chosenNode == null)
                {
                    attempts++;
                    continue;
                }

                // if we prefer avoid same node and node already used, try choose a different one
                if (avoidSameNodeWhenPossible && occupiedNodes.Contains(chosenNode))
                {
                    // try to find another free node quickly
                    bool foundAlt = false;
                    for (int k = 0; k < spawnNodes.Count; k++)
                    {
                        var alt = spawnNodes[(linearIndex + k) % spawnNodes.Count];
                        if (alt == null) continue;
                        if (!occupiedNodes.Contains(alt))
                        {
                            chosenNode = alt;
                            foundAlt = true;
                            break;
                        }
                    }
                    if (!foundAlt)
                    {
                        // no alternative free node — we'll allow reuse of nodes but still try offset spawn
                    }
                }

                Vector3 spawnPos = chosenNode.transform.position;

                // check spacing at exact node position
                Collider[] hits = Physics.OverlapSphere(spawnPos, minSpacing, vehicleLayerMask, QueryTriggerInteraction.Ignore);
                if (hits == null || hits.Length == 0)
                {
                    // good: spawn exactly on node
                    SpawnRandomPrefabAt(chosenNode, spawnPos);
                    spawned = true;
                    occupiedNodes.Add(chosenNode);
                    spawnedCount++;
                    break;
                }
                else
                {
                    // try small random offsets around node to place vehicle nearby (avoid exact overlap)
                    bool foundOffset = false;
                    int offsetTrials = 4;
                    for (int o = 0; o < offsetTrials; o++)
                    {
                        float r = Random.Range(minSpacing * 0.5f, minSpacing * 1.2f);
                        float ang = Random.Range(0f, Mathf.PI * 2f);
                        Vector3 offset = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * r;
                        Vector3 tryPos = spawnPos + offset;

                        Collider[] hits2 = Physics.OverlapSphere(tryPos, minSpacing, vehicleLayerMask, QueryTriggerInteraction.Ignore);
                        if (hits2 == null || hits2.Length == 0)
                        {
                            SpawnRandomPrefabAt(chosenNode, tryPos);
                            spawned = true;
                            occupiedNodes.Add(chosenNode); // still mark node as used (to avoid many vehicles around same node)
                            spawnedCount++;
                            foundOffset = true;
                            break;
                        }
                    }

                    if (foundOffset) break;

                    // else cannot spawn at this chosenNode; try another
                }

                attempts++;
            }

            if (!spawned)
            {
                Debug.LogWarning($"[VehicleSpawner] Could not spawn vehicle #{i + 1} after {maxSpawnAttemptsPerVehicle} attempts.");
                // do not block next vehicles: continue looping
            }
        }

        Debug.Log($"[VehicleSpawner] Spawned {spawnedCount}/{totalToSpawn} vehicles.");
    }

    void SpawnRandomPrefabAt(WaypointNode node, Vector3 pos)
    {
        // pick a random prefab from list
        GameObject prefab = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Count)];
        if (prefab == null)
        {
            Debug.LogWarning("[VehicleSpawner] Selected prefab is null.");
            return;
        }

        GameObject go = null;
        if (usePooling)
        {
            go = GetFromPoolOfType(prefab);
            if (go == null)
            {
                go = Instantiate(prefab, pos, Quaternion.identity);
            }
            else
            {
                go.transform.position = pos;
                go.SetActive(true);
            }
        }
        else
        {
            go = Instantiate(prefab, pos, Quaternion.identity);
        }

        // set rotation to node forward (yaw only)
        Vector3 f = node.transform.forward;
        f.y = 0f;
        if (f.sqrMagnitude > 0.0001f) go.transform.rotation = Quaternion.LookRotation(f.normalized, Vector3.up);

        // ensure spawned object is on vehicle layer for future spacing checks (optional)
        int vehicleLayerIndex = LayerMaskToLayerIndex(vehicleLayerMask);
        if (vehicleLayerIndex >= 0)
        {
            SetGameObjectLayerRecursive(go, vehicleLayerIndex);
        }

        var vc = go.GetComponent<VehicleController>();
        if (vc != null)
        {
            // pass the layer mask for controller sensing
            vc.vehicleLayerMask = vehicleLayerMask;
            vc.InitializeAtNode(node);
        }
        else
        {
            Debug.LogWarning("[VehicleSpawner] Spawned prefab does not contain VehicleController.", go);
        }
    }

    #region Pooling helpers (basic, per-prefab reuse)
    void WarmPool()
    {
        pool.Clear();
        for (int i = 0; i < poolSize; i++)
        {
            // create from first prefab as warm pool fallback
            if (vehiclePrefabs.Count == 0) break;
            var g = Instantiate(vehiclePrefabs[0], Vector3.zero, Quaternion.identity);
            g.SetActive(false);
            pool.Add(g);
        }
    }

    GameObject GetFromPoolOfType(GameObject prefab)
    {
        foreach (var g in pool)
        {
            if (!g.activeInHierarchy && g.name.StartsWith(prefab.name)) return g;
        }
        return null;
    }
    #endregion

    // utility: set layer recursively for object and children
    void SetGameObjectLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform t in obj.transform)
        {
            SetGameObjectLayerRecursive(t.gameObject, layer);
        }
    }

    int LayerMaskToLayerIndex(LayerMask mask)
    {
        int m = mask.value;
        if (m == 0) return -1;
        for (int i = 0; i < 32; i++)
        {
            if ((m & (1 << i)) != 0) return i;
        }
        return -1;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (spawnNodes != null)
        {
            foreach (var n in spawnNodes)
            {
                if (n == null) continue;
                Gizmos.DrawWireSphere(n.transform.position, minSpacing);
            }
        }
    }
}
