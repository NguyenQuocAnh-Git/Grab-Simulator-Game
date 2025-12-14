using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrentSpeedUI : MonoBehaviour
{
    [SerializeField] private TMP_Text currentSpeedTxt;
    [SerializeField] private BikeController controller;

    private void Start()
    {
        EventManager.Instance.OnBikeSpawn += OnBikeSpawn;
    }

    private void LateUpdate()
    {
        int currentSpeed = Mathf.RoundToInt(controller.currentSpeed);
        currentSpeedTxt.text = currentSpeed.ToString();
    }
    private void OnBikeSpawn(GameObject bike)
    {
        if(bike != null)
        {
            controller = bike.GetComponentInChildren<BikeController>();
        }
    }
}
