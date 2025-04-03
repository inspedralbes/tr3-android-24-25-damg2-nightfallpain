using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using GameData;

public class ExperienceController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    
    [SerializeField] private int enemigosRequeridos = 5;
    [SerializeField] private int monedasRequeridas = 10;
    
    private int enemigosMuertos = 0;
    private int monedasRecogidas = 0;
    [SerializeField] private string sceneName;

    private string apiUrl = "http://187.33.145.98:3000/api/usuarios/game/update-xp";
    private int xpAlInicioDelNivel;
    
    private bool escenaCargandose = false;  // 🔹 Nueva bandera

    void Start()
    {
        xpAlInicioDelNivel = PlayerPrefs.GetInt("player_xp", 0);
    }

    void Update()
    {
        if (!escenaCargandose && enemigosMuertos >= enemigosRequeridos && monedasRecogidas >= monedasRequeridas)
        {
            escenaCargandose = true;  // 🔹 Marcamos que ya está cargando
            StartCoroutine(LoadSceneWithDelay());
        }
    }

    IEnumerator LoadSceneWithDelay()
    {
        string username = PlayerPrefs.GetString("player_username", "");
        if (!string.IsNullOrEmpty(username))
        {
            yield return StartCoroutine(EnviarXPAlServidorCoroutine(username, PlayerPrefs.GetInt("player_xp", xpAlInicioDelNivel)));
        }
    
        SceneManager.sceneLoaded += OnSceneLoaded;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = true; // 🔹 Asegurar activación de la nueva escena
    
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    
        escenaCargandose = false;  // 🔹 Reseteamos la bandera tras la carga
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    SceneManager.sceneLoaded -= OnSceneLoaded; // 🔹 Desuscribirse del evento

    if (DataManager.enemiesData != null && DataManager.enemiesData.Count > 0)
    {
        foreach (EnemyController enemy in FindObjectsOfType<EnemyController>())
        {
            enemy.InitializeEnemyData();
        }
    }
    else
    {
        Debug.LogWarning("[ExperienceController] No se han cargado datos de enemigos en EnemyDataManager.");
    }
}

    public void EnemigoMuerto()
    {
        enemigosMuertos++;
        ActualizarXP(100);
    }

    public void MonedaRecogida()
    {
        monedasRecogidas++;
        ActualizarXP(10);
    }

    private void ActualizarXP(int xpGanado)
    {
        int xpActual = PlayerPrefs.GetInt("player_xp", 0);
        PlayerPrefs.SetInt("player_xp", xpActual + xpGanado);
        PlayerPrefs.Save();
    }

    private IEnumerator EnviarXPAlServidorCoroutine(string username, int xp)
    {
        string jsonData = JsonUtility.ToJson(new XPData(username, xp));

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "PUT"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[✅] XP enviada correctamente al servidor: " + xp);
            }
            else
            {
                Debug.LogError("[❌] Error al enviar XP: " + request.error);
            }
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

[System.Serializable]
public class XPData
{
    public string name;
    public int xp;

    public XPData(string name, int xp)
    {
        this.name = name;
        this.xp = xp;
    }
}
