using Mirror;
using UnityEngine;

public class NetworkHealth : NetworkBehaviour
{
    [Header("Vida")]
    public int maxHealth = 100;

    [SyncVar(hook = nameof(OnHealthChanged))]
    public int health = 100;

    // Spawn propio de este jugador
    Vector3 spawnPos;
    Quaternion spawnRot;

    public override void OnStartServer()
    {
        // Guardamos dónde apareció originalmente este player
        spawnPos = transform.position;
        spawnRot = transform.rotation;

        health = maxHealth;
    }

    [Server]
    public void TakeDamage(int amount)
    {
        if (health <= 0) return; // ya está "muerto"

        health = Mathf.Max(health - amount, 0);
        Debug.Log($"[SERVER] {netId} tomó daño, health={health}");

        if (health == 0)
            ServerHandleDeath();
    }

    void OnHealthChanged(int oldV, int newV)
    {
        Debug.Log($"[CLIENT] netId={netId} health {oldV} -> {newV}, isServer={isServer}, isLocal={isLocalPlayer}");
        // Aquí solo HUD / efectos si quieres
    }

    [Server]
void ServerHandleDeath()
{
    Debug.Log($"[SERVER] {netId} murió, respawneando... pos antes = {transform.position}");

    health = maxHealth;

    var cc = GetComponent<CharacterController>();
    if (cc != null)
    {
        cc.enabled = false;
        transform.SetPositionAndRotation(spawnPos, spawnRot);
        cc.enabled = true;
    }
    else
    {
        transform.SetPositionAndRotation(spawnPos, spawnRot);
    }

    Debug.Log($"[SERVER] {netId} pos después = {transform.position}");
}
}