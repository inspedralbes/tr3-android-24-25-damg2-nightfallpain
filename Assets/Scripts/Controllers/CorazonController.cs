using UnityEngine;

public class CorazonController : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();

            if (playerController != null)
            {
                playerController.RestaurarVidaCompleta(); // Cura al m�ximo
            }

            Destroy(gameObject); // Destruir el coraz�n despu�s de recogerlo
        }
    }
}
