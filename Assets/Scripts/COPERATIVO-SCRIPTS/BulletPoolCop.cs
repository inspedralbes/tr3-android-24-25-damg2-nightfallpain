using System.Collections.Generic;
using UnityEngine;
using GameData;
using Mirror;

public class BulletPoolCop : NetworkBehaviour
{
    // Singleton que se mantiene sincronizado entre servidor y clientes
    public static BulletPoolCop Instance { get; private set; }

    public GameObject playerBulletPrefab;
    public GameObject enemyBulletPrefab;
    public GameObject enemyUziBulletPrefab;
    public int poolSize = 10;

    private Dictionary<string, Queue<GameObject>> bulletPools = new Dictionary<string, Queue<GameObject>>();

    public GameObject wallImpactParticles;
    public GameObject enemyImpactParticles;
    public GameObject playerImpactParticles;

    private bool isInitialized = false;

    public override void OnStartServer()
    {
        base.OnStartServer();
        InitializePools();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializePools()
    {
        if (isInitialized) return;

        // Inicializar los diccionarios
        bulletPools["player"] = new Queue<GameObject>();
        bulletPools["enemy"] = new Queue<GameObject>();
        bulletPools["enemyUzi"] = new Queue<GameObject>();

        // Llenar las pools
        FillPool(bulletPools["player"], playerBulletPrefab, true);
        FillPool(bulletPools["enemy"], enemyBulletPrefab, false);
        FillPool(bulletPools["enemyUzi"], enemyUziBulletPrefab, false);

        isInitialized = true;
        Debug.Log("BulletPoolCop initialized on server");
    }

    private void FillPool(Queue<GameObject> pool, GameObject prefab, bool isPlayerBullet)
    {
        if (!isServer) return;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(prefab);
            bullet.SetActive(false);

            BulletControllerCop bulletController = bullet.GetComponent<BulletControllerCop>();
            if (bulletController != null)
            {
                bulletController.isPlayerBullet = isPlayerBullet;
            }

            // No hacemos NetworkServer.Spawn aquí porque las balas inactivas no deben estar en la red todavía
            pool.Enqueue(bullet);
        }
    }

    public GameObject GetBullet(Vector3 position, Quaternion rotation, bool isPlayerBullet, string bulletName)
    {
        if (!isServer)
        {
            Debug.LogError("GetBullet debe ser llamado solo desde el servidor");
            return null;
        }

        if (!isInitialized)
        {
            InitializePools();
        }

        Queue<GameObject> pool;
        GameObject prefab;

        if (isPlayerBullet)
        {
            pool = bulletPools["player"];
            prefab = playerBulletPrefab;
        }
        else if (bulletName == "bulletEnemyUzi")
        {
            pool = bulletPools["enemyUzi"];
            prefab = enemyUziBulletPrefab;
        }
        else
        {
            pool = bulletPools["enemy"];
            prefab = enemyBulletPrefab;
        }

        GameObject bullet;

        if (pool.Count > 0)
        {
            bullet = pool.Dequeue();
        }
        else
        {
            Debug.LogWarning("⚠️ No hay balas disponibles, creando nuevas.");
            bullet = Instantiate(prefab);

            BulletControllerCop bulletController = bullet.GetComponent<BulletControllerCop>();
            if (bulletController != null)
            {
                bulletController.isPlayerBullet = isPlayerBullet;
            }
        }

        // Activar la bala y posicionarla
        bullet.SetActive(true);
        bullet.transform.position = position;
        bullet.transform.rotation = rotation;

        // Asignar daño
        int bulletDamage = GetBulletDamageFromData(bulletName);
        BulletControllerCop controller = bullet.GetComponent<BulletControllerCop>();
        if (controller != null)
        {
            controller.SetDamage(bulletDamage);
        }

        // Ahora sí hacemos el spawn en la red (cuando la bala está activa)
        NetworkServer.Spawn(bullet);

        return bullet;
    }

    private int GetBulletDamageFromData(string bulletName)
    {
        // Si DataManager.enemiesData es null o está vacío, devolver un valor por defecto
        if (DataManager.enemiesData == null || DataManager.enemiesData.Count == 0)
        {
            return 10;
        }

        foreach (EnemyDataInfo enemyData in DataManager.enemiesData)
        {
            if (enemyData != null && enemyData.bulletName == bulletName)
            {
                return enemyData.damage;
            }
        }

        return 10;
    }

    public void ReturnBullet(GameObject bullet, bool isPlayerBullet)
    {
        if (!isServer) return;

        // Des-registrar de la red primero
        NetworkServer.UnSpawn(bullet);

        bullet.SetActive(false);

        if (isPlayerBullet)
        {
            bulletPools["player"].Enqueue(bullet);
        }
        else if (bullet.name.Contains("Uzi"))
        {
            bulletPools["enemyUzi"].Enqueue(bullet);
        }
        else
        {
            bulletPools["enemy"].Enqueue(bullet);
        }
    }

    public void ReturnBullet(GameObject bullet)
    {
        BulletControllerCop bulletController = bullet.GetComponent<BulletControllerCop>();
        bool isPlayerBullet = bulletController != null && bulletController.isPlayerBullet;
        ReturnBullet(bullet, isPlayerBullet);
    }

    [Server]
    public void SpawnImpactParticles(Vector3 position, string impactType)
    {
        GameObject particlePrefab = null;

        switch (impactType)
        {
            case "Wall":
                particlePrefab = wallImpactParticles;
                break;
            case "Enemy":
                particlePrefab = enemyImpactParticles;
                break;
            case "Player":
                particlePrefab = playerImpactParticles;
                break;
        }

        if (particlePrefab != null)
        {
            GameObject particles = Instantiate(particlePrefab, position, Quaternion.identity);
            NetworkServer.Spawn(particles);

            // Autodestrucción después de un tiempo
            StartCoroutine(DestroyAfterDelay(particles, 2f));
        }
    }

    private System.Collections.IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            NetworkServer.Destroy(obj);
        }
    }
}