using UnityEngine;
using UnityEngine.InputSystem;

public class Attack : MonoBehaviour
{
    [Header("References")]
    public Transform muzzle;             // 発射位置（銃口とか）
    public GameObject projectilePrefab;  // 弾Prefab（Rigidbody付き）

    [Header("Settings")]
    public float projectileSpeed = 20f;  // 弾速
    public float cooldown = 0.3f;        // 連射間隔

    private InputSystem_Actions input;   // 自動生成されたInputActions
    private float lastFireTime = -999f;

    private void Awake()
    {
        input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Attack.performed += OnAttack;  // 攻撃ボタン押下で発火
    }

    private void OnDisable()
    {
        input.Player.Attack.performed -= OnAttack;
        input.Disable();
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (Time.time < lastFireTime + cooldown) return;   // クールダウン処理
        if (!muzzle || !projectilePrefab) return;          // 必要な参照が無ければ撃たない

        lastFireTime = Time.time;

        Debug.Log("Attack pressed! 弾を発射します");

        // 弾生成
        GameObject go = Instantiate(projectilePrefab, muzzle.position, muzzle.rotation);

        // Rigidbody で前に飛ばす
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false; // 必要ならtrueに
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            rb.linearVelocity = muzzle.forward * projectileSpeed;
        }
    }
}
