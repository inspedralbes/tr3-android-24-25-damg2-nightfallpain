using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Text;

public class VictoryScreenController : MonoBehaviour
{
    [Serializable]
    public class EstadisticasRequest
    {
        public string usuari_id;
        public int temps;
        public int puntuacio; // Nuevo campo para el score
    }

    [SerializeField] private UIDocument victoryDocument;
    [SerializeField] private string mainMenuSceneName = "menuInicial";
    [SerializeField] private float tiempoTranscurrido;
    [SerializeField] private int playerScore; // Nuevo campo para el score
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private string serverUrl = "http://187.33.145.98:3000/api/estadistiques/guardarEstadisticas";

    private VisualElement rootElement;
    private Label victoryTitle;
    private Label xpValue;
    private Label timeValue;
    private Label scoreValue; // Nueva referencia al Label del score
    private Button mainMenuButton;
    private AudioSource audioSource;

    private void Start()
    {
        Debug.Log("🏁 VictoryScreenController iniciado.");

        // Obtener el tiempo transcurrido y el score
        tiempoTranscurrido = GetElapsedTime();
        playerScore = GetPlayerScore();
        Debug.Log($"📊 Estadísticas registradas - Tiempo: {tiempoTranscurrido} segundos, Score: {playerScore}");

        StartCoroutine(SendGameStatistics());
    }

    private float GetElapsedTime()
    {
        return Time.time; // O tu propia lógica para calcular el tiempo
    }

    private int GetPlayerScore()
    {
        return PlayerPrefs.GetInt("player_xp", 0); // Obtiene el score de PlayerPrefs
    }

    private IEnumerator SendGameStatistics()
    {
        Debug.Log("🚀 Enviando estadísticas...");

        string username = PlayerPrefs.GetString("player_username", string.Empty);
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("⚠️ Error: No se encontró nombre de usuario.");
            yield break;
        }

        // Validación del score (puede ser 0 pero no negativo)
        if (playerScore < 0)
        {
            Debug.LogWarning("⚠️ Score negativo detectado. Se ajustará a 0.");
            playerScore = 0;
        }

        // Crear el objeto con todos los campos
        var estadisticas = new EstadisticasRequest
        {
            usuari_id = username,
            temps = Mathf.RoundToInt(tiempoTranscurrido),
            puntuacio = playerScore
        };

        string jsonData = JsonUtility.ToJson(estadisticas);
        Debug.Log($"📤 Datos preparados para enviar: {jsonData}");

        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ Error al enviar estadísticas: {request.error}");
                Debug.LogError($"📄 Respuesta del servidor: {request.downloadHandler.text}");
            }
            else
            {
                Debug.Log("✅ Estadísticas enviadas correctamente.");
                Debug.Log($"📄 Respuesta: {request.downloadHandler.text}");
            }
        }
    }

    private void OnEnable()
    {
        rootElement = victoryDocument.rootVisualElement.Q("victory-container");
        victoryTitle = rootElement.Q<Label>("victory-title");
        xpValue = rootElement.Q<Label>("xp-value");
        timeValue = rootElement.Q<Label>("time-value");
        scoreValue = rootElement.Q<Label>("score-value"); // Obtiene referencia al Label del score
        mainMenuButton = rootElement.Q<Button>("main-menu-button");

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        mainMenuButton.clicked += OnMainMenuButtonClicked;
        StartCoroutine(PlayVictorySequence());
    }

    private void OnDisable()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.clicked -= OnMainMenuButtonClicked;
        }
    }

    private IEnumerator PlayVictorySequence()
    {
        if (victoryMusic != null)
        {
            audioSource.clip = victoryMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

        victoryTitle.AddToClassList("fade-in");
        yield return new WaitForSeconds(0.5f);

        xpValue.AddToClassList("fade-in");
        StartCoroutine(AnimateXPValue(xpValue));

        yield return new WaitForSeconds(0.5f);

        // Mostrar tiempo
        TimeSpan timeSpan = TimeSpan.FromSeconds(tiempoTranscurrido);
        timeValue.text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
        // Mostrar score
        scoreValue.text = playerScore.ToString("N0");
        scoreValue.RemoveFromClassList("fade-in");
        scoreValue.style.opacity = 1;
    }

    private IEnumerator AnimateXPValue(Label xpLabel)
    {
        Vector3 originalScale = xpLabel.transform.scale;
        float scaleDuration = 0.2f;
        float blinkDuration = 0.5f;

        while (true)
        {
            xpLabel.transform.scale = originalScale * 1.2f;
            xpLabel.style.opacity = 0.9f;
            yield return new WaitForSeconds(scaleDuration);

            xpLabel.transform.scale = originalScale;
            xpLabel.style.opacity = 1f;
            yield return new WaitForSeconds(scaleDuration);

            yield return new WaitForSeconds(blinkDuration);
        }
    }

    private void OnMainMenuButtonClicked()
    {
        Debug.Log($"Returning to main menu: {mainMenuSceneName}");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void GameOver()
    {
        Debug.Log("🎮 GameOver() llamado.");
        Debug.Log("📡 Enviando estadísticas desde GameOver()");
        StartCoroutine(SendGameStatistics());
    }
}