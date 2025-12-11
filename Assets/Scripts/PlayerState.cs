using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerState : MonoBehaviour
{
    [SerializeField] private bool isCarryingFood = false;
    public bool IsCarryingFood => isCarryingFood;
    [SerializeField] private EPlayerState currentState = EPlayerState.Available;
    public EPlayerState CurrentState { get { return currentState; } set { currentState = value; OnStateChanged?.Invoke(currentState); } }

    public event Action<EPlayerState> OnStateChanged; // event to notify when state changes
}
public enum EPlayerState
{
    Available, // không có đơn hàng mới
    GoingToPickUpFood,     // đang tới chỗ lấy đơn hàng
    CarryingFood, // đang mang đơn hàng
    DeliveredFood // đã giao đơn hàng
}
