using UnityEngine;

namespace Mirror
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/Custom Network Manager HUD")]
    [RequireComponent(typeof(NetworkManager))]
    public class CustomNetworkManagerHUD : MonoBehaviour
    {
        NetworkManager manager;
        public int offsetX;
        public int offsetY;

        GUIStyle buttonStyle;
        GUIStyle labelStyle;
        GUIStyle boxStyle;
        GUIStyle textFieldStyle;

        private bool showHUD = true; // Controla si el HUD está visible

        void Awake()
        {
            manager = GetComponent<NetworkManager>();
        }

        void Update()
        {
            // Detectar la tecla "H" para alternar la visibilidad del HUD
            if (Input.GetKeyDown(KeyCode.H))
            {
                showHUD = !showHUD;  // Cambiar el estado de visibilidad
            }
        }

        void OnGUI()
        {
            if (!showHUD) return; // Si el HUD no debe mostrarse, no dibujamos nada.

            InitializeStyles(); // Mover la inicialización aquí

            int width = 700;  // Tamaño de la caja aumentado
            int height = 500; // Tamaño de la caja aumentado
            float xPosition = (Screen.width - width) / 2 + offsetX;  // Centrando en X
            float yPosition = (Screen.height - height) / 2 + offsetY; // Centrando en Y

            GUILayout.BeginArea(new Rect(xPosition, yPosition, width, height), boxStyle); // Caja con fondo negro y borde amarillo
            GUILayout.Label("Network Manager", labelStyle);

            if (!NetworkClient.isConnected && !NetworkServer.active)
                StartButtons();
            else
                StatusLabels();

            if (NetworkClient.isConnected && !NetworkClient.ready)
            {
                if (GUILayout.Button("Client Ready", buttonStyle))
                {
                    NetworkClient.Ready();
                    if (NetworkClient.localPlayer == null)
                        NetworkClient.AddPlayer();
                }
            }
            StopButtons();
            GUILayout.EndArea();
        }

        void InitializeStyles()
        {
            // Estilo para botones (duplicado tamaño)
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 32,  // Tamaño de letra duplicado
                fontStyle = FontStyle.Bold,
                fixedHeight = 80,  // Aumentar el tamaño del botón
                normal = { textColor = Color.white, background = Texture2D.grayTexture }
            };

            // Estilo para etiquetas (duplicado tamaño)
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 36,  // Tamaño de letra duplicado
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.cyan }
            };

            // Estilo para la caja (background negro con opacidad 50% y borde amarillo)
            boxStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                border = new RectOffset(10, 10, 10, 10),
                padding = new RectOffset(10, 10, 10, 10),
            };

            // Crear textura para el fondo con opacidad
            Color32[] pixels = new Color32[1];
            pixels[0] = new Color32(0, 0, 0, 128); // Fondo negro con opacidad 50%
            Texture2D backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixels32(pixels);
            backgroundTexture.Apply();
            boxStyle.normal.background = backgroundTexture;

            // Estilo para los campos de texto (duplicado tamaño)
            textFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 28,  // Tamaño de letra duplicado
                normal = { textColor = Color.white, background = Texture2D.grayTexture }
            };
        }

        void StartButtons()
        {
            if (!NetworkClient.active)
            {
                if (GUILayout.Button("Host (Server + Client)", buttonStyle))
                    manager.StartHost();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Client", buttonStyle)) 
                    manager.StartClient();
                manager.networkAddress = GUILayout.TextField(manager.networkAddress, textFieldStyle);

                // Campo para el puerto si es necesario
                if (Transport.active is PortTransport portTransport)
                {
                    if (ushort.TryParse(GUILayout.TextField(portTransport.Port.ToString(), textFieldStyle), out ushort port))
                        portTransport.Port = port;
                }

                GUILayout.EndHorizontal();

                if (GUILayout.Button("Server Only", buttonStyle)) 
                    manager.StartServer();
            }
            else
            {
                GUILayout.Label($"Connecting to {manager.networkAddress}..", labelStyle);
                if (GUILayout.Button("Cancel Connection Attempt", buttonStyle)) 
                    manager.StopClient();
            }
        }

        void StatusLabels()
        {
            if (NetworkServer.active && NetworkClient.active)
            {
                GUILayout.Label("<b>Host</b>: running", labelStyle);
            }
            else if (NetworkServer.active)
            {
                GUILayout.Label("<b>Server</b>: running", labelStyle);
            }
            else if (NetworkClient.isConnected)
            {
                GUILayout.Label($"<b>Client</b>: connected to {manager.networkAddress}", labelStyle);
            }
        }

        void StopButtons()
        {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Stop Host", buttonStyle)) 
                    manager.StopHost();
                if (GUILayout.Button("Stop Client", buttonStyle)) 
                    manager.StopClient();
                GUILayout.EndHorizontal();
            }
            else if (NetworkClient.isConnected)
            {
                if (GUILayout.Button("Stop Client", buttonStyle)) 
                    manager.StopClient();
            }
            else if (NetworkServer.active)
            {
                if (GUILayout.Button("Stop Server", buttonStyle)) 
                    manager.StopServer();
            }
        }
    }
}
