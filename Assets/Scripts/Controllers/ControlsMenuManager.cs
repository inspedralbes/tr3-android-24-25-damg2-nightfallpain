using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;


public class ControlsMenuManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AudioClip menuOpenSound;
    [SerializeField] private AudioClip menuCloseSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private float animationDuration = 0.3f;

    // UI Elements
    private UIDocument uiDocument;
    private VisualElement controlsOverlay;
    private VisualElement controlsPanel;
    private Button backButton;

    // Estado
    private bool isShowing = false;
    private bool isAnimating = false;

    // Audio
    private AudioSource audioSource;

    // Referencia al PauseMenuManager
    private PauseMenuManager pauseMenuManager;

    private void Awake()
    {
        // Configurar el AudioSource
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        // Obtener referencia al PauseMenuManager
        pauseMenuManager = FindObjectOfType<PauseMenuManager>();
    }

    void Start()
    {
        // Obtener el UIDocument y el root
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // Obtener los elementos del menú de controles
        controlsOverlay = root.Q<VisualElement>("controls-overlay");
        controlsPanel = root.Q<VisualElement>("controls-panel");
        backButton = root.Q<Button>("backButton");

        // Verificar que los elementos existen y asignar handlers
        if (backButton != null)
            backButton.clicked += CloseControlsMenu;

        // Asegurarse de que el menú de controles esté oculto al inicio
        HideMenuInstantly();

        // Suscribirse al evento del botón de controles en el menú de pausa
        Button optionsButton = GameObject.FindObjectOfType<PauseMenuManager>()
            .GetComponent<UIDocument>().rootVisualElement.Q<Button>("optionsButton");

        if (optionsButton != null)
            optionsButton.clicked += ShowControlsMenu;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isShowing)
        {
            CloseControlsMenu();
        }
    }

    public void ShowControlsMenu()
    {
        if (isShowing || isAnimating)
            return;

        // Reproducir sonido
        PlaySound(menuOpenSound);

        // Mostrar y animar el menú
        controlsOverlay.style.display = DisplayStyle.Flex;

        // Aplicar escala inicial
        controlsPanel.style.opacity = 0;
        controlsPanel.transform.scale = new Vector3(0.8f, 0.8f, 1f);

        // Iniciar animación
        StartCoroutine(AnimateMenu(true));
    }

    public void CloseControlsMenu()
    {
        if (!isShowing || isAnimating)
            return;

        // Reproducir sonido
        PlaySound(buttonClickSound);

        // Animar salida del menú
        StartCoroutine(AnimateMenu(false));
    }

    private void HideMenuInstantly()
    {
        controlsOverlay.style.display = DisplayStyle.None;
        controlsPanel.style.opacity = 0;
        controlsPanel.transform.scale = new Vector3(0.8f, 0.8f, 1f);
        isShowing = false;
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
            controlsPanel.style.opacity = currentOpacity;
            controlsPanel.transform.scale = currentScale;

            // Incrementar tiempo usando unscaledDeltaTime (para que funcione incluso cuando el juego está pausado)
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Asegurarse de que los valores finales sean exactos
        controlsPanel.style.opacity = endOpacity;
        controlsPanel.transform.scale = endScale;

        // Si estamos ocultando, desactivar completamente el overlay
        if (!show)
        {
            controlsOverlay.style.display = DisplayStyle.None;

            // Reproducir sonido de cierre
            PlaySound(menuCloseSound);
        }

        isShowing = show;
        isAnimating = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
