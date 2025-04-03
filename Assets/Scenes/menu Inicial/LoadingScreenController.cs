using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using GameData;
using System.Text;

public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "LevelOneCinematic";
    [SerializeField] private float loadingTime = 15f;
    [SerializeField] private string enemyApiUrl = "http://187.33.145.98:3000/api/enemics/game/all";
    [SerializeField] private string createGameApiUrl = "http://187.33.145.98:3000/api/partida/create";
    private VisualElement root;
    private ProgressBar loadingBar;
    private float currentTime = 0f;
    private bool isLoading = false;
    private string username;

    void OnEnable()
    {
        // Obtenemos el nombre de usuario guardado
        username = PlayerPrefs.GetString("player_username");
        // Obtenemos la referencia al documento UI
        root = GetComponent<UIDocument>().rootVisualElement;

        // Encontramos la barra de progreso
        loadingBar = root.Q<ProgressBar>("loading-bar");

        // Cambiamos el color de la barra que avanza a morado
        loadingBar.style.backgroundColor = new StyleColor(new Color(0.5f, 0f, 0.5f)); // Color morado

        // Configuramos el botón para empezar el juego inmediatamente
        Button startNowButton = root.Q<Button>("start-now-button");
        startNowButton.clicked += () => StartCoroutine(LoadGameScene());

        // Iniciamos la carga automática
        StartCoroutine(AutoLoadGame());
    }

    IEnumerator AutoLoadGame()
    {
        isLoading = true;
        currentTime = 0f;

        // Paso 1: Crear la partida en la base de datos
        yield return StartCoroutine(CreateNewGame());

        // Paso 2: Cargar los datos de los enemigos
        yield return StartCoroutine(FetchEnemyData());

        while (currentTime < loadingTime && isLoading)
        {
            currentTime += Time.deltaTime;
            float progress = Mathf.Clamp01(currentTime / loadingTime);
            loadingBar.value = progress * 100; // La barra usa valores de 0-100

            yield return null;
        }

        if (isLoading)
        {
            yield return StartCoroutine(LoadGameScene());
        }
    }

    IEnumerator CreateNewGame()
    {
        Debug.Log("📡 Creando nueva partida en el servidor...");

        // Verificar que tenemos un nombre de usuario
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("❌ No hay nombre de usuario guardado en PlayerPrefs");
            yield break;
        }

        // Preparar los datos para enviar
        string jsonData = JsonUtility.ToJson(new RequestData { usuari_id = username });

        // Configurar la petición POST
        UnityWebRequest request = new UnityWebRequest(createGameApiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseJson = request.downloadHandler.text;
            Debug.Log($"✅ Partida creada correctamente para el usuario {username}: {responseJson}");
            // Guardar los datos de la partida en DataManager
            CreateGameResponse response = JsonUtility.FromJson<CreateGameResponse>(responseJson);
            if (response.success)
            {
                DataManager.currentPartida = new GameData.PartidaInfo
                {
                    id = response.partida._id,
                    usuari_id = response.partida.usuari_id,
                    tipus_partida = response.partida.tipus_partida,
                    createdAt = response.partida.createdAt
                };
                Debug.Log($"✅ Partida guardada en DataManager con ID: {GameData.DataManager.currentPartida.id}");
            }
        }
        else
        {
            Debug.LogError($"❌ Error al crear la partida: {request.error}");
        }
    }

    IEnumerator FetchEnemyData()
    {
        Debug.Log("📡 Conectando al servidor para obtener los enemigos...");

        UnityWebRequest request = UnityWebRequest.Get(enemyApiUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            List<EnemyDataInfo> enemies = JsonUtility.FromJson<EnemyList>(json).enemies;

            // Guardar los enemigos en EnemyDataManager sin modificar los datos
            DataManager.enemiesData.Clear();
            DataManager.enemiesData.AddRange(enemies);

            Debug.Log($"✅ Enemigos cargados: {enemies.Count}");
        }
        else
        {
            Debug.LogError($"❌ Error al obtener los enemigos: {request.error}");
        }
    }

    IEnumerator LoadGameScene()
    {
        isLoading = false;

        // Podemos añadir una animación de fade-out aquí
        VisualElement container = root.Q<VisualElement>("container");
        container.AddToClassList("fade-out");

        yield return new WaitForSeconds(1f);

        // Cargamos la escena del juego
        SceneManager.LoadScene(gameSceneName);
    }
}
// Clases auxiliares
[System.Serializable]
public class RequestData
{
    public string usuari_id;
}

[System.Serializable]
public class CreateGameResponse
{
    public bool success;
    public string message;
    public PartidaData partida;
}

[System.Serializable]
public class PartidaData
{
    public string _id;
    public string usuari_id;
    public string tipus_partida;
    public string createdAt;
}

// Contenedor para deserializar la lista de enemigos desde JSON
[System.Serializable]
public class EnemyList
{
    public List<EnemyDataInfo> enemies;
}
