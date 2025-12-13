using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class TimeDeliveryUI : MonoBehaviour
{
    [SerializeField] TMP_Text timeToDeliveryTxt;
    [SerializeField] PlayerState playerState;
    private bool isSetTimeToDelivery = false;
    private int timeToDelivery = 0;
    private float timer = 0;
    private int snapTime = 0;
    private void Start()
    {
        playerState.OnStateChanged += OnPlayerHasOrder; 
        playerState.OnStateChanged += OnPlayerDelivered;  
    }
    private void Update()
    {
        if(isSetTimeToDelivery)
        {
            timer -= Time.deltaTime;
            if(timer <= 0)
            {
                isSetTimeToDelivery = false;
                timer = 0;
            }
            SetTimeToDelivery(Mathf.RoundToInt(timer));
        }
    }
    private void OnPlayerHasOrder(EPlayerState playerState)
    {
        if(playerState != EPlayerState.GoingToPickUpFood) return;

        timeToDelivery = 90;
        timer = timeToDelivery;
        isSetTimeToDelivery = true;
    }
    private void OnPlayerDelivered(EPlayerState playerState)
    {
        if(playerState != EPlayerState.DeliveredFood) return;
        isSetTimeToDelivery = false;
        snapTime = Mathf.RoundToInt(timer);
        timer = 0;
        timeToDeliveryTxt.text = "";
    }
    private void SetTimeToDelivery(int currentTime)
    {
        timeToDeliveryTxt.text = TimeConvert(currentTime);
        Color defaultColor, rustTimeColor;
        ColorUtility.TryParseHtmlString("#FFCF3D", out defaultColor);
        ColorUtility.TryParseHtmlString("#E21B1B", out rustTimeColor);
        
        if (currentTime < 15)
        {
            timeToDeliveryTxt.color =
                (currentTime & 1) == 0 ?  defaultColor :  rustTimeColor;
        }
        else
        {
            timeToDeliveryTxt.color = defaultColor;
        }
    }
    private string TimeConvert(int totalSeconds)
    {
        if (totalSeconds < 60)
            return totalSeconds.ToString();

        int m = totalSeconds / 60;
        int s = totalSeconds % 60;
        return $"{m}:{s:00}";
    }
    public int MaxTime()
    {
        return timeToDelivery;
    }
    public int SnapTime() => snapTime;
}
