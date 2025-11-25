using System.Linq;
using Mirror;
using UnityEngine;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance;

    [Header("Configuración de partida")]
    public int maxKillsToWin = 5;       // kills necesarias para ganar
    public float matchDuration = 180f;  // segundos (3 minutos)

    [Header("Estado sincronizado")]
    [SyncVar] public bool matchEnded = false;
    [SyncVar] public float timeRemaining; // para mostrar tiempo en HUD si quieres

    double endTimeServer; // solo en server

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("NetworkGameManager: ya existe una instancia, destruyendo duplicado.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnStartServer()
    {
        // Definir cuándo termina la partida
        endTimeServer = NetworkTime.time + matchDuration;
        timeRemaining = matchDuration;
    }

    [ServerCallback]
    void Update()
    {
        if (matchEnded) return;

        // Actualizar tiempo restante
        timeRemaining = Mathf.Max(0f, (float)(endTimeServer - NetworkTime.time));

        if (timeRemaining <= 0f)
        {
            ServerEndMatchByTime();
        }
    }

    // Llamado por NetworkHealth cuando alguien suma kills
    [Server]
    public void ServerCheckKillWinCondition(NetworkHealth killer)
    {
        if (matchEnded || killer == null) return;

        if (killer.kills >= maxKillsToWin)
        {
            ServerEndMatchByKills(killer);
        }
    }

    [Server]
    void ServerEndMatchByKills(NetworkHealth winner)
    {
        matchEnded = true;
        string name = winner != null && !string.IsNullOrEmpty(winner.displayName)
            ? winner.displayName
            : $"Player {winner.netId}";

        Debug.Log($"[SERVER] Partida terminada por kills. Ganador: {name}");
        RpcEndMatch(name, true);
        ServerDisableAllPlayers();
    }

    [Server]
    void ServerEndMatchByTime()
    {
        matchEnded = true;

        // Buscar al que tenga más kills
        var players = FindObjectsOfType<NetworkHealth>().ToList();
        NetworkHealth winner = players.OrderByDescending(p => p.kills).FirstOrDefault();

        string name = (winner != null && !string.IsNullOrEmpty(winner.displayName))
            ? winner.displayName
            : (winner != null ? $"Player {winner.netId}" : "Nadie");

        Debug.Log($"[SERVER] Partida terminada por tiempo. Ganador: {name}");
        RpcEndMatch(name, false);
        ServerDisableAllPlayers();
    }

    [Server]
    void ServerDisableAllPlayers()
    {
        foreach (var p in FindObjectsOfType<NetworkHealth>())
        {
            p.canFight = false; // nuevo flag en NetworkHealth
        }
    }

    [ClientRpc]
    void RpcEndMatch(string winnerName, bool byKills)
    {
        string reason = byKills ? "por kills" : "por tiempo";
        Debug.Log($"[CLIENT] Partida terminada {reason}. Ganador: {winnerName}");

        // Aquí podrías mostrar un panel de victoria
        // por ahora solo lo dejamos en log
    }
}