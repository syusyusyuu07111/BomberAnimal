using UnityEngine;
using UnityEngine.InputSystem;

public class Attack : MonoBehaviour
{
    [Header("References")]
    public Transform muzzle;
    public GameObject projectilePrefab;

    [Header("Fire Settings")]
    public float projectileSpeed = 20f;
    public float cooldown = 0.2f;       // 連射テストしやすく少し短め

    [Header("Aiming")]
    public Camera aimCamera;
    public float aimDistance = 100f;
    public LayerMask aimMask = ~0;      // プレイヤー層は除外推奨

    float lastFireTime = -999f;
    InputSystem_Actions input;

    void Awake() { input = new InputSystem_Actions(); }
    void OnEnable() { input.Enable(); input.Player.Attack.performed += OnAttack; }
    void OnDisable() { input.Player.Attack.performed -= OnAttack; input.Disable(); }

    void OnAttack(InputAction.CallbackContext ctx)
    {
        if (!muzzle || !projectilePrefab) return;
        if (Time.time < lastFireTime + cooldown) return;
        if (!aimCamera) aimCamera = Camera.main;

        lastFireTime = Time.time;

        // カメラ中央から狙点
        Vector3 camPos = aimCamera.transform.position;
        Vector3 camFwd = aimCamera.transform.forward;
        Vector3 aimPoint = camPos + camFwd * aimDistance;
        if (Physics.Raycast(camPos, camFwd, out var hit, aimDistance, aimMask, QueryTriggerInteraction.Ignore))
            aimPoint = hit.point;

        // 方向
        Vector3 dir = (aimPoint - muzzle.position);
        if (dir.sqrMagnitude < 0.0001f) dir = muzzle.forward;
        dir.Normalize();

        // 自分のColliderと少し距離を空けて生成（自爆防止）
        Vector3 spawnPos = muzzle.position + dir * 0.25f;

        var go = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir, Vector3.up));

        // 発射者との衝突無視
        var projCol = go.GetComponent<Collider>();
        if (projCol)
        {
            foreach (var sc in GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(projCol, sc, true);
        }

        // 速度付与
        var rb = go.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = dir * projectileSpeed;
#else
            rb.velocity = dir * projectileSpeed;
#endif
        }

        Debug.Log("Attack: fired toward camera aim.");
    }
}
