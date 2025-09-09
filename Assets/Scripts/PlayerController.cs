using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public InputSystem_Actions inputActions;
    public Rigidbody rb;
    public Vector2 MoveInput;

    [SerializeField] public float MoveSpeed =5f; //移動スピード
    [SerializeField] public Transform CameraTransform;//カメラの向き取得
    void Start()
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

        Vector3 MoveDir = CamForward * CamForward.y + CamRight * CamRight.y;//進む方向




    }

}
