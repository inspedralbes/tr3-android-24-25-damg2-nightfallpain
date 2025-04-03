using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;  // Importar TextMeshPro


public class TimerSceneController : MonoBehaviour
{
    [SerializeField] private float tiempoParaCambio = 10f; // Tiempo en segundos antes de cambiar de escena
    [SerializeField] private string sceneName; // Nombre de la escena a cargar
    [SerializeField] private TextMeshProUGUI timerText; // Referencia al TextMeshProUGUI
    [SerializeField] private AudioSource sonidoFinal; // Sonido opcional al terminar el tiempo

    private float tiempoRestante;
    private bool escenaCambiada = false; // Para evitar múltiples llamadas

    void Start()
    {
        tiempoRestante = tiempoParaCambio;
        ActualizarTexto();
    }

    void Update()
    {
        if (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            ActualizarTexto();

            if (tiempoRestante <= 3) // Efecto de parpadeo en los últimos 3 segundos
            {
                timerText.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * 3, 1));
            }
        }
        else if (!escenaCambiada)
        {
            escenaCambiada = true;
            if (sonidoFinal != null) sonidoFinal.Play(); // Reproduce sonido si está asignado
            CambiarEscena();
        }
    }

    void ActualizarTexto()
    {
        if (timerText != null)
        {
            int minutos = Mathf.FloorToInt(tiempoRestante / 60);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    void CambiarEscena()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("El nombre de la escena no ha sido asignado.");
        }
    }
}
