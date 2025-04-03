using UnityEngine;
using UnityEngine.SceneManagement;

public class ReiniciarController : MonoBehaviour
{
    void Update()
    {
        // Detectar si se presiona la tecla R
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Reiniciar la escena actual
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
