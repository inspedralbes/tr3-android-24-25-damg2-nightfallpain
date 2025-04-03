using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public UIDocument loginAndRegister;
    public UIDocument login;
    public UIDocument register;

    public LoginManager loginManager;
    public RegisterManager registerManager; // Asignado dinámicamente en Start()

    private void Start()
    {
        if (!ValidateReferences()) return;

        // Suscribirse al evento de RegisterManager
        registerManager.OnRegisterSuccess += OnRegisterSuccess;

        SwitchUI(0);

        Button switchButtonLogin = loginAndRegister.rootVisualElement.Q<Button>("ButtonLogin");
        Button switchButtonRegister = loginAndRegister.rootVisualElement.Q<Button>("ButtonRegister");

        if (switchButtonLogin != null)
        {
            switchButtonLogin.clicked += () => SwitchUI(1);
        }

        if (switchButtonRegister != null)
        {
            switchButtonRegister.clicked += () => SwitchUI(2);
        }
    }

    private bool ValidateReferences()
    {
        if (loginAndRegister == null || login == null || register == null ||
            loginManager == null || registerManager == null)
        {
            Debug.LogError("Error: Asegúrate de asignar todos los UIDocument y managers en el Inspector.");
            return false;
        }
        return true;
    }

    private void SwitchUI(int uiNumber)
    {
        ShowUI(loginAndRegister, uiNumber == 0);
        ShowUI(login, uiNumber == 1);
        ShowUI(register, uiNumber == 2);

        loginManager.enabled = (uiNumber == 1);
        registerManager.enabled = (uiNumber == 2);
    }

    private void ShowUI(UIDocument uiDoc, bool show)
    {
        if (uiDoc != null) uiDoc.gameObject.SetActive(show);
    }

    // Método que se llama cuando el registro es exitoso
    private void OnRegisterSuccess()
    {
        Debug.Log("Registro exitoso, cambiando a la pantalla de Login...");
        SwitchUI(1);
    }
}
