/*using UnityEngine;
using Mirror;

public class CustomNetworkManager : NetworkManager
{
    public Transform hostSpawnPoint;  // Asigna la posición de spawn para el host
    public Transform clientSpawnPoint; // Asigna la posición de spawn para los clientes

    // Sobrescribir la función OnServerAddPlayer de la clase NetworkManager
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        // Primero, llamamos al método base para agregar al jugador
        GameObject player = Instantiate(playerPrefab);  // Instanciamos el prefab del jugador

        // Comprobamos si este es el primer jugador (host)
        if (numPlayers == 1)
        {
            // Si es el host, lo colocamos en la posición de host
            player.transform.position = hostSpawnPoint.position;
        }
        else
        {
            // Si no es el host, lo colocamos en la posición de cliente
            player.transform.position = clientSpawnPoint.position;
        }

        // Asignamos el jugador al objeto de conexión y lo registramos en la red
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
*/