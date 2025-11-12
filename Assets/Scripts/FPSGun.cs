using Mirror;
using UnityEngine;

public class FPSGun : NetworkBehaviour
{
    [Header("Refs")]
    public Camera fpCamera;      // referencia a la misma FP camera del Player local
    public Transform muzzle;     // punto de salida (para efectos visuales)
    public LayerMask hitMask;    // capas que puede golpear
    public GunVisual visuals;    // hacemos referencia a el Script de Efectos

    [Header("Disparo")]
    public float fireRate = 8f;  // disparos por segundo
    public float range = 100f;
    public int damage = 20;

    float nextFireTime = 0f;

    void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + (1f / fireRate);

            // Origen/dirección desde la cámara local
            Vector3 origin = fpCamera.transform.position;
            Vector3 dir = fpCamera.transform.forward;
            CmdFire(origin, dir);
            // Opcional: puedes reproducir un pequeño feedback local inmediato
        }
    }

    [Command]
    void CmdFire(Vector3 origin, Vector3 direction)
    {
        // Servidor hace raycast autoritativo
        if (Physics.Raycast(origin, direction, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            // Aplicar daño si hay NetworkHealth
            var nh = hit.collider.GetComponentInParent<NetworkHealth>();
            if (nh != null)
                nh.TakeDamage(damage);

            // Avisar a todos para efectos (tracer/impacto)
            RpcOnFire(origin, hit.point);
        }
        else
        {
            RpcOnFire(origin, origin + direction * range);
        }
    }

    [ClientRpc]
    void RpcOnFire(Vector3 origin, Vector3 hitPoint)
    {
        visuals.PlayShotEffect(origin, hitPoint);
        // Efecto mínimo: dibujar un rayo (debug) o instanciar un tracer local
        //Debug.DrawLine(origin, hitPoint, Color.yellow, 0.2f);
        // Si quieres un muzzle flash local:
        // if (muzzle) { /* play particle or flash */ }
        // Si quieres chispas en el impacto:
        // Instantiate(impactVfxPrefab, hitPoint, Quaternion.identity); // <- si lo haces en red, usa NetworkServer.Spawn desde el server
    }
    //no me gusta balatro atte dorlejan
}