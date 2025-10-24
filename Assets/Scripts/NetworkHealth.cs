using Mirror;
using UnityEngine;

public class NetworkHealth : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnHealthChanged))] public int health = 100;

    public void TakeDamage(int amount)
    {
        if (!isServer) return; // solo el servidor modifica el estado
        health = Mathf.Max(0, health - amount);
        if (health <= 0) OnDeath();
    }

    void OnHealthChanged(int oldV, int newV)
    {
        // Aquí podrías actualizar UI local si existiera
        // Debug.Log($"[{netId}] health {oldV}->{newV}");
    }

    [Server]
    void OnDeath()
    {
        // Respawn simple: teletransportar a origen
        Transform start = NetworkManager.singleton.GetStartPosition();
        Vector3 pos = start ? start.position + Vector3.up * 1f : Vector3.up * 1f;
        health = 100;

        var cc = GetComponent<CharacterController>();
        if (cc)
        {
            cc.enabled = false;
            transform.SetPositionAndRotation(pos, start ? start.rotation : Quaternion.identity);
            cc.enabled = true;
        }
        else
        {
            transform.SetPositionAndRotation(pos, start ? start.rotation : Quaternion.identity);
        }
    }
}