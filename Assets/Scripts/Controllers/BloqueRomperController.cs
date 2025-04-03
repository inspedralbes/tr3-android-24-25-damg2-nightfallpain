using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloqueRomperController : MonoBehaviour
{
    // Puedes añadir efectos de sonido o partículas al destruir el bloque
    [SerializeField] private GameObject efectoDestruccion;
    [SerializeField] private AudioClip sonidoDestruccion;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si la colisión viene de un objeto con el tag "bullet"
        if (collision.gameObject.CompareTag("Bullet"))
        {
            DestruirBloque();

            // También destruimos la bala
            Destroy(collision.gameObject);
        }
    }

    // También podemos usar OnTriggerEnter2D si las balas usan trigger colliders
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Bullet"))
        {
            DestruirBloque();

            // También destruimos la bala
            Destroy(collider.gameObject);
        }
    }

    private void DestruirBloque()
    {
        // Reproducir efectos si están asignados
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