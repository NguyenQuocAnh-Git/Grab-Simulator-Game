using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FuelUI : MonoBehaviour
{
    // Singleton instance
    public static FuelUI Instance { get; private set; }

    [Header("Fuel UI")]
    public Image fuelImage;
    public TMP_Text lowFuelText;
    public Button refuelButton;

    [Header("Economy")]
    public PlayerCoin playerCoin;

    private void Awake()
    {
        // Singleton protection
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Safety check – giúp debug rất nhanh
        if (fuelImage == null)
            Debug.LogError("FuelUI: fuelImage is not assigned");

        if (lowFuelText == null)
            Debug.LogError("FuelUI: lowFuelText is not assigned");

        if (refuelButton == null)
            Debug.LogError("FuelUI: refuelButton is not assigned");

        if (playerCoin == null)
            Debug.LogError("FuelUI: playerCoin is not assigned");
    }
}
