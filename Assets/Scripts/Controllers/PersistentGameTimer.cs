using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class PersistentGameTimer : MonoBehaviour
{
    // Singleton para mantener el timer entre escenas
    public static PersistentGameTimer Instance { get; private set; }

    // Evento para cuando el timer necesite enviarse a la pantalla de victoria
    public event Action<float> OnTimerStopped;

    // Nombre de la primera escena donde se debe iniciar el timer
    [SerializeField] private string firstLevelSceneName = "LevelOneCinematic";

    // Tiempo total transcurrido
    private float elapsedTime = 0f;

    // Indica si el timer está activo
    private bool isRunning = false;

    private void Awake()
    {
        // Implementación de Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Suscribirse a eventos de cambio de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene Loaded: " + scene.name); // Asegúrate de que el nombre de la escena sea el esperado

        // Cuando se carga la primera escena (LevelOneCinematic), reiniciar el temporizador
        if (scene.name == firstLevelSceneName)
        {
            // Reiniciar el tiempo y empezar el timer desde cero
            elapsedTime = 0f;
            StartTimer();
        }

        // Detener el timer cuando se llega a la pantalla final (FinalScreen)
        if (scene.name == "FinalScreen")
        {
            StopTimer();
        }
    }

    private void Update()
    {
        // Incrementar el tiempo si el timer está activo
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
        }
    }


    public void StartTimer()
    {
        isRunning = true;
        Debug.Log("Game Timer Started");
    }

    public void StopTimer()
    {
        isRunning = false;

        // Disparar el evento para enviar el tiempo total a la pantalla de victoria
        OnTimerStopped?.Invoke(elapsedTime);
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    public string GetFormattedTime()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedTime);
        return string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
    }

    private void OnDestroy()
    {
        // Desubscribirse para evitar memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}