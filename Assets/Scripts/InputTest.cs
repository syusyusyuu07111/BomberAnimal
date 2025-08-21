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
    public float moveSpeed = 5f;          // 実移動速度
    private Vector2 moveInput;

    [Header("Jump")]
    public float jumpForce = 5f;
    public Transform groundCheck;         // 未設定でもOK（フォールバックあり）
    public float groundRadius = 0.2f;
    public LayerMask groundMask = ~0;
    private bool jumpQueued;

    [Header("BlendTree (Idle<->Move)")]
    public string blendParam = "Blend";   // Animator 1D BlendTree のパラメータ名（Idle=0, Move=1）
    public float maxSpeedForBlend = 5f;   // Blend=1 とみなす速度（通常は moveSpeed と同値）
    public float blendDampTime = 0.1f;    // Blend の平滑時間
    public float speedSnapEpsilon = 0.05f;// これ未満は0扱い（ガタつき防止）

    [Header("Camera-Relative Move / Facing")]
    public Transform cam;                 // 未設定なら Awake で Camera.main
    public float rotateSpeed = 720f;      // 移動中の回転速度[deg/s]
    public bool autoFaceCameraWhenIdle = true; // 停止中もカメラ向きへ体を向ける
    public float idleFaceSpeed = 360f;    // 停止中の回転速度[deg/s]
    public float moveFaceThreshold = 0.05f; // これ以上の入力で「移動中」判定

    // 内部
    private Vector3 lastRbPos;
    private float blendSmoothed;

    private void Awake()
    {
        inputAction = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // 物理で倒れないように X/Z 回転を固定
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (cam == null && Camera.main != null) cam = Camera.main.transform;
        if (maxSpeedForBlend <= 0f) maxSpeedForBlend = moveSpeed;

        lastRbPos = rb.position;
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
        float dt = Time.fixedDeltaTime;

        // --- 接地判定（未設定でも動くフォールバック） ---
        Vector3 checkPos = groundCheck ? groundCheck.position : transform.position + Vector3.down * 0.6f;
        bool isGrounded = Physics.CheckSphere(checkPos, groundRadius, groundMask, QueryTriggerInteraction.Ignore);

        // --- カメラ基準の移動ベクトル ---
        Vector3 f = cam ? Flat(cam.forward) : Flat(transform.forward);
        Vector3 r = cam ? Flat(cam.right) : Flat(transform.right);
        Vector3 moveWorld = f * moveInput.y + r * moveInput.x;

        // --- 位置移動 ---
        rb.MovePosition(rb.position + moveWorld * moveSpeed * dt);

        // 入力ほぼゼロなら水平速度を止める（任意）
        if (moveInput.sqrMagnitude < 0.0001f)
        {
            var v = rb.linearVelocity; // Unity 6
            v.x = 0f; v.z = 0f;
            rb.linearVelocity = v;
        }

        // --- 向き（回転）：移動中は進行方向、停止中はオプションでカメラ向き ---
        bool isMoving = moveInput.magnitude > moveFaceThreshold;
        Vector3 targetFwd = isMoving ? (moveWorld.sqrMagnitude > 0.000001f ? moveWorld.normalized : Flat(transform.forward))
                                     : (autoFaceCameraWhenIdle ? f : Flat(transform.forward));

        Quaternion targetRot = Quaternion.LookRotation(targetFwd, Vector3.up);
        float turnSpeed = isMoving ? rotateSpeed : idleFaceSpeed;
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, turnSpeed * dt));

        // --- Blend 値更新（実速度ベース） ---
        Vector3 delta = rb.position - lastRbPos;
        delta.y = 0f;
        float planarSpeed = delta.magnitude / Mathf.Max(dt, 0.0001f);
        if (planarSpeed < speedSnapEpsilon) planarSpeed = 0f;

        float blendTarget = Mathf.Clamp01(maxSpeedForBlend > 0f ? planarSpeed / maxSpeedForBlend : 0f);
        float k = 1f - Mathf.Exp(-(1f / Mathf.Max(0.0001f, blendDampTime)) * dt);
        blendSmoothed = Mathf.Lerp(blendSmoothed, blendTarget, k);
        animator.SetFloat(blendParam, blendSmoothed);

        lastRbPos = rb.position;

        // --- ジャンプ ---
        if (jumpQueued && isGrounded)
        {
            var v = rb.linearVelocity; v.y = 0f; rb.linearVelocity = v;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        jumpQueued = false;
    }

    private static Vector3 Flat(Vector3 v) // 水平面に投影して正規化
    {
        Vector3 p = Vector3.ProjectOnPlane(v, Vector3.up);
        return p.sqrMagnitude > 0.000001f ? p.normalized : Vector3.forward;
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
