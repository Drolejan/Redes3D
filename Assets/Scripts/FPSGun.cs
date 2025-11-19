using Mirror;
using UnityEngine;

public class FPSGun : NetworkBehaviour
{
    [Header("Refs")]
    public Camera fpCamera;      // cámara FP del jugador local
    public Transform muzzle;     // punto de salida del disparo
    public LayerMask hitMask;    // capas a las que puede pegar
    public GunVisual visuals;    // opcional, efectos de disparo

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

            Vector3 origin = fpCamera.transform.position;
            Vector3 dir    = fpCamera.transform.forward;

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

            // Evitar auto-hit: si le pegamos a nuestro propio NetworkIdentity, ignoramos daño
            NetworkIdentity hitId = hit.collider.GetComponentInParent<NetworkIdentity>();
            NetworkIdentity myId  = GetComponent<NetworkIdentity>();

            if (hitId != null && hitId == myId)
            {
                // Solo efectos visuales, sin daño
                RpcOnFire(muzzle.position, hitPoint);
                return;
            }

            // Aplicar daño si hay NetworkHealth
            NetworkHealth nh = hit.collider.GetComponentInParent<NetworkHealth>();
            if (nh != null)
            {
                nh.TakeDamage(damage);
            }
        }

        // Efectos visuales para TODOS los clientes
        RpcOnFire(muzzle.position, hitPoint);
    }

    [ClientRpc]
    void RpcOnFire(Vector3 muzzlePos, Vector3 hitPos)
    {
        if (visuals != null)
        {
            visuals.PlayShotEffect(muzzlePos, hitPos);
        }
        else
        {
            // Fallback mínimo: línea debug
            Debug.DrawLine(muzzlePos, hitPos, Color.yellow, 0.2f);
        }
    }
}