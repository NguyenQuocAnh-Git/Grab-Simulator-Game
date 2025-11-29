using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupTrigger : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerState playerState = other.GetComponent<PlayerState>();
            if(playerState != null && playerState.CurrentState == EPlayerState.GoingToPickUpFood)
            {
                Debug.Log("Player has picked up the food!");
                playerState.CurrentState = EPlayerState.CarryingFood;
                Destroy(this.gameObject);
            }
        }
    }
}
