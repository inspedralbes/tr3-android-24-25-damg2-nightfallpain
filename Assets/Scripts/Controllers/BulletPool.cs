using System.Collections.Generic;
using UnityEngine;
using GameData;  // Necesario para acceder al DataManager

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance;

    public GameObject playerBulletPrefab;    // Prefab de la bala del jugador
    public GameObject enemyBulletPrefab;     // Prefab de la bala del enemigo
    public GameObject enemyUziBulletPrefab;  // Prefab de la bala del Enemy Uzi
    public int poolSize = 10;

    private Queue<GameObject> playerBulletPool = new Queue<GameObject>();
    private Queue<GameObject> enemyBulletPool = new Queue<GameObject>();
    private Queue<GameObject> enemyUziBulletPool = new Queue<GameObject>(); // Cola para las balas del Enemy Uzi

    public GameObject wallImpactParticles;
    public GameObject enemyImpactParticles;
    public GameObject playerImpactParticles;

    void Awake()
    {
        // Singleton para acceder a BulletPool desde cualquier parte del código
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Evita instancias duplicadas
            return;
        }
    }

    void Start()
    {
        // Inicializamos todos los pools para cada tipo de bala
        FillPool(playerBulletPool, playerBulletPrefab, true);
        FillPool(enemyBulletPool, enemyBulletPrefab, false);
        FillPool(enemyUziBulletPool, enemyUziBulletPrefab, false);  // Llenar la pool de balas de Enemy Uzi
    }

    private void FillPool(Queue<GameObject> pool, GameObject prefab, bool isPlayerBullet)
    {
        // Rellenamos las colas con el número indicado de balas al inicio
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(prefab);
            bullet.SetActive(false);  // Desactivamos las balas al principio
            bullet.GetComponent<BulletController>().isPlayerBullet = isPlayerBullet;  // Definimos si la bala es del jugador o del enemigo
            pool.Enqueue(bullet);  // Añadimos la bala al pool
        }
    }

    // Función genérica para obtener una bala de un pool
    private GameObject GetBulletFromPool(Queue<GameObject> pool, GameObject prefab, Vector3 position, Quaternion rotation, bool isPlayerBullet, string bulletName)
    {
        GameObject bullet;

        // Si hay balas en el pool, las reutilizamos
        if (pool.Count > 0)
        {
            bullet = pool.Dequeue();
        }
        else
        {
            // Si no hay balas disponibles, creamos nuevas
            Debug.LogWarning("⚠️ No hay balas disponibles, creando nuevas.");
            bullet = Instantiate(prefab);
            bullet.GetComponent<BulletController>().isPlayerBullet = isPlayerBullet;
        }

        // Activamos la bala y la posicionamos en el lugar correcto
        bullet.SetActive(true);
        bullet.transform.position = position;
        bullet.transform.rotation = rotation;

        // Asignar daño según el bulletName (usando DataManager)
        int bulletDamage = GetBulletDamageFromData(bulletName);  // Obtener el daño del bulletName
        bullet.GetComponent<BulletController>().SetDamage(bulletDamage);  // Asignamos el daño a la bala

        return bullet;
    }

    // Método para obtener el daño basado en el bulletName desde el DataManager
    private int GetBulletDamageFromData(string bulletName)
    {
        // Buscar el enemigo correspondiente en el DataManager
        foreach (EnemyDataInfo enemyData in DataManager.enemiesData)
        {
            if (enemyData.bulletName == bulletName)  // Comparar el bulletName
            {
                return enemyData.damage;  // Retornar el daño asociado
            }
        }
        
        // Si no encontramos un bulletName correspondiente, retornamos 0 o un valor por defecto
        Debug.Log("bullet de player");
        return 10;
    }

    // Métodos para obtener las balas de los distintos tipos
    public GameObject GetPlayerBullet(Vector3 position, Quaternion rotation, string bulletName)
    {
        return GetBulletFromPool(playerBulletPool, playerBulletPrefab, position, rotation, true, bulletName);
    }

    public GameObject GetEnemyBullet(Vector3 position, Quaternion rotation, string bulletName)
    {
        return GetBulletFromPool(enemyBulletPool, enemyBulletPrefab, position, rotation, false, bulletName);
    }

    public GameObject GetEnemyUziBullet(Vector3 position, Quaternion rotation, string bulletName)
    {
        return GetBulletFromPool(enemyUziBulletPool, enemyUziBulletPrefab, position, rotation, false, bulletName);  // Obtener balas de Enemy Uzi
    }

    // Método genérico para obtener una bala, basado en si es del jugador, enemigo normal, o enemigo Uzi
    public GameObject GetBullet(Vector3 position, Quaternion rotation, bool isPlayerBullet, string bulletName)
    {
        if (isPlayerBullet)
        {
            return GetPlayerBullet(position, rotation, bulletName);  // Devuelve una bala del jugador
        }
        else if (bulletName == "bulletEnemyUzi")
        {
            return GetEnemyUziBullet(position, rotation, bulletName);  // Devuelve una bala de Enemy Uzi si es necesario
        }
        else if (bulletName == "bulletEnemy")
        {
            return GetEnemyBullet(position, rotation, bulletName);  // Devuelve una bala normal de enemigo
        }
        else
        {
            Debug.LogError("¡Tipo de bala no reconocido!");
            return null;
        }
    }

    // Método para devolver una bala al pool después de que se haya usado
    public void ReturnBullet(GameObject bullet)
    {
        BulletController bulletController = bullet.GetComponent<BulletController>();
        bool isPlayerBullet = bulletController != null && bulletController.isPlayerBullet;  // Determinamos si la bala es del jugador
        ReturnBullet(bullet, isPlayerBullet);
    }

    // Sobrecarga del método para devolver la bala, especificando si es del jugador
    public void ReturnBullet(GameObject bullet, bool isPlayerBullet)
    {
        bullet.SetActive(false);  // Desactivamos la bala
        (isPlayerBullet ? playerBulletPool : enemyBulletPool).Enqueue(bullet);  // Añadimos la bala de vuelta a la cola correspondiente
    }

    // Método para crear partículas de impacto según el tipo de objeto que fue impactado
    public void SpawnImpactParticles(Vector3 position, string impactType)
    {
        GameObject particles = impactType switch
        {
            "Wall" => wallImpactParticles,    // Partículas para impactos con paredes
            "Enemy" => enemyImpactParticles,  // Partículas para impactos con enemigos
            "Player" => playerImpactParticles, // Partículas para impactos con el jugador
            _ => null
        };

        // Si se encuentran partículas para el tipo de impacto, se instancian
        if (particles != null)
        {
            Instantiate(particles, position, Quaternion.identity);
        }
    }
}
