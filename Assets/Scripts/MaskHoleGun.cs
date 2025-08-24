using UnityEngine;

public class MaskHoleGun : MonoBehaviour
{
    [Header("Refs")]
    public Camera aimCam;                 // Main Camera �����蓖��
    public GameObject holeMaskPrefab;     // ��q�́u���v�v���n�u
    public LayerMask hitMask;             // �j�󕨂̃��C���[�B���ݒ�(0)�Ȃ� Everything ����

    [Header("Params")]
    public float range = 200f;
    public float holeRadius = 0.3f;       // ���̔��a(m)
    public bool debugLog = true;

    void Update()
    {
        // �� Attack.cs���猂���Ă���Ȃ�A���̍s�̓R�����g�A�E�g����OK
        if (Input.GetButtonDown("Fire1")) FireOnce();
    }

    public void FireOnce()
    {
        if (aimCam == null) aimCam = Camera.main;
        if (aimCam == null) { Debug.LogError("[MaskHoleGun] aimCam �����ݒ�"); return; }

        int mask = (hitMask.value == 0) ? ~0 : hitMask.value; // ���ݒ�Ȃ� Everything

        // �����̃R���C�_�[��������邽�߂� 5cm �O���猂��
        Vector3 origin = aimCam.transform.position + aimCam.transform.forward * 0.05f;
        Vector3 dir = aimCam.transform.forward;
        Ray ray = new Ray(origin, dir);

        // ���C��̃q�b�g��S���E���A�u�ŏ��Ɍ��������j��Ώہv�����Ɍ����J����
        var hits = Physics.RaycastAll(ray, range, mask, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        DestructibleStencil target = null;
        RaycastHit chosen = default;

        foreach (var h in hits)
        {
            // �����ŏ����C���[�����X�L�b�v���Ă�OK
            // if (h.collider.gameObject.layer == LayerMask.NameToLayer("Ground")) continue;

            var d = h.collider.GetComponentInParent<DestructibleStencil>();
            if (d != null) { target = d; chosen = h; break; }
        }

        if (target == null)
        {
            if (debugLog) Debug.Log("[MaskHoleGun] ���C��ɔj��ΏہiDestructibleStencil�j��������܂���ł����B");
            Debug.DrawRay(origin, dir * range, Color.red, 1f);
            return;
        }

        // ���𐶐��iZ�t�@�C�g�΍�Ŗ@������ +0.005m�j
        Vector3 pos = chosen.point + chosen.normal * 0.005f;
        Quaternion rot = Quaternion.LookRotation(chosen.normal, Vector3.up);

        var go = Instantiate(holeMaskPrefab, pos, rot);
        go.transform.localScale = Vector3.one * (holeRadius * 2f); // ���a�ŃX�P�[��
        go.transform.SetParent(target.transform, true);            // �Ώۂ������Ă��Ǐ]

        // ���ɂ����� _StencilRef ��ݒ�i���̑Ώۂ�����������j
        var r = go.GetComponent<Renderer>();
        if (r)
        {
            var mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);
            mpb.SetInt("_StencilRef", target.stencilRef);
            r.SetPropertyBlock(mpb);
        }

        if (debugLog) Debug.Log($"[MaskHoleGun] ������ OK target={target.name} at {pos} (_StencilRef={target.stencilRef})");
        Debug.DrawRay(origin, dir * chosen.distance, Color.green, 1f);
    }
}
