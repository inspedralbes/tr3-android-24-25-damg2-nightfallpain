using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    void Start()
    {
        // Obtener el root visual element
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Buscar el botón usando su nombre
        var myButton = root.Q<Button>("Bigbtn"); // Asegúrate de que "MyButton" sea el nombre del botón en tu UXML

        // Agregar un evento al hacer clic
        myButton.clicked += OnButtonClick;
    }

    // Función que se ejecutará cuando el botón sea presionado
    private void OnButtonClick()
    {
        // Cambiar a otra escena, usando el nombre de la escena
        SceneManager.LoadScene("Login"); // Cambia "NombreDeTuEscena" por el nombre real de la escena
    }
}
