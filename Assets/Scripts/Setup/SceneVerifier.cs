using UnityEngine;

public class SceneVerifier : MonoBehaviour
{
    void Start()
    {
        // Find all PlayerController components
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        
        if (players.Length > 1)
        {
            Debug.LogError($"Multiple Players found ({players.Length}). Disable MapController.AutoSpawn or remove scene Player.");
            
            // Log details about each player found
            for (int i = 0; i < players.Length; i++)
            {
                Debug.LogError($"Player {i + 1}: {players[i].name} at {players[i].transform.position}");
            }
        }
        else if (players.Length == 0)
        {
            Debug.LogWarning("No PlayerController found in scene. This might be intentional if using MapController.AutoSpawn.");
        }
        else
        {
            Debug.Log($"Scene verification passed: Found {players.Length} player(s).");
        }
    }
}
