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

    }

}
