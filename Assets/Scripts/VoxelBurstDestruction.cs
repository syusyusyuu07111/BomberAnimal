using UnityEngine;
using System.Collections.Generic;

public class VoxelBurstDestruction : MonoBehaviour
{
    [Header("対象")]
    public Renderer targetRenderer;        // 元メッシュのRenderer（クリップシェーダ適用）
    public Collider targetCollider;        // 元メッシュのCollider（ClosestPointで内外判定に使う）

    [Header("ボクセル")]
    public GameObject voxelPrefab;         // 小さな立方体（Rigidbody付き）
    public int poolSize = 300;
    public float voxelSize = 0.1f;         // 立方体1辺
    public float burstRadius = 0.6f;       // 破壊の半径
    public float spacing = 0.12f;          // ボクセル間隔（= 密度）
    public float explosionForce = 6f;
    public float explosionUp = 0.2f;
    public float voxelsLife = 0.8f;        // 片付けまでの秒

    [Header("クリップ球（シェーダ側と連携）")]
    public string clipCenterProp = "_ClipCenterWS";
    public string clipRadiusProp = "_ClipRadius";
    public float clipRadiusMax = 0.55f;
    public float clipExpandTime = 0.06f;
    public float clipShrinkTime = 0.35f;

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

    public void TriggerAt(Vector3 hitPosWS, Vector3 hitNormalWS, float strength = 1f)
    {
        // 1) クリップ球：素早く広げてから戻す
        StopAllCoroutines();
        StartCoroutine(ClipRoutine(hitPosWS));

        // 2) ボクセル出す（半径内を格子でスキャン）
        SpawnVoxels(hitPosWS, strength);

        // 3) 片付け予約
        Invoke(nameof(DespawnAll), voxelsLife);
    }

    System.Collections.IEnumerator ClipRoutine(Vector3 centerWS)
    {
        float t = 0f;
        // expand
        while (t < clipExpandTime)
        {
            t += Time.deltaTime;
            float r = Mathf.Lerp(0f, clipRadiusMax, t / clipExpandTime);
            ApplyClip(centerWS, r);
            yield return null;
        }
        // shrink
        t = 0f;
        while (t < clipShrinkTime)
        {
            t += Time.deltaTime;
            float r = Mathf.Lerp(clipRadiusMax, 0f, t / clipShrinkTime);
            ApplyClip(centerWS, r);
            yield return null;
        }
        ApplyClip(centerWS, 0f);
    }

    void ApplyClip(Vector3 centerWS, float radius)
    {
        // Rendererが複数マテリアルでも MPB でOK
        targetRenderer.GetPropertyBlock(mpb);
        mpb.SetVector(clipCenterProp, centerWS);
        mpb.SetFloat(clipRadiusProp, radius);
        targetRenderer.SetPropertyBlock(mpb);
    }

    void SpawnVoxels(Vector3 centerWS, float strength)
    {
        spawned.Clear();
        float r = burstRadius;
        // 立方体格子を球で切る
        for (float x = -r; x <= r; x += spacing)
            for (float y = -r; y <= r; y += spacing)
                for (float z = -r; z <= r; z += spacing)
                {
                    Vector3 posWS = centerWS + new Vector3(x, y, z);
                    if ((posWS - centerWS).sqrMagnitude > r * r) continue;

                    // 内外ざっくり判定（Colliderが凸なら ClosestPoint が内側で一致しやすい）
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
