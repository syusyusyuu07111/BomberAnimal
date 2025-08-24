using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ProjectileMakesHole : MonoBehaviour
{
    [Header("Hole")]
    public GameObject holeMaskPrefab;   // StencilMask �̃v���n�u
    public float holeRadius = 0.3f;     // ���̔��a
    public bool destroyOnHit = true;    // 1�񓖂����������

    [Header("Debug")]
    public bool debugLog = true;

    // --- Trigger �ł� Collision �ł��E����悤�������� ---
    void OnCollisionEnter(Collision col)
    {
        if (TryMakeHole(col.collider, col.GetContact(0).point, col.GetContact(0).normal) && destroyOnHit)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        // Trigger ���g���Ă���ꍇ�͋ߖT�_�ő�p
        Vector3 p = transform.position;
        Vector3 n = -transform.forward; // �����悻�̖@��
        if (TryMakeHole(other, p, n) && destroyOnHit)
            Destroy(gameObject);
    }

    bool TryMakeHole(Collider col, Vector3 point, Vector3 normal)
    {
        // �j��Ώۃ}�[�J�[��T��
        var destructible = col.GetComponentInParent<DestructibleStencil>();
        if (!destructible)
        {
            if (debugLog) Debug.Log($"[Projectile] Hit '{col.name}' (no DestructibleStencil)");
            return false;
        }

        // ���v���n�u�����ݒ�Ȃ牽�����Ȃ�
        if (!holeMaskPrefab)
        {
            Debug.LogWarning("[Projectile] holeMaskPrefab ���ݒ�ł��B");
            return false;
        }

        // ���𐶐��iZ�t�@�C�g����ɖ@�������֏��������o���j
        Vector3 pos = point + normal * 0.005f;
        Quaternion rot = Quaternion.LookRotation(normal, Vector3.up);

        var go = Instantiate(holeMaskPrefab, pos, rot);
        go.transform.localScale = Vector3.one * (holeRadius * 2f);
        go.transform.SetParent(destructible.transform, true); // �Ώۂ������Ă��Ǐ]

        // �ΏۂƓ��� _StencilRef �������ɂ��n���i���̑Ώۂ���������j
        var r = go.GetComponent<Renderer>();
        if (r)
        {
            var mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);
            mpb.SetInt("_StencilRef", destructible.stencilRef);
            r.SetPropertyBlock(mpb);
        }

        if (debugLog) Debug.Log($"[Projectile] ������ OK at {pos} (target={destructible.name}, _StencilRef={destructible.stencilRef})");
        return true;
    }
}
