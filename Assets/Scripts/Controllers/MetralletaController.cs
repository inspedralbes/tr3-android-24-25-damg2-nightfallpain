using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetralletaController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Prefab de la bala que será disparada")]
    public GameObject bulletPrefab;

    [Tooltip("Puntos de origen para las balas")]
    public Transform[] bulletFirePoints = new Transform[2];

    [Header("Rotación")]
    [Tooltip("Velocidad de rotación de la torreta")]
    public float rotationSpeed = 5f;

    [Tooltip("Ángulo mínimo de rotación (grados)")]
    public float minAngle = -90f;

    [Tooltip("Ángulo máximo de rotación (grados)")]
    public float maxAngle = 90f;

    [Header("Disparo")]
    [Tooltip("Velocidad de las balas")]
    public float bulletSpeed = 10f;

    [Tooltip("Tiempo entre disparos")]
    public float fireRate = 0.5f;

    [Header("Detección")]
    [Tooltip("Rango máximo de detección del jugador")]
    public float detectionRange = 10f;

    [Header("Capas y Tags")]
    [Tooltip("Tag del jugador")]
    public string playerTag = "Player";

    // Variables privadas
    private Transform playerTransform;
    private float nextFireTime;
    private bool playerInRange = false;
    private float initialRotationZ;
    private Rigidbody2D rb;

    void Start()
    {
        // Obtener referencia al Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning("MetralletaController: No se encontró un Rigidbody2D. Algunas funciones podrían no trabajar correctamente.");
        }
        else if (rb.bodyType != RigidbodyType2D.Static)
        {
            Debug.LogWarning("MetralletaController: El Rigidbody2D no está configurado como Static. Para un funcionamiento óptimo, configúrelo como Static.");
        }

        // Guardar la rotación inicial
        initialRotationZ = transform.rotation.eulerAngles.z;

        // Buscar al jugador por su tag
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Verificar que tenemos los puntos de disparo asignados
        if (bulletFirePoints.Length < 2 || bulletFirePoints[0] == null || bulletFirePoints[1] == null)
        {
            Debug.LogError("MetralletaController: No se han asignado los bulletFirePoints correctamente.");
        }

        // Verificar que tenemos el prefab de la bala
        if (bulletPrefab == null)
        {
            Debug.LogError("MetralletaController: No se ha asignado el prefab de la bala.");
        }
    }

    void Update()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                return;
            }
        }

        // Calcular la distancia al jugador
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        playerInRange = distanceToPlayer <= detectionRange;

        if (playerInRange)
        {
            // Disparar si es tiempo
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    void RotateTowardsPlayer()
    {
        // Calcular dirección hacia el jugador
        Vector2 direction = playerTransform.position - transform.position;
        direction.Normalize();

        // Calcular el ángulo de rotación
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Ajustar el ángulo relativo a la rotación inicial
        float relativeAngle = Mathf.DeltaAngle(initialRotationZ, angle);

        // Restringir el ángulo dentro de los límites
        float clampedAngle = Mathf.Clamp(relativeAngle, minAngle, maxAngle);

        // Calcular el ángulo final
        float finalAngle = initialRotationZ + clampedAngle;

        // Crear rotación deseada
        Quaternion targetRotation = Quaternion.Euler(0, 0, finalAngle);

        // Aplicar rotación (con Rigidbody2D si está disponible)
        if (rb != null && rb.bodyType != RigidbodyType2D.Static)
        {
            // Si el rigidbody no es static, usamos MoveRotation para una rotación física correcta
            float step = rotationSpeed * Time.deltaTime;
            float newAngle = Mathf.LerpAngle(rb.rotation, finalAngle, step);
            rb.MoveRotation(newAngle);
        }
        else
        {
            // Si no hay rigidbody o es static, usamos la rotación de transform
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void Shoot()
{
    foreach (Transform firePoint in bulletFirePoints)
    {
        if (firePoint != null)
        {
            // Crear la bala
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            // Obtener Rigidbody2D de la bala
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

            if (bulletRb != null)
            {
                // Calcular la dirección hacia el jugador pero solo en la dirección X
                Vector2 direction = new Vector2(playerTransform.position.x - firePoint.position.x, 0).normalized;

                // Aplicar velocidad en la dirección horizontal (X)
                bulletRb.linearVelocity = direction * bulletSpeed;
            }
            else
            {
                Debug.LogWarning("La bala no tiene un componente Rigidbody2D");
            }

            // Destruir la bala después de un tiempo
            Destroy(bullet, 3f);
        }
    }
}


    // Método para visualizar el rango de detección y el arco de rotación en el editor
    void OnDrawGizmosSelected()
    {
        // Dibujar rango de detección
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Dibujar arco de rotación
        Gizmos.color = Color.yellow;
        float currentZ = transform.rotation.eulerAngles.z;
        Vector3 minDirection = Quaternion.Euler(0, 0, currentZ + minAngle) * Vector3.right;
        Vector3 maxDirection = Quaternion.Euler(0, 0, currentZ + maxAngle) * Vector3.right;

        Gizmos.DrawLine(transform.position, transform.position + minDirection * 2);
        Gizmos.DrawLine(transform.position, transform.position + maxDirection * 2);
    }
}