using UnityEngine;


public class HealthComponent : MonoBehaviour
{
    [SerializeField] private BarraVidaController barraVida;
    [SerializeField] private float maxHealth = 100f;

    private void Awake()
    {
        // Asegurarse de tener las referencias
        if (barraVida == null)
            barraVida = GetComponent<BarraVidaController>();
    }

    public float GetMaxHealth()
    {
        if (barraVida != null)
        {
            return barraVida.GetVidaMaxima();
        }
        return maxHealth;
    }

    public void IncreaseMaxHealth(float amount)
    {
        if (barraVida != null)
        {
            PlayerController playerController = GetComponent<PlayerController>();

            float vidaActual = barraVida.GetVidaActual();
            float vidaMaxima = barraVida.GetVidaMaxima();

            // Incrementar vida máxima
            float nuevaVidaMaxima = vidaMaxima + amount;
            barraVida.SetVidaMaxima(nuevaVidaMaxima);

            // Actualizar la vida máxima en PlayerController
            if (playerController != null)
            {
                playerController.AumentarVidaMaxima(amount);
            }

            // Restaurar vida proporcionalmente
            float cantidadRestauracion = amount;
            barraVida.RecibirDanio(-cantidadRestauracion);
        }
    }
}

