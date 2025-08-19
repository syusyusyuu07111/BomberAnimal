using UnityEngine;

public class InputTest : MonoBehaviour
{
    private InputSystem_Actions inputAction;
    public float moveSpeed = 5f; // 移動速度

    private void Awake()
    {
        inputAction = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputAction.Enable();
    }

    private void OnDisable()
    {
        inputAction.Disable();
    }

    void Update()
    {
        // 移動入力を読み取る (WASD / スティック)
        Vector2 moveInput = inputAction.Player.Move.ReadValue<Vector2>();

        // 入力がある時だけ移動
        if (moveInput != Vector2.zero)
        {
            Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
            transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}
