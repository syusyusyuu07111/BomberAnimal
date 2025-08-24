using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class DestructibleStencil : MonoBehaviour
{
    [Header("Stencil")]
    [Range(1, 255)] public int stencilRef = 0;   // 0�Ȃ玩���̔�
    public Renderer[] targetRenderers;            // ���w��Ȃ�q��Renderer�������擾

    [Header("Clip Holes (max 8)")]
    [Range(1, 8)] public int maxHoles = 8;
    public bool drawGizmos = false;
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.25f);

    // ===== ��������u������萔�ɒB�����甚��/�j��v�@�\ =====
    [Header("Explode by Hole Count")]
    public bool explodeWhenHolesReach = true;     // �������B�Ŕ���
    public int holesToExplode = 4;                // ��4�Ŕ���
    public bool destroyGameObject = true;         // ������ɖ{�̂�����
    public GameObject debrisPrefab;               // �j�Ѓv���n�u�i�C�Ӂj
    public UnityEvent onExplode;                  // �����C�x���g�i�X�R�A�ʒm�Ȃǁj

    [Header("Explosion VFX/SFX (optional)")]
    public GameObject explosionVfxPrefab;         // �����ڗp�G�t�F�N�g
    public AudioSource explosionSfx;              // SE�i�C�Ӂj

    [Header("Explosion Physics")]
    public float explosionRadius = 4f;            // �������a
    public float explosionForce = 20f;            // ���������iImpulse�j
    public float upwardModifier = 0.25f;          // ������u�[�X�g
    public LayerMask explosionAffectMask = ~0;    // �e����^���郌�C��

    // ����
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
    /// ����ǉ��i�ő� maxHoles�A��������Â����̂���j���j�B
    /// �� �ǉ���A������ holesToExplode �ɒB�����甚�����j��B
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
            ExplodeAndDestroy(worldPos); // �Ō�̌��ʒu�𔚐S��
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

        // ��������
        var cols = Physics.OverlapSphere(origin, explosionRadius, explosionAffectMask, QueryTriggerInteraction.Ignore);
        foreach (var c in cols)
        {
            var rb = c.attachedRigidbody;
            if (rb) rb.AddExplosionForce(explosionForce, origin, explosionRadius, upwardModifier, ForceMode.Impulse);
        }

        // ������/������𖳌���
        foreach (var r in _renders) if (r) r.enabled = false;
        foreach (var col in GetComponentsInChildren<Collider>()) col.enabled = false;

        // �f�u���u�������i�C�Ӂj
        if (debrisPrefab) Instantiate(debrisPrefab, transform.position, transform.rotation);

        // �O���ʒm�i�X�R�A���_�E�A���g���K���j
        onExplode?.Invoke();

        // ���̔j��
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

        // �����̖ڈ�
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
