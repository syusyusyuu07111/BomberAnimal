using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public InputSystem_Actions inputActions;
    public Rigidbody rb;
    public Vector2 MoveInput;

    [SerializeField] public float MoveSpeed =5f; //移動スピード
    [SerializeField] public Transform CameraTransform;//カメラの向き取得
    public void Awake()
    {
        inputActions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
    }

    public void OnEnable()
    {
        inputActions.Player.Enable();
    }
    public void Update()
    {
        Vector2 MoveInput = inputActions.Player.Move.ReadValue<Vector2>();//inputaction move取得

        Vector3 CamForward = CameraTransform.forward;//前方向の座標取得
        CamForward.y = 0f;
        CamForward.Normalize();

        Vector3 CamRight = CameraTransform.right;//横方向の座標取得
        CamRight.y = 0f;
        CamRight.Normalize();

        Vector3 MoveDir = CamForward * MoveInput.y + CamRight * MoveInput.x;//進む方向

        transform.position += MoveDir.normalized * MoveSpeed * Time.deltaTime;//進む挙動

        if(MoveDir.magnitude>0.0001f)
        {
            Vector3 PlayerPos = rb.position + MoveDir.normalized * Time.deltaTime;
            rb.position = PlayerPos;
        }


    }

}
