using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProjectileMakesHole : MonoBehaviour
{
    [Header("Hole")]
    public float holeRadius = 0.25f;
    public LayerMask holeAffectMask = ~0;      // 穴を開けてよい相手のレイヤー

    [Header("Life")]
    public bool destroyOnHit = true;           // 命中で弾を消す
    public bool destroyOnlyWhenHoleMade = false; // 穴が開いた時だけ消す
    public float autoDestroyAfter = 5f;        // 自動消滅
    public float armingDelay = 0.08f;          // 発射直後は当たり無効

    [Header("Shooter Ignore")]
    public GameObject shooterRoot;             // 発射者のRoot（衝突無視用）

    float spawnTime;
    Collider myCol;

    void Awake()
    {
        spawnTime = Time.time;
        myCol = GetComponent<Collider>();
        if (autoDestroyAfter > 0f) Destroy(gameObject, autoDestroyAfter);

        // shooterRoot が事前に入っていれば衝突無視をセット
        if (shooterRoot) IgnoreShooterColliders(shooterRoot, true);
    }

    // Attack側から呼んで発射者を設定
    public void SetShooter(GameObject shooter)
    {
        shooterRoot = shooter;
        if (!myCol) myCol = GetComponent<Collider>();
        IgnoreShooterColliders(shooterRoot, true);
    }

    void OnDestroy()
    {
        if (shooterRoot && myCol) IgnoreShooterColliders(shooterRoot, false);
    }

    void IgnoreShooterColliders(GameObject root, bool ignore)
    {
        foreach (var sc in root.GetComponentsInChildren<Collider>())
            Physics.IgnoreCollision(myCol, sc, ignore);
    }

    bool Armed => Time.time >= spawnTime + armingDelay;

    void OnCollisionEnter(Collision c)
    {
        if (!Armed) return;
        HandleHit(c.collider, c.GetContact(0).point);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!Armed) return;
        HandleHit(other, other.ClosestPoint(transform.position));
    }

    void HandleHit(Collider col, Vector3 hitPoint)
    {
        // 発射者なら無視
        if (shooterRoot && col.transform.IsChildOf(shooterRoot.transform)) return;

        bool madeHole = false;

        // 対象レイヤー以外なら穴を開けない（そのまま通過）
        if (((1 << col.gameObject.layer) & holeAffectMask) != 0)
        {
            var ds = col.GetComponentInParent<DestructibleStencil>();
            if (ds != null)
            {
                ds.AddHole(hitPoint, holeRadius);
                madeHole = true;
            }
        }

        // 消す条件を制御
        if (destroyOnHit && (!destroyOnlyWhenHoleMade || madeHole))
        {
            Destroy(gameObject);
        }
    }
}
