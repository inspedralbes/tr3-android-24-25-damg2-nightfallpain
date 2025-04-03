using UnityEngine;
using UnityEngine.UI;

public class EfectoTocado : MonoBehaviour
{
    public Image pantallaRoja; // Referencia a la imagen de pantalla roja
    public float intensidad = 0.5f; // Intensidad de la opacidad (cu�nto de rojo se ve)

    void Start()
    {
        if (pantallaRoja != null)
        {
            pantallaRoja.color = new Color(1, 0, 0, 0); // Inicia invisible
        }
    }

    // M�todo para activar la pantalla roja sin parpadeo
    public void ActivarEfectoTocado()
    {
        if (pantallaRoja != null)
        {
            pantallaRoja.color = new Color(1, 0, 0, intensidad); // Hacer visible con opacidad definida
        }
    }

    // M�todo para desactivar la pantalla roja
    public void DesactivarEfectoTocado()
    {
        if (pantallaRoja != null)
        {
            pantallaRoja.color = new Color(1, 0, 0, 0); // Hacer invisible
        }
    }
}
