using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PedestrianSpawner : MonoBehaviour
{
    [Header("Spawn")]
    public bool spawnOnStart = true;
    [Tooltip("List of pedestrian prefabs (each must have PedestrianController + Animator)")]
    public List<GameObject> pedestrianPrefabs = new List<GameObject>();

    [Tooltip("If empty, spawner will use all PedestrianNode in scene.")]
    public List<PedestrianNode> spawnNodes = new List<PedestrianNode>();
    public int totalToSpawn = 10;
    public bool pickRandomSpawnNodeEachPed = true;

    [Header("Spacing")]
    public float minSpacing = 0.6f;
    public LayerMask pedestrianLayerMask; // layer used to check spawn overlap
    public int maxSpawnAttemptsPerPed = 8;
    public bool avoidSameNodeWhenPossible = true;

    private void Start()
    {
        if (spawnOnStart) SpawnAllNow();
    }

    public void SpawnAllNow()
    {
        if (pedestrianPrefabs == null || pedestrianPrefabs.Count == 0)
        {
            Debug.LogWarning("PedestrianSpawner: no pedestrianPrefabs assigned.");
            return;
        }

        if (spawnNodes == null || spawnNodes.Count == 0)
        {
            var all = FindObjectsOfType<PedestrianNode>();
            if (all != null && all.Length > 0) spawnNodes = new List<PedestrianNode>(all);
        }

        if (spawnNodes == null || spawnNodes.Count == 0)
        {
            Debug.LogWarning("PedestrianSpawner: no spawn nodes available.");
            return;
        }

        HashSet<PedestrianNode> occupied = new HashSet<PedestrianNode>();
        int linearIndex = 0;
        int spawned = 0;

        for (int i = 0; i < totalToSpawn; i++)
        {
            bool success = false;
            int attempts = 0;
            while (!success && attempts < maxSpawnAttemptsPerPed)
            {
                PedestrianNode node = pickRandomSpawnNodeEachPed ? spawnNodes[Random.Range(0, spawnNodes.Count)] : spawnNodes[linearIndex % spawnNodes.Count];
                linearIndex++;

                if (node == null) { attempts++; continue; }

                if (avoidSameNodeWhenPossible && occupied.Contains(node))
                {
                    // attempt to find another free node quickly
                    bool found = false;
                    for (int k = 0; k < spawnNodes.Count; k++)
                    {
                        var alt = spawnNodes[(linearIndex + k) % spawnNodes.Count];
                        if (alt == null) continue;
                        if (!occupied.Contains(alt)) { node = alt; found = true; break; }
                    }
                    if (!found) { /* allow reuse but try offsets */ }
                }

                Vector3 pos = node.transform.position;
                Collider[] hits = Physics.OverlapSphere(pos, minSpacing, pedestrianLayerMask, QueryTriggerInteraction.Ignore);
                if (hits == null || hits.Length == 0)
                {
                    SpawnRandomPrefabAt(node, pos);
                    occupied.Add(node);
                    success = true;
                    spawned++;
                    break;
                }
                else
                {
                    // try offset positions near node
                    bool foundOffset = false;
                    for (int o = 0; o < 4; o++)
                    {
                        float r = Random.Range(minSpacing * 0.4f, minSpacing * 1.0f);
                        float ang = Random.Range(0f, Mathf.PI * 2f);
                        Vector3 offset = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * r;
                        Vector3 tryPos = pos + offset;
                        Collider[] h2 = Physics.OverlapSphere(tryPos, minSpacing, pedestrianLayerMask, QueryTriggerInteraction.Ignore);
                        if (h2 == null || h2.Length == 0)
                        {
                            SpawnRandomPrefabAt(node, tryPos);
                            occupied.Add(node);
                            success = true; spawned++; foundOffset = true; break;
                        }
                    }
                    if (foundOffset) break;
                }

                attempts++;
            }

            if (!success) Debug.LogWarning($"PedestrianSpawner: could not spawn pedestrian #{i + 1} after {maxSpawnAttemptsPerPed} attempts.");
        }

        Debug.Log($"PedestrianSpawner: spawned {spawned}/{totalToSpawn} pedestrians.");
    }

    void SpawnRandomPrefabAt(PedestrianNode node, Vector3 pos)
    {
        GameObject prefab = pedestrianPrefabs[Random.Range(0, pedestrianPrefabs.Count)];
        if (prefab == null) { Debug.LogWarning("PedestrianSpawner: selected prefab is null."); return; }

        GameObject go = Instantiate(prefab, pos, Quaternion.identity);

        // random yaw for natural look
        go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        // set layer recursively to pedestrian layer so OverlapSphere works
        int layer = LayerMaskToLayerIndex(pedestrianLayerMask);
        if (layer >= 0) SetGameObjectLayerRecursive(go, layer);

        var pc = go.GetComponent<PedestrianController>();
        if (pc != null) pc.InitializeAtNode(node);

        // auto-assign Animator if not set
        if (pc != null && pc.animator == null)
        {
            var anim = go.GetComponentInChildren<Animator>();
            if (anim != null) pc.animator = anim;
        }
    }

    void SetGameObjectLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform t in obj.transform) SetGameObjectLayerRecursive(t.gameObject, layer);
    }

    int LayerMaskToLayerIndex(LayerMask mask)
    {
        int m = mask.value;
        if (m == 0) return -1;
        for (int i = 0; i < 32; i++) if ((m & (1 << i)) != 0) return i;
        return -1;
    }
}
