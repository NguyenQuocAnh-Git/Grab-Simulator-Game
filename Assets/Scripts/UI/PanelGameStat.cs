using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelGameStat : MonoBehaviour
{

    [SerializeField] private TMP_Text totalCoin;
    [SerializeField] private Image gasFill;

    [SerializeField] private PlayerCoin playerCoin;
    private void Start()
    {
        playerCoin.OnTotalCoinChanged += SetTotalCoin;
    }
    private void SetTotalCoin(int coin)
    {
        totalCoin.text = coin.ToString();
    }
    private async UniTaskVoid TotalCoinChange()
    {
        
    } 
}
