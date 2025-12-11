using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject panelGameOverUI;
    private RectTransform panelGameOverUIRect;
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnGameOver;
            GameManager.Instance.OnGameStateChanged += OnGameStart;
            GameManager.Instance.OnGameStateChanged += OnGamePlaying;
        }    
        panelGameOverUIRect = panelGameOverUI.GetComponent<RectTransform>();
    }

    private void OnGameStart(GameState gameState)
    {
        if (gameState != GameState.Lobby)
        {
            Debug.Log("OnGameStart not play");
            return;
        }
        Debug.Log("Game Start");
        mainMenuUI.SetActive(true);
    }
    
    private void OnGameOver(GameState gameState)
    {
        if (gameState != GameState.GameOver) return;
        
        panelGameOverUI.SetActive(true);
    }

    private void OnGamePlaying(GameState gameState)
    {
        if (gameState != GameState.GamePlaying) return;
        
        mainMenuUI.SetActive(false);
        panelGameOverUI.GetComponent<RectTransform>().anchoredPosition = panelGameOverUIRect.anchoredPosition;
        panelGameOverUI.SetActive(false);
    }
}