using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private UIDocument uiDocument;
    private Button singleplayerButton;
    private Button cooperativeButton;
    private Button exitButton;
    private Button settingsButton;

    private Button closeSettingsButton; // Nuevo botón de cierre
    private VisualElement settingsMenu; // Menú de configuración

    private Label usernameLabel;
    private Label xpLabel;

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();

        // Cargar los datos del jugador al inicio
        LoadPlayerDataFromPrefs();  // Cargar los datos desde PlayerPrefs
    }

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;

        // Asignar los botones de la UI
        singleplayerButton = root.Q<Button>("singleplayer-button");
        cooperativeButton = root.Q<Button>("cooperative-button");
        exitButton = root.Q<Button>("exit-button");
        settingsButton = root.Q<Button>("settings-button");
        settingsMenu = root.Q<VisualElement>("settings-menu");
        closeSettingsButton = root.Q<Button>("closesettings-btn"); // Nuevo botón de cierre

        // Asignar las etiquetas donde se mostrará la información
        usernameLabel = root.Q<Label>("username-label");
        xpLabel = root.Q<Label>("xp-label");

        settingsMenu.style.display = DisplayStyle.None; // Ocultar al inicio

        singleplayerButton.clicked += OnSingleplayerButtonClicked;
        cooperativeButton.clicked += OnCooperativeButtonClicked;
        exitButton.clicked += OnExitButtonClicked;
        settingsButton.clicked += OnSettingsButtonClicked;
        closeSettingsButton.clicked += OnCloseSettingsButtonClicked;
    }

    void OnDisable()
    {
        // Eliminar los callbacks para evitar pérdidas de memoria
        if (singleplayerButton != null) singleplayerButton.clicked -= OnSingleplayerButtonClicked;
        if (cooperativeButton != null) cooperativeButton.clicked -= OnCooperativeButtonClicked;
        if (exitButton != null) exitButton.clicked -= OnExitButtonClicked;
        if (settingsButton != null) settingsButton.clicked -= OnSettingsButtonClicked;
        if (closeSettingsButton != null) closeSettingsButton.clicked -= OnCloseSettingsButtonClicked;
    }

    private void OnSingleplayerButtonClicked()
    {
        Debug.Log("Cargando escena de un jugador...");
        SceneManager.LoadScene("pantallaCarga");
    }

    private void OnCooperativeButtonClicked()
    {
        Debug.Log("Modo cooperativo seleccionado");
        SceneManager.LoadScene("pantallaCargaCoperativo");


        // Implementar l�gica para modo cooperativo
        // Implementar lógica para modo cooperativo
    }

    private void OnSettingsButtonClicked()
    {
        Debug.Log("Mostrando configuraciones...");
        
        // Actualizar la UI con la información del jugador
        UpdatePlayerInfo();

        // Mostrar el menú de configuraciones
        settingsMenu.style.display = DisplayStyle.Flex;
    }

    private void OnCloseSettingsButtonClicked()
    {
        Debug.Log("Cerrando configuraciones...");
        settingsMenu.style.display = DisplayStyle.None;
    }

    private void OnExitButtonClicked()
    {
        Debug.Log("Saliendo del juego");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Método para actualizar la información del jugador en las etiquetas
    private void UpdatePlayerInfo()
    {
        // Leer los datos desde PlayerPrefs
        string username = PlayerPrefs.GetString("player_username", "No disponible");
        int xp = PlayerPrefs.GetInt("player_xp", 0);

        // Asignar los valores a las etiquetas de la UI
        usernameLabel.text =  username;
        xpLabel.text =  xp.ToString();
    }

    // Método para cargar los datos del jugador desde PlayerPrefs
    private void LoadPlayerDataFromPrefs()
    {
        // Asegúrate de que los datos estén guardados en PlayerPrefs
        if (PlayerPrefs.HasKey("player_username"))
        {
            Debug.Log("[MainMenuController] Datos del jugador cargados desde PlayerPrefs.");
        }
        else
        {
            Debug.Log("[MainMenuController] No se encontraron datos del jugador en PlayerPrefs.");
        }
    }
}
