using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Mirror;
using UnityEngine.SceneManagement;

public class PlayerControllerCop : NetworkBehaviour
{
    [Range(1f, 10f)]
    public float speed = 5f;

    public Transform firePoint1;
    public Transform firePoint2;

    private bool useFirstGun = true;

    [SerializeField] private BarraVidaController barraVida;
    [SerializeField] private TextMeshProUGUI ammoText;

    public int maxAmmo = 10;
    [SyncVar]
    private int currentAmmo;

    [SyncVar]
    [SerializeField] private int experiencia = 0;

    [SyncVar]
    [SerializeField] private float vidaMaxima = 100f;

    private Animator animator;

    [SerializeField] private Vector3 deathScale = new Vector3(1f, 1f, 1f);

    [SyncVar]
    private bool isDead = false;

    [SyncVar]
    private int kills = 0;

    [SyncVar]
    [SerializeField] private string username;
    [SyncVar]
    [SerializeField] private string skinName;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if (PlayerPrefs.HasKey("player_username"))
        {
            username = PlayerPrefs.GetString("player_username");
            skinName = PlayerPrefs.GetString("player_skin");
        }
        else
        {
            username = "Player";
            skinName = "DefaultSkin";
            SavePlayerPrefs();
        }

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

        if (barraVida != null)
        {
            barraVida.SetVidaMaxima(vidaMaxima);
        }

        animator = GetComponent<Animator>();

        ApplySkin();
    }

    [Command]
    public void CmdAumentarVidaMaxima(float cantidad)
    {
        vidaMaxima += cantidad;
        RpcUpdateVidaMaxima(vidaMaxima);
    }

    [ClientRpc]
    void RpcUpdateVidaMaxima(float newVidaMaxima)
    {
        vidaMaxima = newVidaMaxima;
        if (barraVida != null)
        {
            barraVida.SetVidaMaxima(vidaMaxima);
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (isDead || IsGamePaused()) return;

        Move();
        RotateTowardsMouse();
        Shoot();
    }

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
        if (ammoText != null && isLocalPlayer)
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

    [Command]
    public void CmdGastarExperiencia(int cantidad)
    {
        if (experiencia >= cantidad)
        {
            experiencia -= cantidad;
            RpcUpdateExperience(experiencia);
        }
    }

    [Command]
    public void CmdIncreaseMaxHealth(float amount)
    {
        vidaMaxima += amount;
        RpcUpdateVidaMaxima(vidaMaxima);
    }

    [Command]
    public void CmdIncreaseMaxAmmo(int amount)
    {
        maxAmmo += amount;
        currentAmmo = maxAmmo;
        RpcUpdateAmmo(currentAmmo, maxAmmo);
    }

    [ClientRpc]
    void RpcUpdateAmmo(int newCurrentAmmo, int newMaxAmmo)
    {
        currentAmmo = newCurrentAmmo;
        maxAmmo = newMaxAmmo;
        UpdateAmmoUI();
    }

    [ClientRpc]
    void RpcUpdateExperience(int newExperience)
    {
        experiencia = newExperience;
        if (isLocalPlayer)
        {
            SavePlayerPrefs();
        }
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
            CmdShoot();
        }
        else if (currentAmmo <= 0 && isLocalPlayer)
        {
            Debug.Log("¡Sin munición! Encuentra munición para recargar.");
        }
    }

    [Command]
    void CmdShoot()
    {
        if (currentAmmo <= 0) return;

        Transform firePoint = useFirstGun ? firePoint1 : firePoint2;

        Vector3 shootDirection = transform.right;

        if (BulletPoolCop.Instance == null)
        {
            Debug.LogError("BulletPoolCop.Instance es nulo en CmdShoot");
            return;
        }

        GameObject bullet = BulletPoolCop.Instance.GetBullet(firePoint.position, transform.rotation, true, "bullet");

        if (bullet != null)
        {
            BulletControllerCop bulletController = bullet.GetComponent<BulletControllerCop>();
            if (bulletController != null)
            {
                bulletController.SetDirection(shootDirection);
                bulletController.SetShooter(netId); // Pasamos netId del tirador
            }

            currentAmmo--;
            RpcUpdateCurrentAmmo(currentAmmo);
        }
        else
        {
            Debug.LogError("No se pudo obtener una bala del pool");
        }

        useFirstGun = !useFirstGun;
    }

    [ClientRpc]
    void RpcUpdateCurrentAmmo(int newAmmo)
    {
        currentAmmo = newAmmo;
        UpdateAmmoUI();
    }

    [Command]
    public void CmdRestaurarVida(int cantidad)
    {
        if (barraVida != null)
        {
            float nuevaVida = barraVida.GetVidaActual() + cantidad;
            nuevaVida = Mathf.Clamp(nuevaVida, 0, vidaMaxima);
            RpcRestaurarVida(nuevaVida - barraVida.GetVidaActual());
        }
    }

    [ClientRpc]
    void RpcRestaurarVida(float cantidad)
    {
        if (barraVida != null)
        {
            barraVida.RecibirDanio(-cantidad);
        }
    }

    [Command]
    public void CmdRestaurarVidaCompleta()
    {
        RpcRestaurarVidaCompleta();
    }

    [ClientRpc]
    void RpcRestaurarVidaCompleta()
    {
        if (barraVida != null)
        {
            barraVida.RestaurarVidaTotal();
        }
    }

    [SyncVar]
    private int hitCount = 0;

    // Modificamos este método para que pase el ID del disparador
    public void TakeDamage(int damage, uint shooterId)
    {
        if (!isServer)
        {
            CmdTakeDamage(damage, shooterId);
            return;
        }

        if (isDead) return;

        hitCount++;

        RpcTakeDamage(damage);

        if (hitCount >= 3)
        {
            CmdDie(shooterId);
        }
    }

    [Command]
    void CmdTakeDamage(int damage, uint shooterId)
    {
        if (isDead) return;

        hitCount++;

        RpcTakeDamage(damage);

        if (hitCount >= 3)
        {
            CmdDie(shooterId);
        }
    }

    [ClientRpc]
    void RpcTakeDamage(int damage)
    {
        if (isDead) return;

        if (barraVida != null)
        {
            barraVida.RecibirDanio(damage);
        }
    }

    [Command]
    void CmdDie(uint killerId)
    {
        if (isDead) return;

        isDead = true;
        RpcDie();
        hitCount = 0;
        RpcAnnouncePlayerDeath(username);

        // Buscar al asesino por su netId
        foreach (var player in FindObjectsOfType<PlayerControllerCop>())
        {
            if (player.netId == killerId)
            {
                player.AddKill();
                player.RpcRedirectToWinScene();
                break;
            }
        }
    }

    [ClientRpc]
    void RpcDie()
    {
        if (isLocalPlayer)
        {
            // Redirigir al jugador muerto a la escena de derrota
            SceneManager.LoadScene("copLose");
        }

        // Desactivar el objeto del jugador muerto
        gameObject.SetActive(false);
    }

    [ClientRpc]
    void RpcRedirectToWinScene()
    {
        if (isLocalPlayer)
        {
            // Redirigir al jugador que mató al otro a la escena de victoria
            SceneManager.LoadScene("copWin");
        }
    }

    [ClientRpc]
    void RpcAnnouncePlayerDeath(string playerName)
    {
        Debug.Log($"¡{playerName} ha sido eliminado!");
    }

    [Server]
    public void AddKill()
    {
        kills++;
        RpcUpdateKills(kills);
    }

    [ClientRpc]
    void RpcUpdateKills(int newKills)
    {
        kills = newKills;
    }

    public void ReloadAmmo()
    {
        if (isLocalPlayer)
        {
            CmdReloadAmmo();
        }
    }

    [Command]
    public void CmdReloadAmmo()
    {
        if (!isServer) return;

        currentAmmo = maxAmmo;
        RpcReloadAmmo();
    }

    [ClientRpc]
    void RpcReloadAmmo()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
        if (isLocalPlayer)
        {
            Debug.Log("¡Munición recargada!");
        }
    }

    [Command]
    public void CmdAddExperience(int amount)
    {
        experiencia += amount;
        RpcUpdateExperience(experiencia);
    }

    public bool IsDead()
    {
        return isDead;
    }

    public int GetKills()
    {
        return kills;
    }

    public void SavePlayerPrefs()
    {
        if (!isLocalPlayer) return;

        PlayerPrefs.SetString("player_username", username);
        PlayerPrefs.SetString("player_skin", skinName);
        PlayerPrefs.SetInt("player_xp", experiencia);
        PlayerPrefs.SetInt("player_health", Mathf.RoundToInt(vidaMaxima));
        PlayerPrefs.SetFloat("player_speed", speed);
        PlayerPrefs.SetInt("player_maxBullets", maxAmmo);
        PlayerPrefs.Save();
    }

    void ApplySkin()
    {
        Debug.Log("Aplicando skin: " + skinName);
    }

    [Command]
    public void CmdUpdateUsername(string newUsername)
    {
        username = newUsername;
        RpcUpdateUsername(newUsername);
    }

    [ClientRpc]
    void RpcUpdateUsername(string newUsername)
    {
        username = newUsername;
        if (isLocalPlayer)
            SavePlayerPrefs();
    }
}