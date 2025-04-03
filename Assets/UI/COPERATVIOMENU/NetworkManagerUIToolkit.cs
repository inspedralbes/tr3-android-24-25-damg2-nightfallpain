using UnityEngine;
using UnityEngine.UIElements;
using Mirror;


public class NetworkManagerUIToolkit : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    private NetworkManager networkManager;
    private VisualElement root;
    private VisualElement networkPanel;

    // Referencias a elementos UI
    private Button hostButton;
    private Button clientButton;
    private Button serverButton;
    private Button stopClientButton;
    private Button stopServerButton;
    private Button stopHostButton;
    private Button readyButton;
    private TextField addressField;
    private TextField portField;
    private Label statusLabel;
    private VisualElement clientControls;
    private VisualElement serverControls;
    private VisualElement hostControls;
    private VisualElement connectingControls;
    private Button cancelButton;

    private void Awake()
    {
        // Obtenemos referencia al NetworkManager
        networkManager = GetComponent<NetworkManager>();

        if (networkManager == null)
        {
            Debug.LogError("No se encontró NetworkManager en este GameObject. Este componente requiere NetworkManager.");
            enabled = false;
            return;
        }

        // Verificamos que tengamos un UIDocument
        if (uiDocument == null)
        {
            Debug.LogError("Se requiere asignar un UIDocument en el inspector.");
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        if (uiDocument != null)
        {
            root = uiDocument.rootVisualElement;
            InitializeUI();
            UpdateUIVisibility(false); // Inicialmente oculto
        }
    }

    private void Update()
    {
        // Mostrar/ocultar UI con la tecla
        if (Input.GetKeyDown(toggleKey))
        {
            bool isVisible = networkPanel != null && networkPanel.style.display == DisplayStyle.Flex;
            UpdateUIVisibility(!isVisible);
        }

        // Actualizar estado de la UI
        if (networkPanel != null && networkPanel.style.display == DisplayStyle.Flex)
        {
            UpdateUIState();
        }
    }

    private void InitializeUI()
    {
        networkPanel = root.Q<VisualElement>("network-panel");

        if (networkPanel == null)
        {
            Debug.LogError("No se encontró 'network-panel' en el UXML. Verifica la estructura del UXML.");
            return;
        }

        // Obtener referencias a todos los elementos UI
        hostButton = root.Q<Button>("host-button");
        clientButton = root.Q<Button>("client-button");
        serverButton = root.Q<Button>("server-button");
        stopClientButton = root.Q<Button>("stop-client-button");
        stopServerButton = root.Q<Button>("stop-server-button");
        stopHostButton = root.Q<Button>("stop-host-button");
        readyButton = root.Q<Button>("ready-button");
        addressField = root.Q<TextField>("address-field");
        portField = root.Q<TextField>("port-field");
        statusLabel = root.Q<Label>("status-label");
        clientControls = root.Q<VisualElement>("client-controls");
        serverControls = root.Q<VisualElement>("server-controls");
        hostControls = root.Q<VisualElement>("host-controls");
        connectingControls = root.Q<VisualElement>("connecting-controls");
        cancelButton = root.Q<Button>("cancel-button");

        Button closeButton = root.Q<Button>("close-button");

        // Configurar los botones
        if (closeButton != null)
            closeButton.clicked += () => UpdateUIVisibility(false);

        if (hostButton != null)
            hostButton.clicked += OnHostButtonClicked;

        if (clientButton != null)
            clientButton.clicked += OnClientButtonClicked;

        if (serverButton != null)
            serverButton.clicked += OnServerButtonClicked;

        if (stopClientButton != null)
            stopClientButton.clicked += OnStopClientButtonClicked;

        if (stopServerButton != null)
            stopServerButton.clicked += OnStopServerButtonClicked;

        if (stopHostButton != null)
            stopHostButton.clicked += OnStopHostButtonClicked;

        if (readyButton != null)
            readyButton.clicked += OnReadyButtonClicked;

        if (cancelButton != null)
            cancelButton.clicked += OnCancelButtonClicked;

        // Inicializar campos
        if (addressField != null)
            addressField.value = networkManager.networkAddress;

        if (portField != null && Transport.active is PortTransport portTransport)
            portField.value = portTransport.Port.ToString();
    }

    private void OnHostButtonClicked()
    {
        Debug.Log("Iniciando Host...");
        networkManager.StartHost();
    }

    private void OnClientButtonClicked()
    {
        Debug.Log("Iniciando Cliente...");

        // Establecer dirección IP
        if (addressField != null)
            networkManager.networkAddress = addressField.value;

        // Establecer puerto si está disponible
        if (portField != null && Transport.active is PortTransport portTransport)
        {
            if (ushort.TryParse(portField.value, out ushort port))
            {
                portTransport.Port = port;
                Debug.Log($"Configurando puerto: {port}");
            }
        }

        networkManager.StartClient();
    }

    private void OnServerButtonClicked()
    {
        Debug.Log("Iniciando Servidor...");
        networkManager.StartServer();
    }

    private void OnStopClientButtonClicked()
    {
        Debug.Log("Deteniendo Cliente...");
        networkManager.StopClient();
    }

    private void OnStopServerButtonClicked()
    {
        Debug.Log("Deteniendo Servidor...");
        networkManager.StopServer();
    }

    private void OnStopHostButtonClicked()
    {
        Debug.Log("Deteniendo Host...");
        networkManager.StopHost();
    }

    private void OnReadyButtonClicked()
    {
        Debug.Log("Cliente listo...");
        NetworkClient.Ready();
        if (NetworkClient.localPlayer == null)
        {
            Debug.Log("Añadiendo jugador local...");
            NetworkClient.AddPlayer();
        }
    }

    private void OnCancelButtonClicked()
    {
        Debug.Log("Cancelando conexión...");
        networkManager.StopClient();
    }

    private void UpdateUIState()
    {
        bool isClientConnected = NetworkClient.isConnected;
        bool isServerActive = NetworkServer.active;
        bool isClientActive = NetworkClient.active;

        // Actualizar visibilidad de controles
        if (hostControls != null)
            hostControls.style.display = DisplayStyle.None;

        if (clientControls != null)
            clientControls.style.display = DisplayStyle.None;

        if (serverControls != null)
            serverControls.style.display = DisplayStyle.None;

        if (connectingControls != null)
            connectingControls.style.display = DisplayStyle.None;

        if (readyButton != null)
            readyButton.style.display = DisplayStyle.None;

        // Mostrar sección activa
        if (isServerActive && isClientConnected)
        {
            // Modo Host
            if (hostControls != null)
                hostControls.style.display = DisplayStyle.Flex;

            if (statusLabel != null)
                statusLabel.text = $"Host: ejecutándose vía {Transport.active}";
        }
        else if (isServerActive)
        {
            // Solo servidor
            if (serverControls != null)
                serverControls.style.display = DisplayStyle.Flex;

            if (statusLabel != null)
                statusLabel.text = $"Servidor: ejecutándose vía {Transport.active}";
        }
        else if (isClientConnected)
        {
            // Cliente conectado
            if (clientControls != null)
                clientControls.style.display = DisplayStyle.Flex;

            if (statusLabel != null)
                statusLabel.text = $"Cliente: conectado a {networkManager.networkAddress} vía {Transport.active}";

            // Botón Ready si no está listo
            if (readyButton != null && !NetworkClient.ready)
                readyButton.style.display = DisplayStyle.Flex;
        }
        else if (isClientActive)
        {
            // Conectando
            if (connectingControls != null)
                connectingControls.style.display = DisplayStyle.Flex;

            if (statusLabel != null)
                statusLabel.text = $"Conectando a {networkManager.networkAddress}...";
        }
        else
        {
            // No hay conexiones
            if (hostButton != null)
                hostButton.style.display = DisplayStyle.Flex;

            if (clientButton != null)
                clientButton.style.display = DisplayStyle.Flex;

            if (serverButton != null)
                serverButton.style.display = DisplayStyle.Flex;

            if (statusLabel != null)
                statusLabel.text = "No conectado";
        }
    }

    private void UpdateUIVisibility(bool isVisible)
    {
        if (networkPanel != null)
            networkPanel.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}