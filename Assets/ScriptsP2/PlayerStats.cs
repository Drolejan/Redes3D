using Mirror;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnScoreChanged))]
    public int score;

    // Opcional: un nombre simple para el scoreboard
    [SyncVar] public string displayName;

    public override void OnStartLocalPlayer()
    {
        // Si no manejan nombres, usamos el netId
        if (string.IsNullOrEmpty(displayName))
            CmdSetName($"Player {netId}");
    }

    [Command]
    void CmdSetName(string nameRequested)
    {
        displayName = nameRequested;
    }

    // API segura para sumar puntos: solo en servidor
    [Server]
    public void AddScore(int amount)
    {
        score += amount;
    }

    void OnScoreChanged(int oldValue, int newValue)
    {
        // Avisar a la UI (si existe)
        ScoreboardUI.Instance?.RefreshNow();
    }
}