using UnityEngine;

public class PlayerNearbyPrompt : MonoBehaviour
{
    [Header("検知設定")]
    public float detectRadius = 2.0f;
    public LayerMask pickableMask = ~0;       // 必要なら「Pickable」レイヤーを作って指定

    [Header("表示テキスト")]
    public string promptText = "持つ";

    [Header("可視化")]
    public bool drawGizmo = true;

    PickablePrompt current;   // 今表示中の対象

    void Update()
    {
        var next = FindNearestPickable();

        // 表示対象が変わったら前のを消して新しい方に表示
        if (current != next)
        {
            if (current) current.HidePrompt();
            current = next;
            if (current) current.ShowPrompt(promptText);
        }

        // 何も近くにないときは消す
        if (current == null) return;
    }

    PickablePrompt FindNearestPickable()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position, detectRadius, pickableMask, QueryTriggerInteraction.Ignore);

        PickablePrompt nearest = null;
        float best = float.MaxValue;

        foreach (var h in hits)
        {
            if (!h) continue;
            var p = h.GetComponentInParent<PickablePrompt>();
            if (!p) continue;

            // 一番近いもの
            float d2 = (p.transform.position - transform.position).sqrMagnitude;
            if (d2 < best)
            {
                best = d2;
                nearest = p;
            }
        }
        return nearest;
    }

    void OnDisable()
    {
        if (current) current.HidePrompt();
        current = null;
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmo) return;
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.25f);
        Gizmos.DrawSphere(transform.position, detectRadius);
    }
}
