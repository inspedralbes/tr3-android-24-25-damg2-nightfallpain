using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarraEnergiaController : MonoBehaviour
{
    public Image barraEnergia;
    public TextMeshProUGUI textoEstado; // Opcional: para mostrar mensajes de estado

    private TimeControl timeController;

    private void Start()
    {
        // Localizar el TimeControl en la escena
        timeController = FindObjectOfType<TimeControl>();

        if (timeController == null)
        {
            Debug.LogError("No se encontr� el componente TimeControl en la escena");
        }

        if (textoEstado != null)
        {
            textoEstado.text = "";
        }
    }

    private void Update()
    {
        if (timeController != null)
        {
            // Actualizar la barra de energ�a
            barraEnergia.fillAmount = timeController.GetCurrentEnergy() / timeController.GetMaxEnergy();

            // Actualizar colores y mensajes seg�n el estado
            if (timeController.IsInCooldown())
            {
                barraEnergia.color = Color.red;
                if (textoEstado != null)
                {
                    textoEstado.text = "RECARGANDO... " + timeController.GetCooldownRemaining().ToString("F1") + "s";
                }
            }
            else if (timeController.GetCurrentEnergy() / timeController.GetMaxEnergy() < 0.3f)
            {
                barraEnergia.color = Color.yellow;
                if (textoEstado != null)
                {
                    textoEstado.text = "ENERG�A BAJA";
                }
            }
            else
            {
                barraEnergia.color = Color.cyan;
                if (textoEstado != null)
                {
                    textoEstado.text = "";
                }
            }
        }
    }
}