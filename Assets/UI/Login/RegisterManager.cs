using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Collections;
using System;

public class RegisterManager : MonoBehaviour
{
    public event Action OnRegisterSuccess; // Evento para notificar registro exitoso

    private TextField nameTextField;
    private TextField emailTextField;
    private TextField passTextField;
    private Button registerButton;
    private Label registerLabel;

    private const string REGISTER_URL = "http://187.33.145.98:3000/api/usuarios/register";

    void Start()
    {
        var root = GetComponent<UIDocument>()?.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("UIDocument no encontrado en RegisterManager.");
            return;
        }

        nameTextField = root.Q<TextField>("username");
        emailTextField = root.Q<TextField>("email");
        passTextField = root.Q<TextField>("password");
        registerButton = root.Q<Button>("registerButton");
        registerLabel = root.Q<Label>("registerLabel");

        if (registerButton != null)
        {
            registerButton.clicked += OnRegisterClicked;
        }
    }

    private void OnRegisterClicked()
    {
        string name = nameTextField.value;
        string email = emailTextField.value;
        string password = passTextField.value;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            registerLabel.text = "Por favor, completa todos los campos";
        }
        else
        {
            registerButton.SetEnabled(false);
            registerLabel.text = "Registrando...";
            StartCoroutine(RegisterRequest(name, email, password));
        }
    }

    private IEnumerator RegisterRequest(string name, string email, string password)
    {
        string jsonData = JsonUtility.ToJson(new RegisterData(name, email, password));
        byte[] jsonToSend = System.Text.Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(REGISTER_URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        registerButton.SetEnabled(true);

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Registro exitoso: " + request.downloadHandler.text);
            registerLabel.text = "Registro exitoso";

            // Disparar el evento de éxito
            OnRegisterSuccess?.Invoke();
        }
        else
        {
            Debug.LogError("Error en el registro: " + request.error);
            registerLabel.text = "Error al registrar el usuario";
        }
    }
}


// Clase para enviar los datos del registro
[System.Serializable]
public class RegisterData
{
    public string name;
    public string email;
    public string password;
    public int speed = 5;
    public int xp = 0;
    public int health = 30;
    public int damage = 10;
    public string arma = "espada";
    public string skinName = "default"; // Agregar un valor predeterminado
    public string shop = "Tienda1"; // Se asume que es un string por simplicidad

    public RegisterData(string name, string email, string password)
    {
        this.name = name;
        this.email = email;
        this.password = password;
    }
}
