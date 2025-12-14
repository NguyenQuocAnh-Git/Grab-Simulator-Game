using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource background;
    [SerializeField] private AudioSource getMoney;
    [SerializeField] private AudioSource die;
    public static SoundManager Instance;
    private void Awake()
    {
        if(Instance != null) return;

        Instance = this;
    }
    private void Start()
    {
        background.Play();
    }
    public void PlayGetMoney()
    {
        getMoney.Play();
    }
    public void PlayDie()
    {
        die.Play();
    }
}
