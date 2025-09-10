using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public InputSystem_Actions inputActions;
    public Rigidbody rb;
    public Vector2 MoveInput;
    public Transform Player;

    [SerializeField] public float MoveSpeed = 5f; //移動スピード
    [SerializeField] public Transform CameraTransform;//カメラの向き取得
    [SerializeField] public float RotateDegPerSec = 720f; //回転スピード

    public void Awake()
    {
        inputActions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; //XとZは固定　Yは回転する
    }

    public void OnEnable()
    {
        inputActions.Player.Enable();
    }

    public void FixedUpdate()
    {
        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>(); //inputaction move取得

        Vector3 CamForward = CameraTransform.forward; //前方向の座標取得
        CamForward.y = 0f;
        CamForward.Normalize();

        Vector3 CamRight = CameraTransform.right; //横方向の座標取得
        CamRight.y = 0f;
        CamRight.Normalize();

        Vector3 MoveDir = CamForward * moveInput.y + CamRight * moveInput.x; //進む方向

        // ↓ transformを直接動かすのはRigidbodyと競合するのでコメントアウト
        // transform.position += MoveDir.normalized * MoveSpeed * Time.deltaTime; //進む挙動

        Vector3 nextPos = rb.position + MoveDir.normalized * MoveSpeed * Time.fixedDeltaTime; //次の位置
        rb.MovePosition(nextPos); //Rigidbodyで移動

        if (MoveDir.magnitude > 0.0001f)
        {
            // ↓ これもrb.positionを直接書き換えると物理と競合するのでコメントアウト
            // Vector3 PlayerPos = rb.position + MoveDir.normalized * Time.deltaTime;
            // rb.position = PlayerPos;

            //移動方向にキャラを回転（向きを変える）
            Quaternion PlayerRot = Quaternion.LookRotation(MoveDir, Vector3.up); //進行方向を向く回転
            Quaternion MoveRot = Quaternion.RotateTowards(
                rb.rotation, PlayerRot, RotateDegPerSec * Time.fixedDeltaTime //スムーズに回転
            );
            rb.MoveRotation(MoveRot); //Rigidbodyで回転
        }
    }
}
