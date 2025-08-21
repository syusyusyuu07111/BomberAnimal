using UnityEngine;
using UnityEngine.InputSystem;

public class Attack : MonoBehaviour
{
    [Header("References")]
    public Transform muzzle;             // 発射位置（銃口）
    public GameObject projectilePrefab;  // 弾Prefab（Rigidbody付き）

    [Header("Fire Settings")]
    public float projectileSpeed = 20f;  // 弾速
    public float cooldown = 0.3f;        // 連射間隔

    [Header("Aiming")]
    public Camera aimCamera;             // 省略時は Camera.main
    public float aimDistance = 100f;     // 狙点を探す最大距離
    public LayerMask aimMask = ~0;       // 当たり判定用（Playerは外すのが推奨）

    private InputSystem_Actions input;
    private float lastFireTime = -999f;

    private void Awake()
    {
        input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Attack.performed += OnAttack;
    }

    private void OnDisable()
    {
        input.Player.Attack.performed -= OnAttack;
        input.Disable();
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (Time.time < lastFireTime + cooldown) return;
        if (!muzzle || !projectilePrefab) return;

        if (!aimCamera) aimCamera = Camera.main; // 未指定なら自動取得
        lastFireTime = Time.time;

        // ---- カメラ中心からのレイで狙点を決める ----
        Vector3 camPos = aimCamera.transform.position;
        Vector3 camFwd = aimCamera.transform.forward;
        Vector3 aimPoint = camPos + camFwd * aimDistance;

        if (Physics.Raycast(camPos, camFwd, out RaycastHit hit, aimDistance, aimMask, QueryTriggerInteraction.Ignore))
        {
            aimPoint = hit.point; // 当たった場所を狙点に
        }

        // 銃口から狙点方向のベクトル
        Vector3 dir = (aimPoint - muzzle.position);
        if (dir.sqrMagnitude < 0.0001f) dir = muzzle.forward; // 近すぎ対策
        dir.Normalize();

        // ---- 弾生成＆発射 ----
        GameObject go = Instantiate(projectilePrefab, muzzle.position, Quaternion.LookRotation(dir, Vector3.up));

        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false; // 爆弾のように落下させたいなら true に
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Unity 6: velocity ではなく linearVelocity を使用
            rb.linearVelocity = dir * projectileSpeed;
        }

        Debug.Log("Attack: fired toward camera aim.");
    }
}
