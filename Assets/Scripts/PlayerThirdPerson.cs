using UnityEngine;
using Mirror;

[RequireComponent(typeof(CharacterController))]
public class PlayerThirdPerson : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 12f; 
    public float jumpHeight = 2f;     // altura del salto
    public Transform cam;             // arrastra aquí la Main Camera

    CharacterController controller;
    float gravity = -9.81f;
    float verticalVel = 0f;

    [SyncVar(hook = nameof(CambioScore))]//Cada que la variable cambia, se llama la funcion
    public int score;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        //cam=Camera.main.GetComponent<Transform>();//Obtenemos la posición de la camara
        //cam.GetComponent<CameraFollow3P>().getTarget(this.transform);
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // Input crudo (WASD / flechas)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v).normalized;

        // Dirección relativa a la cámara (plano XZ)
        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight   = cam.right;

        Vector3 moveDir = (camForward * input.z + camRight * input.x).normalized;

        // Rotar el cubo hacia la dirección de movimiento
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Gravedad + salto
        if (controller.isGrounded && verticalVel < 0f)
            verticalVel = -2f; // valor pequeño para mantener al jugador en el suelo

        if (controller.isGrounded && Input.GetButtonDown("Jump")) // tecla por defecto = Espacio
        {
            verticalVel = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVel += gravity * Time.deltaTime;

        // Aplicar movimiento (horizontal + vertical)
        Vector3 velocity = moveDir * moveSpeed + Vector3.up * verticalVel;
        controller.Move(velocity * Time.deltaTime);
    }

    public void sumarPuntos()
    {
        if (!isLocalPlayer) return;//Si no es local no suma nada
        CmdSumarPuntos();//Mando llamar mi nueva función
    }

    [Command]//Funcion que desde el cliente activa algo en el servidor
    public void CmdSumarPuntos()
    {
        score++;
    }

    void CambioScore(int oldScore, int newScore)
    {
        ScoreboardUI.Instance.RefreshNow();
    }    
}