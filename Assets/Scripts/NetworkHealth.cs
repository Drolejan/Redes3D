using Mirror;
using UnityEngine;

public class NetworkHealth : NetworkBehaviour
{
    [Header("Vida")]
    public int maxHealth = 100;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public int health = 100;

    public override void OnStartServer()
    {
        // Vida inicial en el servidor
        health = maxHealth;
    }

    // Solo el servidor puede aplicar daño
    [Server]
    public void TakeDamage(int amount)
    {
        if (health <= 0) return; // ya estaba "muerto"

        health = Mathf.Max(health - amount, 0);

        if (health == 0)
            ServerHandleDeath();
    }

    // Se ejecuta en TODOS los clientes cuando cambia la vida
    void OnHealthChanged(int oldV, int newV)
    {
        // Aquí podrías actualizar HUD local si quieres
    }

    [Server]
    public void ServerHandleDeath()
    {
        // 1) Buscar un punto de respawn aleatorio en la escena
        Vector3 respawnPos;
        Quaternion respawnRot;
        GetRandomSpawn(out respawnPos, out respawnRot);

        // 2) Resetear vida en el servidor
        health = maxHealth;

        // 3) Pedir al cliente dueño (host o client) que se teletransporte localmente
        TargetRespawn(connectionToClient, respawnPos, respawnRot);
    }

    // Elegir NetworkStartPosition aleatorio (solo en server)
    [Server]
    void GetRandomSpawn(out Vector3 pos, out Quaternion rot)
    {
        NetworkStartPosition[] spawns = Object.FindObjectsByType<NetworkStartPosition>(FindObjectsSortMode.None);

        if (spawns != null && spawns.Length > 0)
        {
            NetworkStartPosition sp = spawns[Random.Range(0, spawns.Length)];
            pos = sp.transform.position;
            rot = sp.transform.rotation;
        }
        else
        {
            // Fallback por si no hay spawns
            pos = Vector3.up * 1f;
            rot = Quaternion.identity;
            Debug.LogWarning("[SERVER] No hay NetworkStartPosition en la escena, usando (0,1,0).");
        }
    }

    // Se ejecuta SOLO en el cliente dueño de este objeto (incluye al host)
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