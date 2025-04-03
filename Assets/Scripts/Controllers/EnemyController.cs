using UnityEngine;
using System.Collections;

using GameData;

public class EnemyController : MonoBehaviour
{
    public float speed = 3.5f;  // Velocidad del enemigo
    public float shootingRange = 8f;
    public float fireRate = 1f;

    public Transform firePoint1;
    public Transform firePoint2;

    private Transform player;
    private bool useFirstGun = true;
    private float nextFireTime = 0f;

    [SerializeField] private int health = 30;  // Salud del enemigo
    [SerializeField] private GameObject dinamicText;

    [SerializeField] private int experienciaAlMorir = 300;

    [SerializeField] private float deathAnimationDuration = 2f;  // Duración de la animación de muerte
    [SerializeField] private Vector3 deathScale = new Vector3(1f, 1f, 1f);  // Escala del enemigo muerto

    private Animator animator;
    private bool isDead = false;

    private PlayerController playerController;
    private string bulletName;
    private int bulletDamage;  // Almacena el daño de la bala

    // Nuevo parámetro que indica si este enemigo es un Uzi
    public bool isUziEnemy = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();  // Obtenemos el componente Animator
        playerController = player.GetComponent<PlayerController>(); // Obtenemos la referencia al PlayerController

        InitializeEnemyData(); // Inicializa los datos del enemigo
    }

    void Update()
    {
        if (player != null && !isDead && !playerController.IsDead())
        {
            MoveTowardsPlayer();
            RotateTowardsPlayer();
            TryToShoot();
        }
    }

    // Método para inicializar los datos del enemigo (salud, velocidad, tipo de bala, daño)
    public void InitializeEnemyData()
    {
        EnemyDataInfo enemyData = DataManager.enemiesData.Find(e => e.name == gameObject.name);

        if (enemyData != null)
        {
            health = enemyData.health;
            speed = enemyData.speed;
            bulletName = enemyData.bulletName;  // Obtener el nombre de la bala del enemigo
            bulletDamage = enemyData.damage;  // Obtener el daño de la bala
        }
        else
        {
            Debug.LogError($"¡No se encontró datos para el enemigo: {gameObject.name}!");
        }
    }

    // Movimiento hacia el jugador
    void MoveTowardsPlayer()
    {
        if (isDead) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > shootingRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
    }

    // Rotación hacia el jugador
    void RotateTowardsPlayer()
    {
        if (isDead) return;

        Vector3 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Intentar disparar al jugador
    void TryToShoot()
    {
        if (isDead) return;

        if (Time.time >= nextFireTime)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= shootingRange)
            {
                Transform firePoint = useFirstGun ? firePoint1 : firePoint2;

                // Aquí se verifica el tipo de bala según el nombre (bulletName)
                GameObject bullet = null;

                if (bulletName == "bulletEnemy")
                {
                    // Obtener una bala normal del enemigo
                    bullet = BulletPool.Instance.GetEnemyBullet(firePoint.position, firePoint.rotation, bulletName);
                }
                else if (bulletName == "bulletEnemyUzi")
                {
                    // Obtener una bala de Uzi
                    bullet = BulletPool.Instance.GetEnemyUziBullet(firePoint.position, firePoint.rotation, bulletName);
                }

                if (bullet != null)
                {
                    bullet.transform.position = firePoint.position;
                    bullet.transform.rotation = firePoint.rotation;

                    // Asignamos la dirección de la bala
                    bullet.GetComponent<BulletController>().SetDirection((player.position - firePoint.position).normalized);

                    // Asignamos el daño de la bala
                    bullet.GetComponent<BulletController>().SetDamage(bulletDamage);

                    useFirstGun = !useFirstGun;
                    nextFireTime = Time.time + 1f / fireRate;
                }
            }
        }
    }

    // Recibir daño
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    // Matar al enemigo
    // Matar al enemigo
    void Die()
    {
        if (isDead) return; // Si ya está muerto, no hacer nada

        isDead = true; // Marcar al enemigo como muerto

        // Asegurarse de que haya un jugador para otorgar la experiencia
        if (player != null)
        {
            player.GetComponent<PlayerController>().AddExperience(experienciaAlMorir);  // Otorgar experiencia al jugador

            // Instanciar el texto dinámico encima del enemigo muerto
            GameObject textInstance = Instantiate(dinamicText, transform.position + new Vector3(0, 1, -1), Quaternion.identity);
            textInstance.GetComponent<DestroyAfterTime>().SetValue(experienciaAlMorir);
        }

        // Actualizar el contador de enemigos muertos en ExperienceController
        ExperienceController experienceController = FindObjectOfType<ExperienceController>();
        if (experienceController != null)
        {
            experienceController.EnemigoMuerto();  // Aumentar el contador de enemigos muertos
        }

        // Activar animación de muerte
        animator.SetBool("dead", true);

        // Cambiar la escala del enemigo para mostrar la muerte (opcional)
        transform.localScale = deathScale;

        // Desactivar la colisión para evitar más interacciones
        GetComponent<Collider2D>().enabled = false;

        // Desactivar el script para evitar más ejecuciones
        this.enabled = false;

        // Llamar a la corutina para esperar la duración de la animación de muerte antes de destruir el objeto
        StartCoroutine(WaitForDeathAnimation());
    }

    // Esperar la animación de muerte antes de destruir el enemigo
    private IEnumerator WaitForDeathAnimation()
    {
        yield return new WaitForSeconds(deathAnimationDuration);
        Destroy(gameObject);
    }
}
