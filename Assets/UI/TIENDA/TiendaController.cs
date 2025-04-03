using System;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;

public class TiendaController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TextMeshProUGUI experienciaTexto;
    [SerializeField] private BarraVidaController barraVida;

    [Header("Mejoras de Vida")]
    [SerializeField] private int[] costesVida = { 1000, 2000, 3000 };
    [SerializeField] private float[] valoresVida = { 20f, 30f, 50f };

    [Header("Mejoras de Munición")]
    [SerializeField] private int[] costesMunicion = { 800, 1500, 2500 };
    [SerializeField] private int[] valoresMunicion = { 5, 5, 5 };

    private int nivelVidaActual = 0;
    private int nivelMunicionActual = 0;

    private UIDocument uiDocument;
    private VisualElement root;

    private Button cerrarBoton;
    private Label experienciaLabel;
    private VisualElement tiendaPanel;

    private Button[] botonesVida;
    private Label[] nivelesVida;
    private Label[] costesVidaLabels;

    private Button[] botonesMunicion;
    private Label[] nivelesMunicion;
    private Label[] costesMunicionLabels;

    private bool tiendaAbierta = false;

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    void Start()
    {
        // Cargar los datos de la tienda
        CargarDatosDeTienda();

        if (barraVida != null)
        {
            barraVida.SetVidaMaxima(playerController.GetVidaMaxima());
        }

        if (uiDocument == null)
        {
            Debug.LogError("UIDocument no encontrado en TiendaController!");
            return;
        }

        InitializeUI();
        SetTiendaVisible(false);
    }

    void Update()
    {
        if (experienciaLabel != null && playerController != null)
        {
            UpdateExperienciaUI();

        }
        // Abrir/cerrar tienda con la tecla T
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleTienda();
        }
    }

    private void CargarDatosDeTienda()
    {
        // Cargar los valores desde PlayerPrefs
        if (PlayerPrefs.HasKey("player_maxHealth"))
        {
            nivelVidaActual = PlayerPrefs.GetInt("player_maxHealthLevel", 0);
        }

        if (PlayerPrefs.HasKey("player_maxAmmo"))
        {
            nivelMunicionActual = PlayerPrefs.GetInt("player_maxAmmoLevel", 0);
        }
    }

    private void GuardarDatosDeTienda()
    {
        // Guardar los valores actuales de la tienda en PlayerPrefs
        PlayerPrefs.SetInt("player_maxHealthLevel", nivelVidaActual);
        PlayerPrefs.SetInt("player_maxAmmoLevel", nivelMunicionActual);
        PlayerPrefs.Save();
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement;

        tiendaPanel = root.Q<VisualElement>("tienda-panel");
        experienciaLabel = root.Q<Label>("experiencia-valor");
        cerrarBoton = root.Q<Button>("cerrar-boton");

        botonesVida = new Button[costesVida.Length];
        nivelesVida = new Label[costesVida.Length];
        costesVidaLabels = new Label[costesVida.Length];

        for (int i = 0; i < costesVida.Length; i++)
        {
            int index = i;
            botonesVida[i] = root.Q<Button>($"mejorar-vida-{i + 1}");
            nivelesVida[i] = root.Q<Label>($"nivel-vida-{i + 1}");
            costesVidaLabels[i] = root.Q<Label>($"coste-vida-{i + 1}");

            if (botonesVida[i] != null)
            {
                botonesVida[i].clicked += () => MejorarVida(index);
                costesVidaLabels[i].text = costesVida[i].ToString();
            }
        }

        botonesMunicion = new Button[costesMunicion.Length];
        nivelesMunicion = new Label[costesMunicion.Length];
        costesMunicionLabels = new Label[costesMunicion.Length];

        for (int i = 0; i < costesMunicion.Length; i++)
        {
            int index = i;
            botonesMunicion[i] = root.Q<Button>($"mejorar-municion-{i + 1}");
            nivelesMunicion[i] = root.Q<Label>($"nivel-municion-{i + 1}");
            costesMunicionLabels[i] = root.Q<Label>($"coste-municion-{i + 1}");

            if (botonesMunicion[i] != null)
            {
                botonesMunicion[i].clicked += () => MejorarMunicion(index);
                costesMunicionLabels[i].text = costesMunicion[i].ToString();
            }
        }

        if (cerrarBoton != null)
        {
            cerrarBoton.clicked += ToggleTienda;
        }

        UpdateUpgradeButtonsState();
    }

    private void UpdateExperienciaUI()
    {
        int xp = playerController.GetExperience();
        experienciaLabel.text = xp.ToString();
        UpdateUpgradeButtonsState();
    }

    private void MejorarVida(int nivel)
    {
        if (nivel != nivelVidaActual)
        {
            Debug.LogWarning("Solo puedes mejorar en orden secuencial!");
            return;
        }

        int coste = costesVida[nivel];
        int experienciaActual = playerController.GetExperience();

        if (experienciaActual >= coste)
        {
            playerController.GastarExperiencia(coste);
            playerController.IncreaseMaxHealth(valoresVida[nivel]);

            nivelVidaActual++;
            GuardarDatosDeTienda();
            UpdateUpgradeButtonsState();
        }
        else
        {
            Debug.Log("No tienes suficiente experiencia para esta mejora.");
        }
    }

    private void MejorarMunicion(int nivel)
    {
        if (nivel != nivelMunicionActual)
        {
            Debug.LogWarning("Solo puedes mejorar en orden secuencial!");
            return;
        }

        int coste = costesMunicion[nivel];
        int experienciaActual = playerController.GetExperience();

        if (experienciaActual >= coste)
        {
            playerController.GastarExperiencia(coste);
            playerController.IncreaseMaxAmmo(valoresMunicion[nivel]);

            nivelMunicionActual++;
            GuardarDatosDeTienda();
            UpdateUpgradeButtonsState();
        }
        else
        {
            Debug.Log("No tienes suficiente experiencia para esta mejora.");
        }
    }

    private void UpdateUpgradeButtonsState()
    {
        int experienciaActual = playerController.GetExperience();

        for (int i = 0; i < costesVida.Length; i++)
        {
            if (botonesVida[i] != null)
            {
                bool esNivelActual = i == nivelVidaActual;
                bool puedeComprar = experienciaActual >= costesVida[i] && esNivelActual;
                bool yaComprado = i < nivelVidaActual;

                botonesVida[i].SetEnabled(puedeComprar);

                if (yaComprado)
                {
                    botonesVida[i].AddToClassList("comprado");
                    nivelesVida[i].text = "COMPRADO";
                }
                else
                {
                    botonesVida[i].RemoveFromClassList("comprado");
                    nivelesVida[i].text = $"Nivel {i + 1}";
                }
            }
        }

        for (int i = 0; i < costesMunicion.Length; i++)
        {
            if (botonesMunicion[i] != null)
            {
                bool esNivelActual = i == nivelMunicionActual;
                bool puedeComprar = experienciaActual >= costesMunicion[i] && esNivelActual;
                bool yaComprado = i < nivelMunicionActual;

                botonesMunicion[i].SetEnabled(puedeComprar);

                if (yaComprado)
                {
                    botonesMunicion[i].AddToClassList("comprado");
                    nivelesMunicion[i].text = "COMPRADO";
                }
                else
                {
                    botonesMunicion[i].RemoveFromClassList("comprado");
                    nivelesMunicion[i].text = $"Nivel {i + 1}";
                }
            }
        }
    }

    public void ToggleTienda()
    {
        tiendaAbierta = !tiendaAbierta;
        SetTiendaVisible(tiendaAbierta);

        Time.timeScale = tiendaAbierta ? 0 : 1;
    }

    private void SetTiendaVisible(bool visible)
    {
        if (tiendaPanel != null)
        {
            tiendaPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
