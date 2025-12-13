using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawning : MonoBehaviour
{
    [SerializeField] private GameObject pickupFoodPrefab;
    [SerializeField] private GameObject dropFoodPrefab;
    [SerializeField] private PlayerState playerState;

    [SerializeField] private Transform[] pickupPoints;
    [SerializeField] private Transform[] dropPoints;
    
    private bool spawnForTheFirstTime = true;
    private void SpawnPickup(Vector3 pickUpPos)
    {
        GameObject go = Instantiate(pickupFoodPrefab, pickUpPos, Quaternion.identity);
        go.transform.position = pickUpPos + Vector3.up * 0.5f;
    }

    private void SpawnDrop(Vector3 dropPos)
    {
        GameObject.Instantiate(dropFoodPrefab, dropPos, Quaternion.identity);
    }
    private void Start()
    {
        playerState.OnStateChanged -= ResetPlayerStateToNewSpawn;
        playerState.OnStateChanged += ResetPlayerStateToNewSpawn;

        GameManager.Instance.OnGameStateChanged += OnGameOver;
        GameManager.Instance.OnGameStateChanged += OnGamePlay;
    }

    private void ResetPlayerStateToNewSpawn(EPlayerState playerState)
    {
        if (GameManager.Instance.GameState != GameState.GamePlaying)
        {
            return;
        }
        
        if (playerState == EPlayerState.Available)
        {
            SpawnPickupAfterTime(5f).Forget();
            // do nothing, wait for next spawn
            return;
        }

        if (playerState == EPlayerState.GoingToPickUpFood)
        {
            // do nothing, wait for player to pick up food
            return;
        }

        if (playerState == EPlayerState.CarryingFood)
        {
            var dropPos = RandomInRange(dropPoints);
            SpawnDrop(dropPos);
        }

        // khi nhận được thông báo là đã giao đồ ăn xong thì chuyển trạng thái về Available
        if (playerState == EPlayerState.DeliveredFood)
        {
            Debug.Log("Delivered food");
            this.playerState.CurrentState = EPlayerState.Available;
            // can implement score or something here
        }
    }

    private async UniTaskVoid SpawnPickupAfterTime(float time)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(time));
        // sau khi mà bắn task đi mà bị đổi game state thì không làm gì cả
        if (GameManager.Instance.GameState != GameState.GamePlaying)
        {
            return;
        }
        
        var pickUpPos = RandomInRange(pickupPoints);
        SpawnPickup(pickUpPos);
        Debug.Log("SpawnPickupAfterTime: " + time);
        playerState.CurrentState = EPlayerState.GoingToPickUpFood;
    }

    private void OnGameOver(GameState gameState)
    {
        if (gameState == GameState.GameOver)
        {
            var pickups = TagRegistry.Get("Pickup").ToArray();
            for (int i = 0; i < pickups.Length; i++)
            {
                var pickup = pickups[i];
                if (pickup != null) Destroy(pickup);
            }

            var drops = TagRegistry.Get("Drop").ToArray();
            for (int i = 0; i < drops.Length; i++)
            {
                var drop = drops[i];
                if (drop != null) Destroy(drop);
            }
        }
    }
    
    private void OnGamePlay(GameState gameState)
    {
        if (gameState == GameState.GamePlaying)
        {
            if (spawnForTheFirstTime)
            {
                SpawnPickupAfterTime(5).Forget();
                spawnForTheFirstTime = false;
            }
        }
    }

    private Vector3 RandomInRange(Transform[] transforms)
    {
        var index = Random.Range(0, transforms.Length);
        return transforms[index].position;
    }
}