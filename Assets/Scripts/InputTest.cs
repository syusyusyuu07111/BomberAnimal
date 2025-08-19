using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class InputTest : MonoBehaviour
{
    private InputSystem_Actions inputAction;
    private Rigidbody rb;

    [Header("Move")]
    public float moveSpeed = 5f;
    private Vector2 moveInput;

    [Header("Jump")]
    public float jumpForce = 5f;
    public Transform groundCheck;      // ���ݒ�ł�OK�i���Ńt�H�[���o�b�N�j
    public float groundRadius = 0.2f;
    public LayerMask groundMask = ~0;  // ����́u�S���v= �ݒ肵�Y��Ă��ڒn���肷��
    private bool jumpQueued;

    private void Awake()
    {
        inputAction = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void OnEnable()
    {
        inputAction.Enable();
        // �O�̂��ߌ�Enable
        inputAction.Player.Jump.Enable();
        inputAction.Player.Move.Enable();

        inputAction.Player.Jump.started += OnJump;  // ���O�p�� started ���E��
        inputAction.Player.Jump.performed += OnJump;
        inputAction.Player.Jump.canceled += OnJump;
    }

    private void OnDisable()
    {
        inputAction.Player.Jump.started -= OnJump;
        inputAction.Player.Jump.performed -= OnJump;
        inputAction.Player.Jump.canceled -= OnJump;
        inputAction.Disable();
    }

    private void Update()
    {
        moveInput = inputAction.Player.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        // --- �ڒn����i���ݒ�ł������t�H�[���o�b�N�j ---
        Vector3 checkPos = groundCheck ? groundCheck.position : transform.position + Vector3.down * 0.6f;
        bool isGrounded = Physics.CheckSphere(checkPos, groundRadius, groundMask, QueryTriggerInteraction.Ignore);

        // --- �ړ� ---
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);

        // --- �W�����v ---
        if (jumpQueued && isGrounded)
        {
            // �㉺���x�����Z�b�g���Ă���C���p���X
            var v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Debug.Log("�W�����v���s�I");
        }
        jumpQueued = false;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        // �������o�Ȃ��Ȃ�u�@���΂��Ă��Ȃ��v
        Debug.Log($"Jump {ctx.phase}"); // started / performed / canceled ���o��

        if (ctx.performed)
            jumpQueued = true; // ����FixedUpdate�ŏ���
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}
