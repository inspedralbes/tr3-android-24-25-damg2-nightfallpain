using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    private TextField userTextField;
    private TextField passTextField;
    private Button loginButton;
    private Label loginLabel;

    private const string LOGIN_URL = "http://187.33.145.98:3000/api/usuarios/game/login"; // URL correcta

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Obtener los elementos del UI
        userTextField = root.Q<TextField>("username");
        passTextField = root.Q<TextField>("password");
        loginButton = root.Q<Button>("loginButton");
        loginLabel = root.Q<Label>("loginLabel");

        // Asignar evento de clic al botón de login
        loginButton.clicked += OnLoginClicked;
    }

    private void OnLoginClicked()
    {
        string username = userTextField.value;
        string password = passTextField.value;

        // Verificar que los campos no estén vacíos
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            loginLabel.text = "Por favor, completa todos los campos";
        }
        else
        {
            // Deshabilitar el botón mientras se procesa la solicitud
            loginButton.SetEnabled(false);
            loginLabel.text = "Cargando...";

            // Llamar a la función de login
            StartCoroutine(LoginRequest(username, password));
        }
    }

    private IEnumerator LoginRequest(string username, string password)
    {
        string jsonData = JsonUtility.ToJson(new LoginData(username, password));
        byte[] jsonToSend = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(LOGIN_URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        // Habilitar el botón de login nuevamente
        loginButton.SetEnabled(true);

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("Respuesta del servidor: " + responseText);

            LoginResponse response = JsonUtility.FromJson<LoginResponse>(responseText);
            PlayerPrefs.DeleteAll();


            if (response != null && response.user != null)
            {
                PlayerDataInfo playerData = response.user;

                // Guardar los datos del usuario en PlayerPrefs
                PlayerPrefs.SetString("player_username", playerData.username);
                PlayerPrefs.SetString("player_skin", playerData.skinName);
                PlayerPrefs.SetInt("player_xp", playerData.xp);
                PlayerPrefs.SetInt("player_health", playerData.health);
                PlayerPrefs.SetFloat("player_speed", playerData.speed);
                PlayerPrefs.SetInt("player_maxBulltes", playerData.maxBullets);
                PlayerPrefs.Save(); // Guardar los cambios en PlayerPrefs

                Debug.Log("[LoginManager] Datos del jugador guardados en PlayerPrefs.");

                // Mostrar mensaje de éxito
                loginLabel.text = "Inicio de sesión exitoso";

                ShowPlayerData();

                // Cambiar a la siguiente escena
                SceneManager.LoadScene("menuInicial");
            }
            else
            {
                loginLabel.text = "Error en los datos de inicio de sesión";
            }
        }
        else
        {
            Debug.LogError("Error: " + request.error);
            loginLabel.text = "Error al conectar con el servidor";
        }
    }
    private void ShowPlayerData()
    {
        // Comprobar si los datos están guardados
        if (PlayerPrefs.HasKey("player_username"))
        {
            string username = PlayerPrefs.GetString("player_username");
            string skinName = PlayerPrefs.GetString("player_skin");
            int xp = PlayerPrefs.GetInt("player_xp");
            int health = PlayerPrefs.GetInt("player_health");
            float speed = PlayerPrefs.GetFloat("player_speed");
            int maxBullets = PlayerPrefs.GetInt("player_maxBullets");

        }
        
    }

}

// Clase para manejar la respuesta del login del servidor
[System.Serializable]
public class LoginResponse
{
    public string message;
    public PlayerDataInfo user;
}

// Clase para enviar los datos del login
[System.Serializable]
public class LoginData
{
    public string email;
    public string password;

    public LoginData(string email, string password)
    {
        this.email = email;
        this.password = password;
    }
}

// Clase con la información del jugador
[System.Serializable]
public class PlayerDataInfo
{
    public string username;
    public string skinName;
    public int xp;
    public int health;
    public float speed;
    public int maxBullets;
}
