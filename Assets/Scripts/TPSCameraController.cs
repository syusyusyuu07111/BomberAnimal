using UnityEngine;
using UnityEngine.InputSystem;

public class TPSCameraController : MonoBehaviour
{
    [Header("ターゲット")]
    public Transform target;

    [Header("距離・高さ")]
    public float distance = 5f;
    public float minDistance = 0.5f;
    public float height = 2f;

    [Header("感度")]
    public float sensitivityX = 120f;    // ヨー（横）deg/秒
    public float sensitivityY = 120f;    // ピッチ（縦）deg/秒
    public float minPitch = -20f;
    public float maxPitch = 70f;

    [Header("ヨーのハード制限（スプラ風）")]
    [Range(10f, 170f)]
    public float maxYawOffset = 80f;     // キャラ前からの左右許容角（±）
    // 入力で限界を越えた分はキャラをこのフレームで回して追従（別速度は不要）

    [Header("カメラコリジョン")]
    public float collisionRadius = 0.25f;
    public float skin = 0.05f;
    public LayerMask collisionMask = ~0;

    private float yaw;                   // カメラの絶対ヨー
    private float pitch;                 // カメラのピッチ
    private float currentDistance;
    private InputSystem_Actions input;
    private Camera cam;

    void Awake()
    {
        input = new InputSystem_Actions();
        cam = GetComponent<Camera>();
        currentDistance = distance;

        if (target)
        {
            // 初期はキャラ背面＝オフセット0から開始
            yaw = target.eulerAngles.y;
        }
        pitch = Mathf.Clamp(transform.eulerAngles.x, minPitch, maxPitch);
    }

    void OnEnable()
    {
        input.Enable();
        input.Player.Look.Enable();
    }

    void OnDisable()
    {
        input.Player.Look.Disable();
        input.Disable();
    }

    void LateUpdate()
    {
        if (!target) return;

        float dt = Time.unscaledDeltaTime;
        Vector2 look = input.Player.Look.ReadValue<Vector2>();

        // ---- 入力でカメラ角度を更新（候補値） ----
        float deltaYaw = look.x * sensitivityX * dt;
        float deltaPitch = -look.y * sensitivityY * dt;
        float yawCandidate = yaw + deltaYaw;
        float pitchCandidate = Mathf.Clamp(pitch + deltaPitch, minPitch, maxPitch);

        // ---- キャラ前方からの相対ヨーをハードクランプし、超過分はキャラを回す ----
        float targetYaw = target.eulerAngles.y;
        // いまのオフセット（-180〜180）
        float offsetNow = Mathf.DeltaAngle(targetYaw, yaw);
        // 入力適用後に“相対”でどれだけズレるか（=オフセットの理想値）
        float offsetDesired = offsetNow + deltaYaw;
        // 許容範囲にクランプ
        float offsetClamped = Mathf.Clamp(offsetDesired, -maxYawOffset, maxYawOffset);
        // クランプで削られた分＝限界超過分 → そのぶんだけキャラを回す
        float overflow = offsetDesired - offsetClamped;
        if (Mathf.Abs(overflow) > 0.0001f)
        {
            // 同フレームでキャラを回して、常にオフセットが範囲内に収まるようにする
            target.Rotate(0f, overflow, 0f, Space.World);
            targetYaw = target.eulerAngles.y; // 念のため再取得
        }

        // 最終的なカメラ角は「キャラのヨー + クランプ済みオフセット」
        yaw = targetYaw + offsetClamped;
        pitch = pitchCandidate;

        // ---- 位置計算（肩口ピボット） ----
        Vector3 pivot = target.position + Vector3.up * height;
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desired = pivot - rot * Vector3.forward * distance;

        // ---- コリジョン（壁抜け防止） ----
        Vector3 dir = desired - pivot;
        float dist = dir.magnitude;
        dir = dist > 0.0001f ? dir / dist : Vector3.back;

        float targetDist = distance;
        if (Physics.SphereCast(pivot, collisionRadius, dir, out RaycastHit hit, distance, collisionMask, QueryTriggerInteraction.Ignore))
        {
            targetDist = Mathf.Clamp(hit.distance - skin, minDistance, distance);
        }
        currentDistance = Mathf.Lerp(currentDistance, targetDist, 10f * dt);

        // 適用
        cam.transform.position = pivot - rot * Vector3.forward * currentDistance;
        cam.transform.rotation = rot;
    }
}
