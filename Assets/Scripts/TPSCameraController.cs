using UnityEngine;
using UnityEngine.InputSystem; // 新InputSystemを使う

public class TPSCameraController : MonoBehaviour
{
    [Header("ターゲット")]
    public Transform target;

    [Header("距離・高さ")]
    public float distance = 5f;
    public float minDistance = 0.5f;
    public float height = 2f;

    [Header("感度")]
    public float sensitivityX = 120f;
    public float sensitivityY = 120f;
    public float minPitch = -20f;
    public float maxPitch = 70f;

    [Header("カメラコリジョン")]
    public float collisionRadius = 0.25f;
    public float skin = 0.05f;
    public LayerMask collisionMask = ~0;

    private float yaw, pitch;
    private float currentDistance;
    private InputSystem_Actions input; // ← ここでInputActionを使う
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

        // --- Look入力取得 ---
        Vector2 look = input.Player.Look.ReadValue<Vector2>();

        yaw += look.x * sensitivityX * Time.deltaTime;
        pitch -= look.y * sensitivityY * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // ピボット位置（頭の高さあたり）
        Vector3 pivot = target.position + Vector3.up * height;

        // カメラ回転
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        // 理想位置
        Vector3 desired = pivot - rot * Vector3.forward * distance;

        // コリジョン補正
        float targetDist = distance;
        if (Physics.SphereCast(pivot, collisionRadius, (desired - pivot).normalized, out RaycastHit hit, distance, collisionMask))
        {
            targetDist = Mathf.Clamp(hit.distance - skin, minDistance, distance);
        }
        currentDistance = Mathf.Lerp(currentDistance, targetDist, 10f * Time.deltaTime);

        // カメラ位置/回転適用
        cam.transform.position = pivot - rot * Vector3.forward * currentDistance;
        cam.transform.rotation = rot;
    }
}
