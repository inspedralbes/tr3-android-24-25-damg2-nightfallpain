using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;  // Define el daño que hace la bala
    public LayerMask murosLayer; // Capa para detectar los muros

    // Indica si la bala pertenece al jugador o al enemigo
    [HideInInspector]
    public bool isPlayerBullet = false;

    private Vector3 direction;  // Almacenará la dirección de la bala
    void Update()
    {
        // Si no hay dirección asignada, no moveremos la bala
        if (direction != Vector3.zero)
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        // Eliminar la destrucción de la bala cuando sale de los límites
        // No hacemos nada aquí, las balas no se destruyen ni reposicionan
    }

    // Método para establecer la dirección de la bala
    public void SetDirection(Vector3 newDirection)
    {
        direction = newDirection.normalized;  // Normaliza la dirección para mantener la velocidad constante
    }

    // Método para establecer el daño de la bala
    public void SetDamage(int newDamage)
    {
        damage = newDamage;  // Asignamos el daño recibido
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Si es una bala del jugador, solo daña a los enemigos
        if (isPlayerBullet && collision.CompareTag("Enemy"))
        {
            // Genera partículas de impacto para enemigos
            BulletPool.Instance.SpawnImpactParticles(transform.position, "Enemy");

            // Aplica el daño al enemigo
            collision.GetComponent<EnemyController>().TakeDamage(damage);

            // Devuelve la bala al pool
            BulletPool.Instance.ReturnBullet(gameObject, isPlayerBullet);
        }
        // Si es una bala del enemigo, solo daña al jugador
        else if (!isPlayerBullet && collision.CompareTag("Player"))
        {
            // Genera partículas de impacto para el jugador
            BulletPool.Instance.SpawnImpactParticles(transform.position, "Player");

            // Aplica el daño al jugador
            collision.GetComponent<PlayerController>().TakeDamage(damage);

            // Devuelve la bala al pool
            BulletPool.Instance.ReturnBullet(gameObject, isPlayerBullet);
        }
        // Las paredes detienen cualquier tipo de bala
        else if (((1 << collision.gameObject.layer) & murosLayer) != 0)
        {
            // Genera partículas de impacto para los muros
            BulletPool.Instance.SpawnImpactParticles(transform.position, "Wall");

            // Devuelve la bala al pool
            BulletPool.Instance.ReturnBullet(gameObject, isPlayerBullet);
        }
    }
}
