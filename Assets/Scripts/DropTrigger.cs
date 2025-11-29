using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerState playerState = other.GetComponent<PlayerState>();
            if(playerState != null && playerState.CurrentState == EPlayerState.CarryingFood)
            {
                Debug.Log("Player has dropped off the food!");
                playerState.CurrentState = EPlayerState.DeliveredFood;
                Destroy(this.gameObject);
            }
        }
    }
}
