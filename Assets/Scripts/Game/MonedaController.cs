using UnityEngine;

public class MonedaController : MonoBehaviour
{
    [SerializeField] private int experienciaOtorgada = 100; // Experiencia que da la moneda
    [SerializeField] private GameObject dinamicText; // Prefab del texto dinámico
    [SerializeField] private AudioClip pickupSound; // Sonido al recoger la moneda

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // Añadir experiencia (aunque ahora usaremos monedas, esto es solo si quieres aún sumar experiencia)
                player.AddExperience(experienciaOtorgada);

                // Actualizar el contador de monedas en ExperienceController
                ExperienceController experienceController = FindObjectOfType<ExperienceController>();
                if (experienceController != null)
                {
                    experienceController.MonedaRecogida();  // Aumentar el contador de monedas
                }

                Debug.Log("Has recogido una moneda. +" + experienciaOtorgada + " EXP");

                // Instanciar el texto dinámico encima de la moneda
                if (dinamicText != null)
                {
                    GameObject textInstance = Instantiate(dinamicText, transform.position + new Vector3(0, 1, -1), Quaternion.identity);
                    textInstance.GetComponent<DestroyAfterTime>().SetValue(experienciaOtorgada);
                }

                // Reproducir sonido de recogida si existe
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                // Destruir la moneda después de recogerla
                Destroy(gameObject);
            }
        }
    }
}