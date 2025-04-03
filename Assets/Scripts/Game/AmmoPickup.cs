using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Verifica si el jugador toca la munici�n
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            if (player != null)
            {
                player.ReloadAmmo(); // Recarga la munici�n
                Destroy(gameObject); // Destruye el objeto de munici�n despu�s de recogerlo
            }
        }
    }
}
