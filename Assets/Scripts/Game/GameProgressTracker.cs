using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameProgressTracker : MonoBehaviour
{
    // Singleton pattern to persist between scenes
    public static GameProgressTracker Instance { get; private set; }

    // Player data to carry between scenes
    [Serializable]
    public class PlayerProgressData
    {
        public int Experience { get; set; }
        public float TotalGameTime { get; set; }
    }

    // Current player progress
    public PlayerProgressData CurrentProgress { get; private set; }

    // Timer for tracking total game time
    private float gameStartTime;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize progress
            CurrentProgress = new PlayerProgressData();
            gameStartTime = Time.time;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update total game time
    private void Update()
    {
        if (CurrentProgress != null)
        {
            CurrentProgress.TotalGameTime = Time.time - gameStartTime;
        }
    }

    // Method to update player progress when changing scenes
    public void UpdatePlayerProgress(PlayerController player)
    {
        if (player != null)
        {
            CurrentProgress.Experience = player.GetExperience();
        }
    }

    // Method to apply saved progress to a new player
    public void ApplyProgressToPlayer(PlayerController player)
    {
        if (player != null && CurrentProgress != null)
        {
            // Usar CurrentProgress.Experience para darle la experiencia al jugador
            player.AddExperience(CurrentProgress.Experience);

            // Siempre establecer munición al máximo
            player.ReloadAmmo();

            // Restaurar vida completa
            player.RestaurarVidaCompleta();
        }
    }
}
