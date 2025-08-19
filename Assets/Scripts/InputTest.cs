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
    public Transform groundCheck;      // 未設定でもOK（下でフォールバック）
    public float groundRadius = 0.2f;
    public LayerMask groundMask = ~0;  // 既定は「全部」= 設定し忘れても接地判定する
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
        // 念のため個別Enable
        inputAction.Player.Jump.Enable();
        inputAction.Player.Move.Enable();

        inputAction.Player.Jump.started += OnJump;  // ログ用に started も拾う
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
        // --- 接地判定（未設定でも動くフォールバック） ---
        Vector3 checkPos = groundCheck ? groundCheck.position : transform.position + Vector3.down * 0.6f;
        bool isGrounded = Physics.CheckSphere(checkPos, groundRadius, groundMask, QueryTriggerInteraction.Ignore);

        // --- 移動 ---
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);

        // --- ジャンプ ---
        if (jumpQueued && isGrounded)
        {
            // 上下速度をリセットしてからインパルス
            var v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Debug.Log("ジャンプ実行！");
        }
        jumpQueued = false;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        // ここが出ないなら「①発火していない」
        Debug.Log($"Jump {ctx.phase}"); // started / performed / canceled が出る

        if (ctx.performed)
            jumpQueued = true; // 次のFixedUpdateで処理
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
