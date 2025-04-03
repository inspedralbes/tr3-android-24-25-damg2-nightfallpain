using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace GameData
{
    public static class DataManager
    {
        // Lista para almacenar los datos de enemigos
        public static List<EnemyDataInfo> enemiesData = new List<EnemyDataInfo>();
        // Datos de la partida actual
        public static PartidaInfo currentPartida = null;
    }
    [System.Serializable]
    public class EnemyDataInfo
    {
        public string name;
        public int health;
        public float speed;
        public int damage;
        public string bulletName;
    }
    [System.Serializable]
    public class PartidaInfo
    {
        public string id;
        public string usuari_id;
        public string tipus_partida;
        public string createdAt;
    }
    [System.Serializable]
    public class EstadisticasInfo
    {
        public string usuari_id;
        public int temps;
    }
}