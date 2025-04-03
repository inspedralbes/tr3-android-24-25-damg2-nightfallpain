using UnityEngine;
using Mirror;

public class AmmoPickupCopV2 : NetworkBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Verifica si el jugador toca la munici�n
        {
            PlayerControllerCop playerControllercop = other.GetComponent<PlayerControllerCop>();

            if (playerControllercop != null)
            {
                playerControllercop.CmdReloadAmmo(); // Enviar comando al servidor para recargar la munici�n
                Destroy(gameObject); // Destruye el objeto de munici�n despu�s de recogerlo
            }
        }
    }
}