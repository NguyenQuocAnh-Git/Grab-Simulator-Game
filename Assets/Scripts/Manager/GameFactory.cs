using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFactory : MonoBehaviour
{
    public static GameFactory Instance;

    void Awake()
    {
        if (Instance != null)
        {
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    [Header("Bike properties")]
    [SerializeField] GameObject bikePrefab;
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private GameObject miniMap;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform playerTransform;

    public GameObject SpawnBike(Vector3 pos, Quaternion rot)
    {
        var bike = Instantiate(bikePrefab, pos, rot);
        var ctrl = bike.GetComponent<BikeInteraction>();

        ctrl.Setup(interactionUI, miniMap, playerController, playerTransform);
        Debug.Log("Before spawn 2");
        EventManager.Instance.BikeSpawned(bike);
        return bike;
    }
}
