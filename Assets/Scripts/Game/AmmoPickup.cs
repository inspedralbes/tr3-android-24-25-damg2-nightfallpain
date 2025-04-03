using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Verifica si el jugador toca la munición
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            if (player != null)
            {
                player.ReloadAmmo(); // Recarga la munición
                Destroy(gameObject); // Destruye el objeto de munición después de recogerlo
            }
        }
    }
}
