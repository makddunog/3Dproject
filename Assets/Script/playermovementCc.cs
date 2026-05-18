using UnityEngine;
using Fusion;

public class playermovementCc : NetworkBehaviour
{
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float runSpeed = 7f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpHeight = 2f;
    [SerializeField] float mouseSpeed = 1.5f;

    float xRot;
    Vector3 velocity;
    Transform camTr;
    CharacterController cc;

    public override void Spawned()
    {
        cc = GetComponent<CharacterController>();

        Camera cam = GetComponentInChildren<Camera>(true);

        if (cam == null)
        {
            Debug.LogError("Ghost1 안에서 Camera를 못 찾음");
            return;
        }

        camTr = cam.transform;

        bool isMine = Object.HasInputAuthority;
        cam.gameObject.SetActive(isMine);

        if (isMine)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Debug.Log($"Spawned: {gameObject.name}, IsMine: {isMine}");
    }

    void Update()
    {
        if (!Object.HasInputAuthority)
            return;

        Look();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority)
            return;

        Move();
    }

    void Move()
    {
        if (cc == null)
            return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        float curSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        Vector3 moveDir = transform.right * h + transform.forward * v;

        if (moveDir.magnitude > 1f)
            moveDir.Normalize();

        if (cc.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetKey(KeyCode.Space) && cc.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Runner.DeltaTime;

        Vector3 finalMove = moveDir * curSpeed + Vector3.up * velocity.y;

        cc.Move(finalMove * Runner.DeltaTime);
    }

    void Look()
    {
        if (camTr == null)
            return;

        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSpeed;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSpeed;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -80f, 80f);

        camTr.localRotation = Quaternion.Euler(xRot, 0f, 0f);

        // 좌우 회전
        transform.Rotate(0f, mouseX, 0f);
    }
}