using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class DeathScreenController : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    private Button retryButton;

    // Referencias
    [SerializeField] private PlayerController playerController;

    // Para la animaci�n de aparici�n
    private float fadeInDuration = 0.5f;
    private float currentFadeTime = 0f;
    private bool isFading = false;

    // Par�metro para decidir si pausar el juego
    [SerializeField] private bool pauseGameOnDeath = true; // Esto puedes cambiarlo desde el Inspector

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    private void Start()
    {
        // Obtener referencias de UI
        root = uiDocument.rootVisualElement.Q("root");
        retryButton = root.Q<Button>("retry-button");

        // Configurar el bot�n de reintentar
        retryButton.clicked += OnRetryClicked;

        // Ocultar la pantalla al inicio
        root.style.opacity = 0;
        root.style.display = DisplayStyle.None;

        // Si no hay referencia al jugador, buscarlo en la escena
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
    }

    private void Update()
    {
        // Verificar si el jugador ha muerto
        if (playerController != null && playerController.IsDead() && !isFading && root.style.display == DisplayStyle.None)
        {
            ShowDeathScreen();
        }

        // Manejar la animaci�n de fadeIn
        if (isFading)
        {
            currentFadeTime += Time.unscaledDeltaTime; // Usamos Time.unscaledDeltaTime para evitar que la animaci�n se vea afectada por la pausa
            float normalizedTime = Mathf.Clamp01(currentFadeTime / fadeInDuration);
            root.style.opacity = normalizedTime;

            if (normalizedTime >= 1f)
            {
                isFading = false;
            }
        }
    }

    private void ShowDeathScreen()
    {
        // Mostrar la pantalla
        root.style.display = DisplayStyle.Flex;

        // Iniciar la animaci�n de fadeIn
        currentFadeTime = 0f;
        isFading = true;

       
    }

    private void OnRetryClicked()
    {
        // Restaurar el tiempo de juego
        Time.timeScale = 1f;

        // Recargar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
