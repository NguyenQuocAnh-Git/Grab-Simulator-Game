using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
   public static NetworkManager instance;

   private void Awake()
   {
    if(instance == null)
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        ConnectToServer();
    }
    else {
        Destroy(gameObject);
    }
   }

   public void ConnectToServer()
   {
        Debug.Log("Connecting to server...");
        // TODO: Connect to server
   }
}
