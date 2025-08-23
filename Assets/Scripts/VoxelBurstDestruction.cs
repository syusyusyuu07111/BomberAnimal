using UnityEngine;
using System.Collections.Generic;

public class VoxelBurstDestruction : MonoBehaviour
{
    [Header("対象")]
    public Renderer targetRenderer;        // クリップ対応マテリアルを割り当てた Renderer
    public Collider targetCollider;        // 内外ざっくり判定用（任意）

    [Header("ボクセル")]
    public GameObject voxelPrefab;         // Rigidbody + BoxCollider 付き小キューブ
    public int poolSize = 300;
    public float voxelSize = 0.1f;
    public float burstRadius = 0.6f;
    public float spacing = 0.12f;
    public float explosionForce = 6f;
    public float explosionUp = 0.2f;
    public float voxelsLife = 0.8f;

    [Header("クリップ（シェーダ連携：必ずプロパティ名を一致）")]
    public string clipCenterProp = "_ClipCenterWS";
    public string clipRadiusProp = "_ClipRadius";
    // ★追加：円錐マスク
    public string clipDirProp = "_ClipDirWS";   // Vector3（ヒット法線）
    public string clipCosProp = "_ClipCos";     // Float（cos(theta)）

    [Range(0.1f, 1.0f)] public float clipRadiusMax = 0.55f;
    [Range(0.01f, 1.0f)] public float clipExpandTime = 0.06f;
    [Range(0.05f, 1.0f)] public float clipShrinkTime = 0.35f;

    // ★追加：円錐角（度）。小さいほど狭く＝より“当たった所だけ”
    [Range(5f, 80f)] public float clipConeAngleDeg = 35f;

    Queue<Rigidbody> pool;
    readonly List<GameObject> spawned = new();
    MaterialPropertyBlock mpb;

    void Awake()
    {
        pool = new Queue<Rigidbody>(poolSize);
        for (int i = 0; i < poolSize; i++)
        {
            var go = Instantiate(voxelPrefab);
            go.transform.localScale = Vector3.one * voxelSize;
            go.SetActive(false);
            var rb = go.GetComponent<Rigidbody>();
            pool.Enqueue(rb);
        }
        mpb = new MaterialPropertyBlock();
    }

    /// <summary>
    /// ヒット地点と法線を渡して“その付近だけ”破壊演出
    /// </summary>
    public void TriggerAt(Vector3 hitPosWS, Vector3 hitNormalWS, float strength = 1f)
    {
        // 1) クリップ（球∩円錐）を一瞬広げて戻す
        StopAllCoroutines();
        StartCoroutine(ClipRoutine(hitPosWS, hitNormalWS));

        // 2) ヒット周辺だけボクセルを出す
        SpawnVoxels(hitPosWS, strength);

        // 3) 片付け
        Invoke(nameof(DespawnAll), voxelsLife);
    }

    System.Collections.IEnumerator ClipRoutine(Vector3 centerWS, Vector3 dirWS)
    {
        dirWS.Normalize();
        float cos = Mathf.Cos(clipConeAngleDeg * Mathf.Deg2Rad);

        float t = 0f;
        // expand
        while (t < clipExpandTime)
        {
            t += Time.deltaTime;
            float r = Mathf.Lerp(0f, clipRadiusMax, t / clipExpandTime);
            ApplyClip(centerWS, r, dirWS, cos);
            yield return null;
        }
        // shrink
        t = 0f;
        while (t < clipShrinkTime)
        {
            t += Time.deltaTime;
            float r = Mathf.Lerp(clipRadiusMax, 0f, t / clipShrinkTime);
            ApplyClip(centerWS, r, dirWS, cos);
            yield return null;
        }
        // 最終的に元に戻す（半径0）
        ApplyClip(centerWS, 0f, dirWS, cos);
    }

    /// <summary>
    /// ★中心＋半径＋円錐方向/開きをシェーダに渡す
    /// </summary>
    void ApplyClip(Vector3 centerWS, float radius, Vector3 dirWS, float clipCos)
    {
        if (mpb == null) mpb = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(mpb);

        // 球
        mpb.SetVector(clipCenterProp, centerWS);
        mpb.SetFloat(clipRadiusProp, radius);

        // ★円錐
        if (!string.IsNullOrEmpty(clipDirProp)) mpb.SetVector(clipDirProp, dirWS);
        if (!string.IsNullOrEmpty(clipCosProp)) mpb.SetFloat(clipCosProp, clipCos);

        targetRenderer.SetPropertyBlock(mpb);
    }

    void SpawnVoxels(Vector3 centerWS, float strength)
    {
        spawned.Clear();
        float r = burstRadius;
        for (float x = -r; x <= r; x += spacing)
            for (float y = -r; y <= r; y += spacing)
                for (float z = -r; z <= r; z += spacing)
                {
                    Vector3 posWS = centerWS + new Vector3(x, y, z);
                    if ((posWS - centerWS).sqrMagnitude > r * r) continue;

                    if (targetCollider != null)
                    {
                        Vector3 c = targetCollider.ClosestPoint(posWS);
                        if ((c - posWS).sqrMagnitude > 1e-5f) continue; // 外側はスキップ
                    }

                    if (pool.Count == 0) return;
                    var rb = pool.Dequeue();
                    var go = rb.gameObject;
                    go.transform.position = posWS;
                    go.transform.rotation = Random.rotation;
                    go.SetActive(true);
                    spawned.Add(go);

                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.mass = 0.02f;
                    rb.linearDamping = 0.2f;
                    rb.angularDamping = 0.05f;

                    rb.AddExplosionForce(explosionForce * strength, centerWS, r, explosionUp, ForceMode.Impulse);
                }
    }

    void DespawnAll()
    {
        foreach (var go in spawned)
        {
            if (!go) continue;
            go.SetActive(false);
            var rb = go.GetComponent<Rigidbody>();
            pool.Enqueue(rb);
        }
        spawned.Clear();
    }
}
