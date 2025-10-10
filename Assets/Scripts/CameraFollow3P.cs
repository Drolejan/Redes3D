using UnityEngine;
using Mirror;

public class CameraFollow3P : NetworkBehaviour
{
    public Transform target;         // arrastra aquí el cubo (player)
    public Vector3 offset = new Vector3(0f, 4f, -6f);
    public float followSmooth = 8f;  // suavizado de seguimiento
    public bool lookAtTarget = true; // que mire al jugador
    public Camera cam;
    public AudioListener audioL;

    private void Start()
    {
        if (!isLocalPlayer)
        { 
            cam.enabled = false;
            audioL.enabled = false;
        }
    }

    public override void OnStartLocalPlayer()
    {
        cam.enabled = true;
        audioL.enabled = true;
    }

    public void getTarget(Transform t)
    {
        target = t;//Asignamos el target cuando el player lo llama
    }

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSmooth * Time.deltaTime);

        if (lookAtTarget)
            transform.LookAt(target.position + Vector3.up * 1.2f);
    }
}