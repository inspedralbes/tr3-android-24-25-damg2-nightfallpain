using UnityEngine;
using TMPro;

public class ExperienceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI experienciaText; // El texto de UI que se actualizará
    [SerializeField] private PlayerController playerController; // El controlador del jugador para obtener la experiencia

    void Update()
    {
        if (experienciaText != null && playerController != null)
        {
            // Actualiza el texto con la cantidad de experiencia del jugador
            experienciaText.text =  playerController.GetExperience().ToString();
        }
    }
}
