using Mirror;
using UnityEngine;

public class FPSGun : NetworkBehaviour
{
    [Header("Refs")]
    public Camera fpCamera;      // la cámara FP del jugador local
    public Transform muzzle;     // el transform Muzzle
    public LayerMask hitMask;
    public GunVisual visuals;    // <-- NUEVO, referencia al script GunVisual

    [Header("Disparo")]
    public float fireRate = 8f;  // balas por segundo
    public float range = 100f;
    public int damage = 20;

    float nextFireTime = 0f;

    void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + (1f / fireRate);

            // calculamos desde la cámara local
            Vector3 origin = fpCamera.transform.position;
            Vector3 dir    = fpCamera.transform.forward;

            // Se lo pedimos al servidor
            CmdFire(origin, dir);
        }
    }

    [Command]
    void CmdFire(Vector3 origin, Vector3 direction)
    {
        Vector3 hitPoint = origin + direction * range;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            hitPoint = hit.point;

            // Aplicar daño si pegamos a alguien con NetworkHealth
            NetworkHealth nh = hit.collider.GetComponentInParent<NetworkHealth>();
            if (nh != null)
                nh.TakeDamage(damage);
        }

        // Avisar a TODOS los clientes para que muestren efectos visuales iguales
        RpcOnFire(muzzle.position, hitPoint);
    }

    [ClientRpc]
    void RpcOnFire(Vector3 muzzlePos, Vector3 hitPos)
    {
        // Efectos visibles para todos los jugadores
        if (visuals != null)
        {
            visuals.PlayShotEffect(muzzlePos, hitPos);
        }
    }
}