using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class InputTest : MonoBehaviour
{
    private InputSystem_Actions inputAction;
    private Rigidbody rb;
    private Animator animator;

    [Header("Move")]
    public float moveSpeed = 5f;          // 実際の移動速度
    private Vector2 moveInput;

    [Header("Jump")]
    public float jumpForce = 5f;
    public Transform groundCheck;         // 未設定でもOK（フォールバックあり）
    public float groundRadius = 0.2f;
    public LayerMask groundMask = ~0;
    private bool jumpQueued;

    [Header("BlendTree (Idle<->Move)")]
    public string blendParam = "Blend";   // Animator 1D BlendTree のパラメータ名
    public float maxSpeedForBlend = 5f;   // Blend=1 とみなす速度（通常は moveSpeed と同値）
    public float blendDampTime = 0.1f;    // Blend の平滑時間
    public float speedSnapEpsilon = 0.05f;// これ未満の速度は0扱い（ガタつき防止）

    // 内部
    private Vector3 lastRbPos;            // ★ Rigidbody の前フレーム位置
    private float blendSmoothed;

    private void Awake()
    {
        inputAction = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // 物理で倒れないように X/Z 回転を固定
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (maxSpeedForBlend <= 0f) maxSpeedForBlend = moveSpeed;
        lastRbPos = rb.position;          // ★ ここも rb.position
    }

    private void OnEnable()
    {
        inputAction.Enable();
        inputAction.Player.Move.Enable();
        inputAction.Player.Jump.Enable();

        inputAction.Player.Jump.started += OnJump;
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
        // 入力（WASD/左スティック）
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

        // 入力ゼロなら水平速度をスナップ 0（揺れ防止・任意）
        if (moveInput.sqrMagnitude < 0.0001f)
        {
            var v = rb.linearVelocity; // Unity 6
            v.x = 0f; v.z = 0f;
            rb.linearVelocity = v;
        }

        // --- 実速度 → Blend 値（Idle 0 / Move 1） ---
        Vector3 delta = rb.position - lastRbPos;   // ★ transform ではなく rb.position
        delta.y = 0f;
        float planarSpeed = delta.magnitude / Mathf.Max(Time.fixedDeltaTime, 0.0001f);
        if (planarSpeed < speedSnapEpsilon) planarSpeed = 0f;

        float blendTarget = Mathf.Clamp01(maxSpeedForBlend > 0f ? planarSpeed / maxSpeedForBlend : 0f);

        // Damp（指数平滑）
        float k = 1f - Mathf.Exp(-(1f / Mathf.Max(0.0001f, blendDampTime)) * Time.fixedDeltaTime);
        blendSmoothed = Mathf.Lerp(blendSmoothed, blendTarget, k);
        animator.SetFloat(blendParam, blendSmoothed);

        lastRbPos = rb.position;          // ★ 更新

        // --- ジャンプ ---
        if (jumpQueued && isGrounded)
        {
            var v = rb.linearVelocity; v.y = 0f; rb.linearVelocity = v;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        jumpQueued = false;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) jumpQueued = true;
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
