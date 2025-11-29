using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawning : MonoBehaviour
{
    [SerializeField] private GameObject pickupFoodPrefab;
    [SerializeField] private GameObject dropFoodPrefab;
    [SerializeField] private PlayerState playerState;
    private void SpawnPickup(Vector3 pickUpPos)
    {
        GameObject.Instantiate(pickupFoodPrefab, pickUpPos, Quaternion.identity);
    }
    private void SpawnDrop(Vector3 dropPos)
    {
        GameObject.Instantiate(dropFoodPrefab, dropPos, Quaternion.identity);
    }
    private void Start()
    {
        playerState.OnStateChanged += ResetPlayerStateToNewSpawn;
        StartCoroutine(SpawnPickupAfterTime(5f)); // for the first time
    }
    private void ResetPlayerStateToNewSpawn(EPlayerState playerState)
    {
        if(playerState == EPlayerState.Available)
        {
            StartCoroutine(SpawnPickupAfterTime(5f));
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
            var dropPos = new Vector3(Random.Range(-20, 20), 1.1f, Random.Range(-20, 20));
            SpawnDrop(dropPos);
        }
        // khi nhận được thông báo là đã giao đồ ăn xong thì chuyển trạng thái về Available
        if (playerState == EPlayerState.DeliveredFood)
        {
            this.playerState.CurrentState = EPlayerState.Available;
            // can implement score or something here
        }
        
    }
    IEnumerator SpawnPickupAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        var pickUpPos = new Vector3(Random.Range(-20, 20), 1.1f, Random.Range(-20, 20));
        SpawnPickup(pickUpPos);
        playerState.CurrentState = EPlayerState.GoingToPickUpFood;
    }
}
