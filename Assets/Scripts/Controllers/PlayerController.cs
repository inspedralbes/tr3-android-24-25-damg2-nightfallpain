using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Range(1f, 10f)]
    public float speed = 5f;

    public Transform firePoint1;
    public Transform firePoint2;

    private bool useFirstGun = true;

    [SerializeField] private BarraVidaController barraVida;
    [SerializeField] private TextMeshProUGUI ammoText;

    // Sistema de munición
    public int maxAmmo = 10;
    private int currentAmmo;

    // Nueva variable para gestionar la experiencia
    [SerializeField] private int experiencia = 0;

    // Nueva variable para la vida máxima del jugador
    [SerializeField] private float vidaMaxima = 100f;

    // Animator para el control de animaciones
    private Animator animator;

    // Duración de la animación de muerte
    [SerializeField] private float deathAnimationDuration = 2f;

    // Escala de la muerte
    [SerializeField] private Vector3 deathScale = new Vector3(1f, 1f, 1f);

    // Para verificar si el jugador está muerto
    private bool isDead = false;

    // Variables para el username y el skin
    [SerializeField] private string username;
    [SerializeField] private string skinName;

    void Start()
    {
        // Cargar los valores desde PlayerPrefs si existen
        if (PlayerPrefs.HasKey("player_username"))
        {
            username = PlayerPrefs.GetString("player_username");
            skinName = PlayerPrefs.GetString("player_skin");
        }
        else
        {
            // Si no existen, asignar valores por defecto
            username = "Player";
            skinName = "DefaultSkin";
            SavePlayerPrefs(); // Guardar valores iniciales
        }

        // Cargar otros valores
        if (PlayerPrefs.HasKey("player_xp"))
            experiencia = PlayerPrefs.GetInt("player_xp");

        if (PlayerPrefs.HasKey("player_health"))
            vidaMaxima = PlayerPrefs.GetInt("player_health");

        if (PlayerPrefs.HasKey("player_speed"))
            speed = PlayerPrefs.GetFloat("player_speed");

        if (PlayerPrefs.HasKey("player_maxBullets"))
            maxAmmo = PlayerPrefs.GetInt("player_maxBullets");

        currentAmmo = maxAmmo;
        UpdateAmmoUI();

        // Configurar la vida máxima en la barra de vida
        if (barraVida != null)
        {
            barraVida.SetVidaMaxima(vidaMaxima);
        }

        // Obtener el componente Animator
        animator = GetComponent<Animator>();

        // Asignar el skin del jugador, puedes modificar esto para cambiar el skin visualmente
        ApplySkin();
    }
    // En PlayerController
    public void AumentarVidaMaxima(float cantidad)
    {
        vidaMaxima += cantidad;

        // Actualizar la barra de vida si es necesario
        if (barraVida != null)
        {
            barraVida.SetVidaMaxima(vidaMaxima);
        }
    }

    void Update()
    {
        // No procesar nada si el jugador está muerto o el juego está pausado
        if (isDead || IsGamePaused()) return;

        Move();
        RotateTowardsMouse();
        Shoot();
    }

    // Método para verificar si el juego está pausado
    private bool IsGamePaused()
    {
        return Time.timeScale == 0;
    }

    void Move()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(moveX, moveY, 0).normalized;
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo.ToString();
        }
    }

    public int GetExperience()
    {
        return experiencia;
    }
    public float GetVidaMaxima()
    {
        return vidaMaxima;
    }


    public void GastarExperiencia(int cantidad)
    {
        if (experiencia >= cantidad)
        {
            experiencia -= cantidad;
            SavePlayerPrefs(); // Guardar la experiencia actualizada
        }
    }
    public void IncreaseMaxHealth(float amount)
    {
        vidaMaxima += amount;
        barraVida.SetVidaMaxima(vidaMaxima); // Actualizar la barra de vida
        SavePlayerPrefs(); // Guardar en PlayerPrefs
    }

    public void IncreaseMaxAmmo(int amount)
    {
        maxAmmo += amount;
        currentAmmo = maxAmmo; // Recargar munición al máximo
        UpdateAmmoUI(); // Actualizar la UI de munición
        SavePlayerPrefs(); // Guardar en PlayerPrefs
    }


    void RotateTowardsMouse()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z;

        Vector3 direction = (mousePosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Shoot()
    {
        if (Input.GetMouseButtonDown(0) && currentAmmo > 0)
        {
            Transform firePoint = useFirstGun ? firePoint1 : firePoint2;

            // Obtener la bala del pool
            GameObject bullet = BulletPool.Instance.GetBullet(firePoint.position, Quaternion.identity, true, "bullet");

            if (bullet != null)
            {
                bullet.transform.position = firePoint.position;
                bullet.transform.rotation = transform.rotation;

                // Establecer la dirección de la bala
                Vector3 shootDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - firePoint.position).normalized;
                shootDirection.z = 0; // Asegurarse de que la dirección está en el plano 2D
                bullet.GetComponent<BulletController>().SetDirection(shootDirection);

                currentAmmo--;
                UpdateAmmoUI();
            }

            useFirstGun = !useFirstGun;
        }
        else if (currentAmmo <= 0)
        {
            Debug.Log("¡Sin munición! Encuentra munición para recargar.");
        }
    }

    public void RestaurarVida(int cantidad)
    {
        if (barraVida != null)
        {
            float nuevaVida = barraVida.GetVidaActual() + cantidad;
            nuevaVida = Mathf.Clamp(nuevaVida, 0, vidaMaxima); // Limitar la vida al máximo
            barraVida.RecibirDanio(-(nuevaVida - barraVida.GetVidaActual())); // Curar
        }
    }

    public void RestaurarVidaCompleta()
    {
        if (barraVida != null)
        {
            barraVida.RestaurarVidaTotal();
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return; // Si ya está muerto, no recibe más daño

        if (barraVida != null)
        {
            barraVida.RecibirDanio(damage);

            // 🔥 Verificar si la vida llegó a 0 para llamar a Die()
            if (barraVida.GetVidaActual() <= 0)
            {
                Die();
            }
        }
    }

    void Die()
    {
        if (isDead) return; // Si ya está muerto, no hacer nada

        isDead = true; // Marcar como muerto para evitar ejecuciones adicionales

        // Activar animación de muerte
        animator.SetTrigger("dead");

        // Cambiar la escala del jugador muerto para que se vea más grande o pequeño
        transform.localScale = deathScale;

        // Desactivar colisión y otros componentes para que el jugador no interfiera después de morir
        GetComponent<Collider2D>().enabled = false;

        // Desactivar el script para evitar futuras ejecuciones
        this.enabled = false;

        // Llamar a una corutina para esperar la duración de la animación antes de destruir al jugador
        StartCoroutine(WaitForDeathAnimation());
    }

    // Corutina para esperar la duración de la animación de muerte
    private IEnumerator WaitForDeathAnimation()
    {
        // Esperar el tiempo de duración de la animación de muerte
        yield return new WaitForSeconds(deathAnimationDuration);

        // Después de la animación, destruimos al jugador
        Destroy(gameObject);
    }

    public void ReloadAmmo()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
        Debug.Log("¡Munición recargada!");
    }

    // Actualizar experiencia y guardar en PlayerPrefs
    public void AddExperience(int amount)
    {
        experiencia += amount;
        SavePlayerPrefs(); // Guardar la experiencia cada vez que se agrega
        Debug.Log("Experiencia acumulada: " + experiencia);
    }

    public bool IsDead() // Método para comprobar si el jugador está muerto
    {
        return isDead;
    }

    // Guardar valores en PlayerPrefs (solo se guarda si se actualizan)
    public void SavePlayerPrefs()
    {
        PlayerPrefs.SetString("player_username", username); // Guardar el nombre de usuario
        PlayerPrefs.SetString("player_skin", skinName); // Guardar el nombre del skin
        PlayerPrefs.SetInt("player_xp", experiencia);
        PlayerPrefs.SetInt("player_health", Mathf.RoundToInt(vidaMaxima)); // Guardar la salud
        PlayerPrefs.SetFloat("player_speed", speed);
        PlayerPrefs.SetInt("player_maxBullets", maxAmmo);
        PlayerPrefs.Save(); // Asegurarse de guardar los cambios
    }

    // Método para aplicar el skin del jugador
    void ApplySkin()
    {
        // Aquí puedes agregar la lógica para aplicar el skin del jugador.
        // Por ejemplo, cambiar la textura, modelo o material basado en el skin.
        Debug.Log("Aplicando skin: " + skinName);
    }

    // Método para actualizar el username del jugador
    public void UpdateUsername(string newUsername)
    {
        username = newUsername;
        SavePlayerPrefs(); // Guardar el nuevo username en PlayerPrefs
    }
}
