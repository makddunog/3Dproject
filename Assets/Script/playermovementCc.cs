using UnityEngine;
using Fusion;

public class playermovementCc : NetworkBehaviour
{
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float runSpeed = 7f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpHeight = 2f;
    [SerializeField] float mouseSpeed = 3.0f;

    float xRot;
    float yRot;
    Vector3 velocity;

    Transform camTr;
    CharacterController cc;
    Camera playerCamera;
    AudioListener audioListener;

    public override void Spawned()
    {
        cc = GetComponent<CharacterController>();
        SetupCamera();

        bool isMine = Object.HasInputAuthority;

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(isMine);
            playerCamera.enabled = isMine;
        }

        if (audioListener != null)
        {
            audioListener.enabled = isMine;
        }

        if (isMine)
        {
            yRot = transform.eulerAngles.y;
            LockCursor();
        }

        Debug.Log($"Spawned: {gameObject.name}, InputAuthority: {Object.HasInputAuthority}, StateAuthority: {Object.HasStateAuthority}");
    }

    void OnEnable()
    {
        SetupCamera();

        if (Object != null && Object.HasInputAuthority)
        {
            LockCursor();
        }
    }

    void Update()
    {
        if (!Object.HasInputAuthority)
            return;

        if (camTr == null || playerCamera == null)
        {
            SetupCamera();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            LockCursor();
        }
    }

    public override void FixedUpdateNetwork()
    {
        /*
         * 중요:
         * HasInputAuthority가 아니라 GetInput을 기준으로 움직인다.
         * 이렇게 해야 Host/StateAuthority 쪽에서 위치가 갱신되고,
         * NetworkTransform을 통해 상대방 화면에도 반영된다.
         */
        if (GetInput<PlayerInputData>(out PlayerInputData input))
        {
            ApplyLook(input.look);
            Move(input);
        }
    }

    void SetupCamera()
    {
        playerCamera = GetComponentInChildren<Camera>(true);

        if (playerCamera == null)
        {
            Debug.LogError("Ghost1 안에서 Camera를 못 찾음");
            return;
        }

        camTr = playerCamera.transform;
        audioListener = playerCamera.GetComponent<AudioListener>();

        if (Object != null)
        {
            bool isMine = Object.HasInputAuthority;

            playerCamera.gameObject.SetActive(isMine);
            playerCamera.enabled = isMine;

            if (audioListener != null)
            {
                audioListener.enabled = isMine;
            }
        }
    }

    void Move(PlayerInputData input)
    {
        if (cc == null)
        {
            cc = GetComponent<CharacterController>();
        }

        if (cc == null)
            return;

        float h = input.move.x;
        float v = input.move.y;

        bool isRunning = input.buttons.IsSet(PlayerButtons.Run);
        bool isJumping = input.buttons.IsSet(PlayerButtons.Jump);

        float curSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 moveDir = transform.right * h + transform.forward * v;

        if (moveDir.magnitude > 1f)
            moveDir.Normalize();

        if (cc.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (isJumping && cc.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Runner.DeltaTime;

        Vector3 finalMove = moveDir * curSpeed + Vector3.up * velocity.y;

        cc.Move(finalMove * Runner.DeltaTime);
    }

    void ApplyLook(Vector2 lookInput)
    {
        float mouseX = lookInput.x * mouseSpeed;
        float mouseY = lookInput.y * mouseSpeed;

        yRot += mouseX;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -80f, 80f);

        // 몸 좌우 회전: 네트워크로 동기화될 회전
        transform.rotation = Quaternion.Euler(0f, yRot, 0f);

        // 카메라 위아래 회전: 내 화면에서만 필요
        if (Object.HasInputAuthority && camTr != null)
        {
            camTr.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        }
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}