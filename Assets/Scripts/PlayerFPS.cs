using Mirror;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerFPS : NetworkBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 6f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;

    [Header("Cámara FP")]
    public Camera fpCamera;         // asigna FP_Camera
    public float mouseSensitivity = 120f;
    public Transform pitchPivot;    // asigna FP_Pivot (padre de la cámara)

    CharacterController controller;
    float verticalVel = 0f;
    float yaw;   // rotación horizontal del cuerpo
    float pitch; // rotación vertical (cámara)

    NetworkHealth health;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<NetworkHealth>();
    }

    public override void OnStartLocalPlayer()
    {
        // Activar cámara solo en el jugador local
        if (fpCamera != null)
            fpCamera.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        // Apagar cámara en jugadores remotos
        if (!isLocalPlayer && fpCamera != null)
            fpCamera.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // --- Mouse look ---
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw   += mx;
        pitch -= my;
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (pitchPivot != null)
            pitchPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // --- Movimiento WASD ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v).normalized;

        Vector3 moveDir = transform.right * input.x + transform.forward * input.z;

        // --- Gravedad + salto ---
        if (controller.isGrounded && verticalVel < 0f)
            verticalVel = -2f;

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
            verticalVel = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVel += gravity * Time.deltaTime;

        Vector3 velocity = moveDir * moveSpeed + Vector3.up * verticalVel;
        controller.Move(velocity * Time.deltaTime);

        if (transform.position.y < -5f) health.ServerHandleDeath();

    }

    // Llamado por NetworkHealth al respawnear
    public void ResetVerticalVelocity()
    {
        verticalVel = 0f;
    }
}