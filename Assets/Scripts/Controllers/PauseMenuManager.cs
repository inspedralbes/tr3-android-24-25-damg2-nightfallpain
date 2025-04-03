using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;
using System.Collections;


public class PauseMenuManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AudioClip menuOpenSound;
    [SerializeField] private AudioClip menuCloseSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private float animationDuration = 0.3f;

    // UI Elements
    private UIDocument uiDocument;
    private VisualElement pauseMenu;
    private VisualElement overlay;
    private Button resumeButton;
    private Button settingsButton;
    private Button quitButton;

    // Estado
    private bool isPaused = false;
    private bool isAnimating = false;

    // Audio
    private AudioSource audioSource;

    // Evento para notificar a otros scripts cuando el juego está pausado/despausado
    public static event Action<bool> OnGamePaused;

    // Propiedad para verificar si el juego está pausado desde otros scripts
    public static bool IsGamePaused { get; private set; }

    private void Awake()
    {
        // Configurar el AudioSource
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    void Start()
    {
        // Obtener el UIDocument y el root
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // Obtener el menú de pausa y overlay
        pauseMenu = root.Q<VisualElement>("pauseMenu");
        overlay = root.Q<VisualElement>("overlay");

        // Si no existe el overlay, usar el pauseMenu como contenedor principal
        if (overlay == null)
            overlay = pauseMenu;

        // Botones
        resumeButton = root.Q<Button>("resumeButton");
        settingsButton = root.Q<Button>("optionsButton"); // not "settingsButton"
        quitButton = root.Q<Button>("quitButton");

        // Verificar que los botones existen y asignar handlers
        if (resumeButton != null)
            resumeButton.clicked += ResumeGame;

        if (settingsButton != null)
            settingsButton.clicked += OpenSettings;

        if (quitButton != null)
            quitButton.clicked += QuitGame;

        // Asegurar que el menú de pausa esté oculto al inicio
        HideMenuInstantly();

        // Inicializar el estado de pausa
        IsGamePaused = false;
    }

    void Update()
    {
        // Detectar la tecla "Esc" para abrir/cerrar el menú de pausa
        // Solo responder si no estamos en animación
        if (Input.GetKeyDown(KeyCode.Escape) && !isAnimating)
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            ShowPauseMenu();
        }
        else
        {
            HidePauseMenu();
        }

        // Actualizar el estado estático
        IsGamePaused = isPaused;

        // Notificar a los observadores
        OnGamePaused?.Invoke(isPaused);
    }

    private void ShowPauseMenu()
    {
        // Detener el tiempo del juego
        Time.timeScale = 0;

        // Reproducir sonido
        PlaySound(menuOpenSound);

        // Mostrar y animar el menú
        overlay.style.display = DisplayStyle.Flex;

        // Aplicar escala inicial
        if (pauseMenu != null)
        {
            pauseMenu.style.opacity = 0;
            pauseMenu.transform.scale = new Vector3(0.8f, 0.8f, 1f);
        }

        // Iniciar animación
        StartCoroutine(AnimateMenu(true));
    }

    private void HidePauseMenu()
    {
        // Restaurar el tiempo del juego
        Time.timeScale = 1;

        // Reproducir sonido
        PlaySound(menuCloseSound);

        // Animar salida del menú
        StartCoroutine(AnimateMenu(false));
    }

    private void HideMenuInstantly()
    {
        if (overlay != null)
            overlay.style.display = DisplayStyle.None;

        if (pauseMenu != null)
        {
            pauseMenu.style.opacity = 0;
            pauseMenu.transform.scale = new Vector3(0.8f, 0.8f, 1f);
        }
    }

    private IEnumerator AnimateMenu(bool show)
    {
        isAnimating = true;

        float elapsed = 0;

        // Valores iniciales y finales para la animación
        Vector3 startScale = show ? new Vector3(0.8f, 0.8f, 1f) : new Vector3(1f, 1f, 1f);
        Vector3 endScale = show ? new Vector3(1f, 1f, 1f) : new Vector3(0.8f, 0.8f, 1f);
        float startOpacity = show ? 0 : 1;
        float endOpacity = show ? 1 : 0;

        // Si solo tenemos el pauseMenu sin overlay
        VisualElement elementToAnimate = pauseMenu != null ? pauseMenu : overlay;

        while (elapsed < animationDuration)
        {
            // Calcular progreso normalizado
            float t = elapsed / animationDuration;

            // Función de easing suave
            float smoothT = t * t * (3f - 2f * t); // Smoothstep

            // Interpolar valores
            float currentOpacity = Mathf.Lerp(startOpacity, endOpacity, smoothT);
            Vector3 currentScale = Vector3.Lerp(startScale, endScale, smoothT);

            // Aplicar valores
            elementToAnimate.style.opacity = currentOpacity;
            elementToAnimate.transform.scale = currentScale;

            // Incrementar tiempo usando unscaledDeltaTime (para que funcione incluso cuando el juego está pausado)
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Asegurarse de que los valores finales sean exactos
        elementToAnimate.style.opacity = endOpacity;
        elementToAnimate.transform.scale = endScale;

        // Si estamos ocultando, desactivar completamente el overlay
        if (!show)
        {
            overlay.style.display = DisplayStyle.None;
        }

        isAnimating = false;
    }

    private void ResumeGame()
    {
        PlaySound(buttonClickSound);
        isPaused = false;
        HidePauseMenu();

        // Actualizar el estado estático
        IsGamePaused = false;

        // Notificar a los observadores
        OnGamePaused?.Invoke(false);
    }

    public void OpenSettings()
    {
        PlaySound(buttonClickSound);
        // The ControlsMenuManager will handle showing the controls menu
    }

    private void QuitGame()
    {
        PlaySound(buttonClickSound);
        Debug.Log("Regresando al menú inicial...");

        // Pausar un momento para que se reproduzca el sonido antes de cambiar de escena
        StartCoroutine(DelayedLoadMenu());
    }

    private IEnumerator DelayedLoadMenu()
    {
        yield return new WaitForSecondsRealtime(0.2f);

        // Cargar la escena del menú inicial
        SceneManager.LoadScene("menuInicial");
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    // Método público para verificar si el juego está pausado
    public static bool IsPaused()
    {
        return IsGamePaused;
    }

    // Método público para pausar/despausar el juego desde otros scripts
    public void SetPaused(bool paused)
    {
        if (isPaused != paused && !isAnimating)
        {
            isPaused = paused;

            if (isPaused)
                ShowPauseMenu();
            else
                HidePauseMenu();

            // Actualizar el estado estático
            IsGamePaused = isPaused;

            // Notificar a los observadores
            OnGamePaused?.Invoke(isPaused);
        }
    }
}