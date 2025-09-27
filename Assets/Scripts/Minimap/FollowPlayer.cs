using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] Transform player;

    private void LateUpdate()
    {
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y; // Keep the minimap's height constant
        transform.position = newPosition;

        // Not rotate the minimap with the player
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
