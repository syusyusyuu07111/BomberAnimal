using UnityEngine;

public class InputTest : MonoBehaviour
{
    private InputSystem_Actions inputAction;
    public float moveSpeed = 5f; // �ړ����x

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
        // �ړ����͂�ǂݎ�� (WASD / �X�e�B�b�N)
        Vector2 moveInput = inputAction.Player.Move.ReadValue<Vector2>();

        // ���͂����鎞�����ړ�
        if (moveInput != Vector2.zero)
        {
            Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
            transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}
