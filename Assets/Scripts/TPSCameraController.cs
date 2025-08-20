using UnityEngine;
using UnityEngine.InputSystem; // �VInputSystem���g��

public class TPSCameraController : MonoBehaviour
{
    [Header("�^�[�Q�b�g")]
    public Transform target;

    [Header("�����E����")]
    public float distance = 5f;
    public float minDistance = 0.5f;
    public float height = 2f;

    [Header("���x")]
    public float sensitivityX = 120f;
    public float sensitivityY = 120f;
    public float minPitch = -20f;
    public float maxPitch = 70f;

    [Header("�J�����R���W����")]
    public float collisionRadius = 0.25f;
    public float skin = 0.05f;
    public LayerMask collisionMask = ~0;

    private float yaw, pitch;
    private float currentDistance;
    private InputSystem_Actions input; // �� ������InputAction���g��
    private Camera cam;

    private void Awake()
    {
        input = new InputSystem_Actions();
        cam = GetComponent<Camera>();
        currentDistance = distance;
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void LateUpdate()
    {
        if (!target) return;

        // --- Look���͎擾 ---
        Vector2 look = input.Player.Look.ReadValue<Vector2>();

        yaw += look.x * sensitivityX * Time.deltaTime;
        pitch -= look.y * sensitivityY * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // �s�{�b�g�ʒu�i���̍���������j
        Vector3 pivot = target.position + Vector3.up * height;

        // �J������]
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        // ���z�ʒu
        Vector3 desired = pivot - rot * Vector3.forward * distance;

        // �R���W�����␳
        float targetDist = distance;
        if (Physics.SphereCast(pivot, collisionRadius, (desired - pivot).normalized, out RaycastHit hit, distance, collisionMask))
        {
            targetDist = Mathf.Clamp(hit.distance - skin, minDistance, distance);
        }
        currentDistance = Mathf.Lerp(currentDistance, targetDist, 10f * Time.deltaTime);

        // �J�����ʒu/��]�K�p
        cam.transform.position = pivot - rot * Vector3.forward * currentDistance;
        cam.transform.rotation = rot;
    }
}
