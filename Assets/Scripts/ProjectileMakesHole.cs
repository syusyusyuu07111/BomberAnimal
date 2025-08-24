using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ProjectileMakesHole : MonoBehaviour
{
    [Header("Hole")]
    public GameObject holeMaskPrefab;   // StencilMask のプレハブ
    public float holeRadius = 0.3f;     // 穴の半径
    public bool destroyOnHit = true;    // 1回当たったら消す

    [Header("Debug")]
    public bool debugLog = true;

    // --- Trigger でも Collision でも拾えるよう両方実装 ---
    void OnCollisionEnter(Collision col)
    {
        if (TryMakeHole(col.collider, col.GetContact(0).point, col.GetContact(0).normal) && destroyOnHit)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        // Trigger を使っている場合は近傍点で代用
        Vector3 p = transform.position;
        Vector3 n = -transform.forward; // おおよその法線
        if (TryMakeHole(other, p, n) && destroyOnHit)
            Destroy(gameObject);
    }

    bool TryMakeHole(Collider col, Vector3 point, Vector3 normal)
    {
        // 破壊対象マーカーを探す
        var destructible = col.GetComponentInParent<DestructibleStencil>();
        if (!destructible)
        {
            if (debugLog) Debug.Log($"[Projectile] Hit '{col.name}' (no DestructibleStencil)");
            return false;
        }

        // 穴プレハブが未設定なら何もしない
        if (!holeMaskPrefab)
        {
            Debug.LogWarning("[Projectile] holeMaskPrefab 未設定です。");
            return false;
        }

        // 穴を生成（Zファイト回避に法線方向へ少し押し出す）
        Vector3 pos = point + normal * 0.005f;
        Quaternion rot = Quaternion.LookRotation(normal, Vector3.up);

        var go = Instantiate(holeMaskPrefab, pos, rot);
        go.transform.localScale = Vector3.one * (holeRadius * 2f);
        go.transform.SetParent(destructible.transform, true); // 対象が動いても追従

        // 対象と同じ _StencilRef を穴側にも渡す（この対象だけ抜ける）
        var r = go.GetComponent<Renderer>();
        if (r)
        {
            var mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);
            mpb.SetInt("_StencilRef", destructible.stencilRef);
            r.SetPropertyBlock(mpb);
        }

        if (debugLog) Debug.Log($"[Projectile] 穴生成 OK at {pos} (target={destructible.name}, _StencilRef={destructible.stencilRef})");
        return true;
    }
}
