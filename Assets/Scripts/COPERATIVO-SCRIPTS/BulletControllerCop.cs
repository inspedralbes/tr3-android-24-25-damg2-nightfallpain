using UnityEngine;
using Mirror;

public class BulletControllerCop : NetworkBehaviour
{
    public float speed = 10f;
    public int damage = 34;  // Cambiado a 34 para que muera con 3 disparos (33.33... por disparo)
    public LayerMask murosLayer; // Capa para detectar los muros

    // Indica si la bala pertenece al jugador o al enemigo
    [HideInInspector]
    public bool isPlayerBullet = false;

    // Identificador del jugador que disparó esta bala
    [SyncVar]
    private uint shooterNetId;

    private Vector3 direction;  // Almacenará la dirección de la bala

    void Update()
    {
        // Si no hay dirección asignada, no moveremos la bala
        if (direction != Vector3.zero)
        {
            transform.position += direction * speed * Time.deltaTime;
        }
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

    // Método para establecer quién disparó esta bala
    public void SetShooter(uint netId)
    {
        shooterNetId = netId;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Si no somos el servidor, no procesamos las colisiones
        if (!isServer)
            return;

        // Si es una bala del jugador, puede dañar a enemigos y a otros jugadores
        if (isPlayerBullet)
        {
            if (collision.CompareTag("Enemy"))
            {
                // Genera partículas de impacto para enemigos
                BulletPoolCop.Instance.SpawnImpactParticles(transform.position, "Enemy");
                // Aplica el daño al enemigo
                collision.GetComponent<EnemyController>().TakeDamage(damage);
                // Devuelve la bala al pool
                BulletPoolCop.Instance.ReturnBullet(gameObject, isPlayerBullet);
            }
            else if (collision.CompareTag("Player"))
            {
                // Obtener el NetworkIdentity del jugador impactado
                NetworkIdentity hitPlayerIdentity = collision.GetComponent<NetworkIdentity>();

                // Solo dañamos si no es el mismo jugador que disparó
                if (hitPlayerIdentity != null && hitPlayerIdentity.netId != shooterNetId)
                {
                    // Genera partículas de impacto para el jugador
                    BulletPoolCop.Instance.SpawnImpactParticles(transform.position, "Player");

                    // Aplica el daño al jugador - MODIFICADO para pasar el ID del disparador
                    PlayerControllerCop playerHit = collision.GetComponent<PlayerControllerCop>();
                    if (playerHit != null)
                    {
                        playerHit.TakeDamage(damage, shooterNetId);
                    }

                    // Devuelve la bala al pool
                    BulletPoolCop.Instance.ReturnBullet(gameObject, isPlayerBullet);
                }
            }
        }
        // Si es una bala del enemigo, solo daña al jugador
        else if (!isPlayerBullet && collision.CompareTag("Player"))
        {
            // Genera partículas de impacto para el jugador
            BulletPoolCop.Instance.SpawnImpactParticles(transform.position, "Player");

            // Aplica el daño al jugador - MODIFICADO para pasar un ID de disparador 0 (enemigo)
            PlayerControllerCop playerHit = collision.GetComponent<PlayerControllerCop>();
            if (playerHit != null)
            {
                playerHit.TakeDamage(damage, 0); // 0 indica que fue un enemigo, no un jugador
            }

            // Devuelve la bala al pool
            BulletPoolCop.Instance.ReturnBullet(gameObject, isPlayerBullet);
        }

        // Las paredes detienen cualquier tipo de bala
        if (((1 << collision.gameObject.layer) & murosLayer) != 0)
        {
            // Genera partículas de impacto para los muros
            BulletPoolCop.Instance.SpawnImpactParticles(transform.position, "Wall");
            // Devuelve la bala al pool
            BulletPoolCop.Instance.ReturnBullet(gameObject, isPlayerBullet);
        }
    }
}