using UnityEngine;

public class playermovementCc : MonoBehaviour
{
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float runSpeed = 7f;

    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpHeight = 2f;

    [SerializeField] float mouseSpeed = 1.5f;

    float xRot;
    Vector3 velo;
    Transform camTr;

    CharacterController cc;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        camTr = Camera.main.transform;
    }

    void Update()
    {
        Move();
        Look();
    


    }
    // Update is called once per frame

    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float curSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        bool grounded = cc.isGrounded;
        if (grounded && velo.y < 0) velo.y = -2f;


        Vector3 movDir = transform.right * h + transform.forward * v;
        cc.Move(movDir * curSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && grounded) velo.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        velo.y += gravity * Time.deltaTime;

        cc.Move(velo * Time.deltaTime);
    }
    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSpeed;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        camTr.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
