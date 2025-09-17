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

    public bool CanJump;
    public bool IsGround;
    public float JumpPower = 10.0f;
    //オーディオ系==========================================================================================================
    [SerializeField] AudioSource JumpSource;
    [SerializeField] AudioSource MoveSource;
    [SerializeField] AudioClip Jump;
    [SerializeField] AudioClip Move;
    public void Awake()
    {
        inputActions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; //XとZは固定　Yは回転する
        CanJump = false;
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


        Vector3 nextPos = rb.position + MoveDir.normalized * MoveSpeed * Time.fixedDeltaTime; //次の位置
        rb.MovePosition(nextPos); //Rigidbodyで移動

        if (MoveDir.sqrMagnitude > 0.0001f)
        {
            //移動方向にキャラを回転（向きを変える)&移動--------------------------------------------------------------------------------------
            Quaternion PlayerRot = Quaternion.LookRotation(MoveDir, Vector3.up); //進行方向を向く回転
            Quaternion MoveRot = Quaternion.RotateTowards(rb.rotation, PlayerRot, RotateDegPerSec * Time.fixedDeltaTime);
            rb.MoveRotation(MoveRot); //Rigidbodyで回転
            //移動中に足音SE---------------------------------------------------------------------------------------------------------------------
            if (!MoveSource.isPlaying)
            {
                MoveSource.PlayOneShot(Move);
            }
        }

    }
    private void Update()
    {
        //ジャンプキー押されたらジャンプ-------------------------------------------------------------------------------------------------------------
        if (inputActions.Player.Jump.triggered && CanJump == true)
        {
            CanJump = false;
            rb.AddForce(transform.up * JumpPower, ForceMode.Impulse);
            JumpSource.PlayOneShot(Jump);
        }
    }
    //床に触れてたらフラグを切り替える-----------------------------------------------------------------------------------------------------------------
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            CanJump = true;
        }
    }
}
