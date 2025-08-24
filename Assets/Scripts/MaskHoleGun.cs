using UnityEngine;

public class MaskHoleGun : MonoBehaviour
{
    [Header("Refs")]
    public Camera aimCam;                 // Main Camera を割り当て
    public GameObject holeMaskPrefab;     // 後述の「穴」プレハブ
    public LayerMask hitMask;             // 破壊物のレイヤー。未設定(0)なら Everything 扱い

    [Header("Params")]
    public float range = 200f;
    public float holeRadius = 0.3f;       // 穴の半径(m)
    public bool debugLog = true;

    void Update()
    {
        // ※ Attack.csから撃っているなら、この行はコメントアウトしてOK
        if (Input.GetButtonDown("Fire1")) FireOnce();
    }

    public void FireOnce()
    {
        if (aimCam == null) aimCam = Camera.main;
        if (aimCam == null) { Debug.LogError("[MaskHoleGun] aimCam が未設定"); return; }

        int mask = (hitMask.value == 0) ? ~0 : hitMask.value; // 未設定なら Everything

        // 自分のコライダー内を避けるために 5cm 前から撃つ
        Vector3 origin = aimCam.transform.position + aimCam.transform.forward * 0.05f;
        Vector3 dir = aimCam.transform.forward;
        Ray ray = new Ray(origin, dir);

        // レイ上のヒットを全部拾い、「最初に見つかった破壊対象」だけに穴を開ける
        var hits = Physics.RaycastAll(ray, range, mask, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        DestructibleStencil target = null;
        RaycastHit chosen = default;

        foreach (var h in hits)
        {
            // ここで床レイヤー等をスキップしてもOK
            // if (h.collider.gameObject.layer == LayerMask.NameToLayer("Ground")) continue;

            var d = h.collider.GetComponentInParent<DestructibleStencil>();
            if (d != null) { target = d; chosen = h; break; }
        }

        if (target == null)
        {
            if (debugLog) Debug.Log("[MaskHoleGun] レイ上に破壊対象（DestructibleStencil）が見つかりませんでした。");
            Debug.DrawRay(origin, dir * range, Color.red, 1f);
            return;
        }

        // 穴を生成（Zファイト対策で法線側に +0.005m）
        Vector3 pos = chosen.point + chosen.normal * 0.005f;
        Quaternion rot = Quaternion.LookRotation(chosen.normal, Vector3.up);

        var go = Instantiate(holeMaskPrefab, pos, rot);
        go.transform.localScale = Vector3.one * (holeRadius * 2f); // 直径でスケール
        go.transform.SetParent(target.transform, true);            // 対象が動いても追従

        // 穴にも同じ _StencilRef を設定（この対象だけが抜ける）
        var r = go.GetComponent<Renderer>();
        if (r)
        {
            var mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);
            mpb.SetInt("_StencilRef", target.stencilRef);
            r.SetPropertyBlock(mpb);
        }

        if (debugLog) Debug.Log($"[MaskHoleGun] 穴生成 OK target={target.name} at {pos} (_StencilRef={target.stencilRef})");
        Debug.DrawRay(origin, dir * chosen.distance, Color.green, 1f);
    }
}
