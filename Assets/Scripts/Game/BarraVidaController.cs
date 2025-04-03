using UnityEngine;
using UnityEngine.UI;

public class BarraVidaController : MonoBehaviour
{
    public Image barraVida;
    public Image pantallaRoja; // Imagen que cubrirá toda la pantalla (pantalla roja)
    private float vidaMaxima;
    private float vidaActual;

    // Para el efecto de color suave
    private Color colorOriginal;

    private bool estaBajaVida = false;
    public float intensidad = 0.5f; // Intensidad de la opacidad

    // Método para inicializar la vida máxima desde el PlayerController
    public void SetVidaMaxima(float maxVida)
    {
        vidaMaxima = maxVida;
        vidaActual = maxVida; // Inicializamos con la vida máxima
        colorOriginal = barraVida.color; // Guardamos el color original de la barra de vida
        ActualizarBarraVida();
    }

    public void RecibirDanio(float danio)
    {
        vidaActual -= danio;
        vidaActual = Mathf.Clamp(vidaActual, 0, vidaMaxima); // Evita valores negativos o superiores al máximo
        ActualizarBarraVida();
    }

    void ActualizarBarraVida()
    {
        barraVida.fillAmount = vidaActual / vidaMaxima;

        // Si la vida es la mitad o menos, activamos la pantalla roja
        if (vidaActual <= vidaMaxima / 2)
        {
            if (!estaBajaVida)
            {
                estaBajaVida = true;
                pantallaRoja.color = new Color(1, 0, 0, intensidad); // Hacer visible con opacidad definida
            }
        }
        else
        {
            if (estaBajaVida)
            {
                estaBajaVida = false;
                pantallaRoja.color = new Color(1, 0, 0, 0); // Hacer invisible
            }
        }
    }

    public void RestaurarVidaTotal()
    {
        vidaActual = vidaMaxima; // Restaura la vida al máximo
        ActualizarBarraVida();
    }

    public float GetVidaActual()
    {
        return vidaActual;
    }

    public float GetVidaMaxima()
    {
        return vidaMaxima;
    }
}
