using UnityEngine;
using UnityEngine.UI;

public class TimeControl : MonoBehaviour
{
    [Header("Configuración de Tiempo")]
    public float slowDownFactor = 0.1f;    // Cuánto se ralentiza el tiempo

    [Header("Configuración de Energía")]
    [SerializeField] private float maxEnergy = 100f;         // Energía máxima
    [SerializeField] private float energyDrainRate = 20f;    // Velocidad de consumo de energía por segundo
    [SerializeField] private float energyRechargeRate = 10f; // Velocidad de recarga de energía por segundo

    [Header("Configuración de Cooldown")]
    [SerializeField] private float cooldownDuration = 2f;    // Duración del cooldown cuando la energía llega a 0

    [Header("Referencias UI")]
    [SerializeField] private Image energyBar;                // Referencia a la imagen de la barra de energía

    // Variables privadas
    private float currentEnergy;
    private bool isSlowed = false;
    private bool isInCooldown = false;
    private float cooldownTimer = 0f;
    private float originalTimeScale;

    private void Start()
    {
        currentEnergy = maxEnergy;
        UpdateEnergyBar();
    }

    void Update()
    {
        if (isInCooldown)
        {
            HandleCooldown();
            return;
        }

        // Verificamos si la tecla Shift está presionada y tenemos energía
        if (Input.GetKey(KeyCode.LeftShift) && currentEnergy > 0)
        {
            SlowDownTime();
        }
        else
        {
            RestoreTime();
        }

        // Recargar energía cuando no estamos ralentizando el tiempo
        if (!isSlowed && currentEnergy < maxEnergy)
        {
            RechargeEnergy();
        }

        UpdateEnergyBar();
    }

    private void SlowDownTime()
    {
        // Si no está en modo ralentizado, cambiamos el tiempo
        if (!isSlowed)
        {
            originalTimeScale = Time.timeScale;
            Time.timeScale = slowDownFactor;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            isSlowed = true;
        }

        // Consumir energía
        currentEnergy -= energyDrainRate * Time.unscaledDeltaTime;

        // Verificar si se agotó la energía
        if (currentEnergy <= 0)
        {
            currentEnergy = 0;
            RestoreTime();
            ActivateCooldown();
        }
    }

    private void RestoreTime()
    {
        // Si la tecla no está presionada, restauramos el tiempo original
        if (isSlowed)
        {
            Time.timeScale = originalTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            isSlowed = false;
        }
    }

    private void RechargeEnergy()
    {
        currentEnergy += energyRechargeRate * Time.unscaledDeltaTime;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
    }

    private void HandleCooldown()
    {
        cooldownTimer -= Time.unscaledDeltaTime;

        if (cooldownTimer <= 0)
        {
            isInCooldown = false;
            cooldownTimer = 0f;
            currentEnergy = maxEnergy * 0.25f; // Recarga parcial después del cooldown
        }
    }

    private void ActivateCooldown()
    {
        isInCooldown = true;
        cooldownTimer = cooldownDuration;
    }

    private void UpdateEnergyBar()
    {
        if (energyBar != null)
        {
            energyBar.fillAmount = currentEnergy / maxEnergy;

            // Cambiar color en función del estado
            if (isInCooldown)
            {
                energyBar.color = Color.red;
            }
            else if (currentEnergy / maxEnergy < 0.3f)
            {
                energyBar.color = Color.yellow;
            }
            else
            {
                energyBar.color = Color.cyan;
            }
        }
    }

    // Métodos públicos para acceder a los valores
    public float GetCurrentEnergy()
    {
        return currentEnergy;
    }

    public float GetMaxEnergy()
    {
        return maxEnergy;
    }

    public bool IsInCooldown()
    {
        return isInCooldown;
    }

    public float GetCooldownRemaining()
    {
        return cooldownTimer;
    }
}