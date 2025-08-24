using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class DestructibleStencil : MonoBehaviour
{
    [Header("Stencil")]
    [Range(1, 255)] public int stencilRef = 0;   // 0なら自動採番
    public Renderer[] targetRenderers;            // 未指定なら子孫Rendererを自動取得

    [Header("Clip Holes (max 8)")]
    [Range(1, 8)] public int maxHoles = 8;
    public bool drawGizmos = false;
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.25f);

    // ===== ここから「穴が一定数に達したら爆発/破壊」機能 =====
    [Header("Explode by Hole Count")]
    public bool explodeWhenHolesReach = true;     // 穴数到達で爆発
    public int holesToExplode = 4;                // ★4個で爆発
    public bool destroyGameObject = true;         // 爆発後に本体を消す
    public GameObject debrisPrefab;               // 破片プレハブ（任意）
    public UnityEvent onExplode;                  // 爆発イベント（スコア通知など）

    [Header("Explosion VFX/SFX (optional)")]
    public GameObject explosionVfxPrefab;         // 見た目用エフェクト
    public AudioSource explosionSfx;              // SE（任意）

    [Header("Explosion Physics")]
    public float explosionRadius = 4f;            // 爆風半径
    public float explosionForce = 20f;            // 爆風強さ（Impulse）
    public float upwardModifier = 0.25f;          // 上向きブースト
    public LayerMask explosionAffectMask = ~0;    // 影響を与えるレイヤ

    // 内部
    static int _next = 1;
    readonly List<Vector4> _holes = new();        // xyz=center(WS), w=radius
    Renderer[] _renders;
    MaterialPropertyBlock _mpb;
    bool _exploded = false;

    // Shader property IDs
    static readonly int StencilRefID = Shader.PropertyToID("_StencilRef");
    static readonly int HoleCountID = Shader.PropertyToID("_HoleCountF");
    static readonly int[] HoleIDs = {
        Shader.PropertyToID("_Hole0"), Shader.PropertyToID("_Hole1"),
        Shader.PropertyToID("_Hole2"), Shader.PropertyToID("_Hole3"),
        Shader.PropertyToID("_Hole4"), Shader.PropertyToID("_Hole5"),
        Shader.PropertyToID("_Hole6"), Shader.PropertyToID("_Hole7"),
    };

    void Awake() { Cache(); EnsureStencilRef(); ApplyAll(); }
    void OnEnable() { Cache(); ApplyAll(); }
    void OnValidate() { if (!enabled) return; Cache(); EnsureStencilRef(); ApplyAll(); }

    void Cache()
    {
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        if (targetRenderers != null && targetRenderers.Length > 0)
            _renders = targetRenderers;
        else
            _renders = GetComponentsInChildren<Renderer>(true);
    }

    void EnsureStencilRef()
    {
        if (stencilRef <= 0)
        {
            stencilRef = _next++;
            if (_next > 255) _next = 1;
        }
    }

    void ApplyAll() { ApplyStencilRef(); ApplyHoles(); }

    void ApplyStencilRef()
    {
        foreach (var r in _renders)
        {
            if (!r) continue;
            r.GetPropertyBlock(_mpb);
            _mpb.SetInt(StencilRefID, stencilRef);
            r.SetPropertyBlock(_mpb);
        }
    }

    void ApplyHoles()
    {
        int count = Mathf.Clamp(_holes.Count, 0, HoleIDs.Length);
        foreach (var r in _renders)
        {
            if (!r) continue;
            r.GetPropertyBlock(_mpb);

            _mpb.SetFloat(HoleCountID, count);
            for (int i = 0; i < HoleIDs.Length; i++)
            {
                Vector4 v = (i < _holes.Count) ? _holes[i] : Vector4.zero;
                _mpb.SetVector(HoleIDs[i], v);
            }
            r.SetPropertyBlock(_mpb);
        }
    }

    /// <summary>
    /// 穴を追加（最大 maxHoles、超えたら古いものから破棄）。
    /// ★ 追加後、穴数が holesToExplode に達したら爆発→破壊。
    /// </summary>
    public void AddHole(Vector3 worldPos, float radius)
    {
        if (_exploded) return;

        if (_holes.Count >= Mathf.Clamp(maxHoles, 1, HoleIDs.Length))
            _holes.RemoveAt(0);

        _holes.Add(new Vector4(worldPos.x, worldPos.y, worldPos.z, radius));
        ApplyHoles();

        if (explodeWhenHolesReach && _holes.Count >= Mathf.Max(1, holesToExplode))
        {
            ExplodeAndDestroy(worldPos); // 最後の穴位置を爆心に
        }
    }

    public void ClearHoles()
    {
        _holes.Clear();
        ApplyHoles();
    }

    void ExplodeAndDestroy(Vector3 origin)
    {
        if (_exploded) return;
        _exploded = true;

        // VFX/SE
        if (explosionVfxPrefab) Instantiate(explosionVfxPrefab, origin, Quaternion.identity);
        if (explosionSfx) explosionSfx.Play();

        // 物理爆風
        var cols = Physics.OverlapSphere(origin, explosionRadius, explosionAffectMask, QueryTriggerInteraction.Ignore);
        foreach (var c in cols)
        {
            var rb = c.attachedRigidbody;
            if (rb) rb.AddExplosionForce(explosionForce, origin, explosionRadius, upwardModifier, ForceMode.Impulse);
        }

        // 見た目/当たりを無効化
        foreach (var r in _renders) if (r) r.enabled = false;
        foreach (var col in GetComponentsInChildren<Collider>()) col.enabled = false;

        // デブリ置き換え（任意）
        if (debrisPrefab) Instantiate(debrisPrefab, transform.position, transform.rotation);

        // 外部通知（スコア加点・連鎖トリガ等）
        onExplode?.Invoke();

        // 実体破棄
        if (destroyGameObject) Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (drawGizmos && _holes.Count > 0)
        {
            Gizmos.color = gizmoColor;
            foreach (var h in _holes)
                Gizmos.DrawSphere(new Vector3(h.x, h.y, h.z), h.w);
        }

        // 爆風の目安
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
