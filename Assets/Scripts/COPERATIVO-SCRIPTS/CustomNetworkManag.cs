/*using UnityEngine;
using Mirror;

public class CustomNetworkManager : NetworkManager
{
    public Transform hostSpawnPoint;  // Asigna la posici�n de spawn para el host
    public Transform clientSpawnPoint; // Asigna la posici�n de spawn para los clientes

    // Sobrescribir la funci�n OnServerAddPlayer de la clase NetworkManager
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // Primero, llamamos al m�todo base para agregar al jugador
        GameObject player = Instantiate(playerPrefab);  // Instanciamos el prefab del jugador

        // Comprobamos si este es el primer jugador (host)
        if (numPlayers == 1)
        {
            // Si es el host, lo colocamos en la posici�n de host
            player.transform.position = hostSpawnPoint.position;
        }
        else
        {
            // Si no es el host, lo colocamos en la posici�n de cliente
            player.transform.position = clientSpawnPoint.position;
        }

        // Asignamos el jugador al objeto de conexi�n y lo registramos en la red
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
*/