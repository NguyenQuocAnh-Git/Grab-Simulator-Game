using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FuelSystem : MonoBehaviour
{
    [Header("References")]
    public BikeController bike;
    public Image fuelImage;
    public TMP_Text lowFuelText;
    public Button refuelButton; // chỉ hiển thị UI

    [Header("Fuel Settings")]
    public float fuelCapacity = 100f;
    [SerializeField] private float fuelAmount;
    public float consumptionPerMeter = 0.5f;

    [Range(0f, 1f)]
    public float lowFuelPercent = 0.25f;

    [Header("Economy")]
    public PlayerCoin playerCoin;
    public float costPerFuelUnit = 0.1f;

    [Header("Station Interaction")]
    public string gasStationTag = "GasStation";
    private bool isInStation = false;

    // internal
    private bool isOutOfFuel = false;
    private Rigidbody sphereRB;
    private bool started = false;

    void Start()
    {
        if (bike == null)
        {
            Debug.LogError("FuelSystem: Bike missing!");
            enabled = false;
            return;
        }

        FuelUI ui = FuelUI.Instance;
        if (ui == null)
        {
            Debug.LogError("FuelSystem: FuelUI not found!");
            enabled = false;
            return;
        }

        fuelImage = ui.fuelImage;
        lowFuelText = ui.lowFuelText;
        refuelButton = ui.refuelButton;
        playerCoin = ui.playerCoin;

        fuelAmount = Mathf.Clamp(fuelAmount, 0f, fuelCapacity);
        sphereRB = bike.SphereRB;
        started = true;

        UpdateUIImmediate();

        if (lowFuelText) lowFuelText.gameObject.SetActive(false);
        if (refuelButton) refuelButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!started) return;

        // ===== Fuel Consumption =====
        if (!isOutOfFuel &&
            GameManager.Instance != null &&
            GameManager.Instance.IsGamePlaying())
        {
            float distance = bike.currentSpeed * Time.deltaTime;
            float fuelUsed = distance * consumptionPerMeter;

            if (fuelUsed > 0f)
            {
                fuelAmount -= fuelUsed;

                if (fuelAmount <= 0f)
                {
                    fuelAmount = 0f;
                    OnOutOfFuel();
                }

                UpdateUIImmediate();
            }
        }

        // ===== Low fuel warning =====
        if (lowFuelText)
        {
            float percent = fuelAmount / fuelCapacity;
            lowFuelText.gameObject.SetActive(percent <= lowFuelPercent && percent > 0f);
        }

        // ===== Refuel interaction =====
        if (isInStation && Input.GetKeyDown(KeyCode.F))
        {
                TryRefuelFull();
        }
    }

    private void OnOutOfFuel()
    {
        isOutOfFuel = true;

        if (sphereRB != null)
        {
            sphereRB.velocity = Vector3.zero;
            sphereRB.isKinematic = true;
        }

        if (bike.engineSound) bike.engineSound.Stop();
        if (bike.skidSound) bike.skidSound.Stop();

        // 🔥 GAME OVER KHI HẾT XĂNG
        GameManager.Instance.GameOver();

        Debug.Log("Hết xăng → Game Over");
    }

    private void ResumeAfterRefuel()
    {
        isOutOfFuel = false;

        if (sphereRB != null)
            sphereRB.isKinematic = false;

        if (bike.engineSound)
            bike.engineSound.Play();
    }

    public void TryRefuelFull()
    {
        float need = fuelCapacity - fuelAmount;
        if (need <= 0f) return;

        int cost = Mathf.CeilToInt(need * costPerFuelUnit);

        if (!playerCoin.CanSpendCoin(cost)) return;
        if (!playerCoin.SpendCoin(cost)) return;

        fuelAmount = fuelCapacity;
        UpdateUIImmediate();
        ResumeAfterRefuel();
    }

    private void UpdateUIImmediate()
    {
        if (fuelImage)
            fuelImage.fillAmount = Mathf.Clamp01(fuelAmount / fuelCapacity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(gasStationTag))
        {
            isInStation = true;
            if (refuelButton) refuelButton.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(gasStationTag))
        {
            isInStation = false;
            if (refuelButton) refuelButton.gameObject.SetActive(false);
        }
    }
}
