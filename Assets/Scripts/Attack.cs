using UnityEngine;
using UnityEngine.InputSystem;

public class Attack : MonoBehaviour
{
    [Header("References")]
    public Transform muzzle;                 // 発射位置（銃口）
    public GameObject projectilePrefab;      // 弾Prefab

    [Header("Fire Settings")]
    public float cooldown = 0.2f;            // 発射間隔
    public float aimDistance = 100f;         // 照準距離
    public Camera aimCamera;                 // 照準用カメラ
    public LayerMask aimMask = ~0;           // Raycastの対象レイヤー

    private float lastFireTime = -999f;
    private InputSystem_Actions input;

    void Awake()
    {
        input = new InputSystem_Actions();
    }

    void OnEnable()
    {
        input.Enable();
        input.Player.Attack.performed += OnAttack;
    }

    void OnDisable()
    {
        input.Player.Attack.performed -= OnAttack;
        input.Disable();
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (!muzzle || !projectilePrefab) return;
        if (Time.time < lastFireTime + cooldown) return;
        if (!aimCamera) aimCamera = Camera.main;

        lastFireTime = Time.time;

        // カメラからRayを飛ばして狙点を決める
        Vector3 camPos = aimCamera.transform.position;
        Vector3 camFwd = aimCamera.transform.forward;
        Vector3 aimPoint = camPos + camFwd * aimDistance;

        if (Physics.Raycast(camPos, camFwd, out var hit, aimDistance, aimMask, QueryTriggerInteraction.Ignore))
        {
            aimPoint = hit.point;
        }

        // muzzle から狙点方向を算出
        Vector3 dir = (aimPoint - muzzle.position).normalized;

        // 発射位置を少し前にずらす（自爆防止）
        Vector3 spawnPos = muzzle.position + dir * 0.25f;

        // 弾を生成（回転は進行方向を向かせる）
        Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir, Vector3.up));

        Debug.Log("Attack: projectile spawned.");
    }
}
