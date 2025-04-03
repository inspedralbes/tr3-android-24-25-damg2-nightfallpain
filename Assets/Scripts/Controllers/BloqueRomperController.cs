using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloqueRomperController : MonoBehaviour
{
    // Puedes a�adir efectos de sonido o part�culas al destruir el bloque
    [SerializeField] private GameObject efectoDestruccion;
    [SerializeField] private AudioClip sonidoDestruccion;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si la colisi�n viene de un objeto con el tag "bullet"
        if (collision.gameObject.CompareTag("Bullet"))
        {
            DestruirBloque();

            // Tambi�n destruimos la bala
            Destroy(collision.gameObject);
        }
    }

    // Tambi�n podemos usar OnTriggerEnter2D si las balas usan trigger colliders
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Bullet"))
        {
            DestruirBloque();

            // Tambi�n destruimos la bala
            Destroy(collider.gameObject);
        }
    }

    private void DestruirBloque()
    {
        // Reproducir efectos si est�n asignados
        if (efectoDestruccion != null)
        {
            Instantiate(efectoDestruccion, transform.position, Quaternion.identity);
        }

        if (sonidoDestruccion != null)
        {
            AudioSource.PlayClipAtPoint(sonidoDestruccion, transform.position);
        }

        // Destruir este bloque
        Destroy(gameObject);
    }
}