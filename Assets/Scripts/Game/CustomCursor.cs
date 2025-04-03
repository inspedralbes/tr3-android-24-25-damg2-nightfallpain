using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    public Texture2D crosshairTexture;  // La textura de la mirilla
    //public Vector2 hotspot = new Vector2(16, 16);  // El punto de anclaje de la mirilla (aj�stalo seg�n tu textura)
    public Transform player;  // Referencia al transform del jugador

    void Start()
    {
        // Cambiar el cursor al inicio del juego
        Vector2  cursorHotspot = new Vector2(crosshairTexture.width / 2, crosshairTexture.height / 2);
        Cursor.SetCursor(crosshairTexture, cursorHotspot, CursorMode.Auto);
        //Cursor.SetCursor(crosshairTexture, hotspot, CursorMode.ForceSoftware);
        Cursor.lockState = CursorLockMode.Confined;  // Mantener el cursor dentro de la ventana del juego
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        // Sincronizar la direcci�n del disparo con la posici�n del cursor
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mousePosition.z = player.position.z;  // Asegurarse de que el Z est� alineado con el jugador

        Vector3 direction = mousePosition - player.position;
        player.right = direction;  // Asegurarse de que el jugador apunte en la direcci�n del cursor

        // Si quieres que el cursor vuelva al normal cuando dejas de disparar (opcional)
        if (Input.GetKeyDown(KeyCode.Escape))  // Aqu� lo hacemos con Escape, pero puedes usar cualquier condici�n
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}