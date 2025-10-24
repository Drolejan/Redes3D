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
    public Camera fpCamera;         // cámara en hijo del player
    public float mouseSensitivity = 120f;
    public Transform pitchPivot;    // un transform hijo donde rota el pitch (suele ser el padre de la cámara)
    
    CharacterController controller;
    float verticalVel = 0f;
    float yaw;   // rotación horizontal (cuerpo)
    float pitch; // rotación vertical (solo cámara)

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnStartLocalPlayer()
    {
        // Activar cámara local y capturar cursor
        if (fpCamera) fpCamera.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        // En clientes remotos, apagar cámara
        if (!isLocalPlayer && fpCamera)
            fpCamera.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // Mouse look
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mx;
        pitch -= my;
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        // Aplicar rotaciones
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (pitchPivot) pitchPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // Movimiento WASD relativo al yaw
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v).normalized;

        Vector3 moveDir = transform.right * input.x + transform.forward * input.z;

        // Gravedad/salto
        if (controller.isGrounded && verticalVel < 0f) verticalVel = -2f;
        if (controller.isGrounded && Input.GetButtonDown("Jump"))
            verticalVel = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVel += gravity * Time.deltaTime;

        Vector3 velocity = moveDir * moveSpeed + Vector3.up * verticalVel;
        controller.Move(velocity * Time.deltaTime);
    }
}