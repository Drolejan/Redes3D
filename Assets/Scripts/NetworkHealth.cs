using Mirror;
using UnityEngine;

public class NetworkHealth : NetworkBehaviour
{
    [Header("Vida")]
    public int maxHealth = 100;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public int health = 100;

    [Header("Score / Nombre")]
    [SyncVar(hook = nameof(OnKillsChanged))]
    public int kills = 0;

    [SyncVar]
    public string displayName;

    public override void OnStartServer()
    {
        // Vida inicial en el servidor
        health = maxHealth;
    }

    public override void OnStartLocalPlayer()
    {
        // Nombre por defecto; luego puedes cambiarlo por input de usuario
        CmdSetName($"Player {netId}");
    }

    [Command]
    void CmdSetName(string newName)
    {
        displayName = newName;
    }

    // Solo el servidor puede aplicar daño
    [Server]
    public void TakeDamage(int amount, NetworkIdentity attacker)
    {
        if (health <= 0) return; // ya estaba "muerto"

        health = Mathf.Max(health - amount, 0);
        Debug.Log($"[SERVER] netId={netId} tomó daño, health={health}");

        if (health == 0)
        {
            // Si hay atacante válido y no es suicidio, sumamos kill
            if (attacker != null)
            {
                var killerHealth = attacker.GetComponent<NetworkHealth>();
                if (killerHealth != null && killerHealth != this)
                {
                    killerHealth.kills++;
                }
            }

            ServerHandleDeath();
        }
    }

    // Se ejecuta en TODOS los clientes cuando cambia la vida
    void OnHealthChanged(int oldV, int newV)
    {
        Debug.Log($"[CLIENT] netId={netId} health {oldV}->{newV} isServer={isServer} isLocal={isLocalPlayer}");
        // Aquí podrías actualizar HUD local de vida
    }

    void OnKillsChanged(int oldV, int newV)
    {
        Debug.Log($"[CLIENT] netId={netId} kills {oldV}->{newV}");
        ScoreboardUI.Instance?.Refresh(); // refrescar tabla inmediatamente
    }

    // === MUERTE Y RESPAWN ===

    [Server]
    void ServerHandleDeath()
    {
        Debug.Log($"[SERVER] netId={netId} murió, respawneando...");

        // 1) Buscar un punto de respawn aleatorio
        Vector3 respawnPos;
        Quaternion respawnRot;
        GetRandomSpawn(out respawnPos, out respawnRot);

        // 2) Resetear vida en el servidor
        health = maxHealth;

        // 3) Pedir al cliente dueño que se teletransporte localmente
        TargetRespawn(connectionToClient, respawnPos, respawnRot);
    }

    [Server]
    void GetRandomSpawn(out Vector3 pos, out Quaternion rot)
    {
        NetworkStartPosition[] spawns = FindObjectsOfType<NetworkStartPosition>();

        if (spawns != null && spawns.Length > 0)
        {
            NetworkStartPosition sp = spawns[Random.Range(0, spawns.Length)];
            pos = sp.transform.position;
            rot = sp.transform.rotation;
        }
        else
        {
            pos = Vector3.up * 1f;
            rot = Quaternion.identity;
            Debug.LogWarning("[SERVER] No hay NetworkStartPosition en la escena, usando (0,1,0).");
        }
    }

    // Cliente dueño (host o client) hace el teleport real
    [TargetRpc]
    void TargetRespawn(NetworkConnectionToClient target, Vector3 pos, Quaternion rot)
    {
        Debug.Log($"[CLIENT TargetRespawn] netId={netId} respawn en {pos}");

        CharacterController cc = GetComponent<CharacterController>();
        PlayerFPS fps = GetComponent<PlayerFPS>();

        if (cc != null)
        {
            cc.enabled = false;
            transform.SetPositionAndRotation(pos, rot);
            cc.enabled = true;
        }
        else
        {
            transform.SetPositionAndRotation(pos, rot);
        }

        if (fps != null)
            fps.ResetVerticalVelocity();
    }
}