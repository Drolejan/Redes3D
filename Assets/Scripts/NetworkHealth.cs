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
        // Aseguramos vida inicial correcta en el server
        health = maxHealth;
    }

    // Solo el servidor puede hacer daño
    [Server]
    public void TakeDamage(int amount)
    {
        if (health <= 0) return; // ya estaba muerto

        health = Mathf.Max(health - amount, 0);
        Debug.Log($"[SERVER] {netId} tomó daño, health={health}");

        if (health == 0)
            ServerHandleDeath();
    }

    // Hook: se dispara en TODOS los clientes cuando cambia la vida
    void OnHealthChanged(int oldV, int newV)
    {
        Debug.Log($"[CLIENT] netId={netId} health {oldV} -> {newV} isServer={isServer} isLocal={isLocalPlayer}");

        // Aquí SOLO actualiza UI / efectos locales
        // NUNCA hagas health = algo aquí.
    }

    [Server]
    void ServerHandleDeath()
    {
        Debug.Log($"[SERVER] {netId} murió, respawneando...");

        // 1) Elegir punto de respawn
        Transform start = NetworkManager.singleton.GetStartPosition();
        Vector3 pos = start ? start.position + Vector3.up * 1f : Vector3.up * 1f;
        Quaternion rot = start ? start.rotation : Quaternion.identity;

        // 2) Resetear vida en el server
        health = maxHealth;

        // 3) Teletransportar al jugador (y resetear controller)
        var cc = GetComponent<CharacterController>();
        if (cc)
        {
            cc.enabled = false;
            transform.SetPositionAndRotation(pos, rot);
            cc.enabled = true;
        }
        else
        {
            transform.SetPositionAndRotation(pos, rot);
        }
    }
}