using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProjectileMakesHole : MonoBehaviour
{
    [Header("Hole")]
    public float holeRadius = 0.25f;
    public LayerMask holeAffectMask = ~0;      // �����J���Ă悢����̃��C���[

    [Header("Life")]
    public bool destroyOnHit = true;           // �����Œe������
    public bool destroyOnlyWhenHoleMade = false; // �����J��������������
    public float autoDestroyAfter = 5f;        // ��������
    public float armingDelay = 0.08f;          // ���˒���͓����薳��

    [Header("Shooter Ignore")]
    public GameObject shooterRoot;             // ���ˎ҂�Root�i�Փ˖����p�j

    float spawnTime;
    Collider myCol;

    void Awake()
    {
        spawnTime = Time.time;
        myCol = GetComponent<Collider>();
        if (autoDestroyAfter > 0f) Destroy(gameObject, autoDestroyAfter);

        // shooterRoot �����O�ɓ����Ă���ΏՓ˖������Z�b�g
        if (shooterRoot) IgnoreShooterColliders(shooterRoot, true);
    }

    // Attack������Ă�Ŕ��ˎ҂�ݒ�
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
        // ���ˎ҂Ȃ疳��
        if (shooterRoot && col.transform.IsChildOf(shooterRoot.transform)) return;

        bool madeHole = false;

        // �Ώۃ��C���[�ȊO�Ȃ猊���J���Ȃ��i���̂܂ܒʉ߁j
        if (((1 << col.gameObject.layer) & holeAffectMask) != 0)
        {
            var ds = col.GetComponentInParent<DestructibleStencil>();
            if (ds != null)
            {
                ds.AddHole(hitPoint, holeRadius);
                madeHole = true;
            }
        }

        // ���������𐧌�
        if (destroyOnHit && (!destroyOnlyWhenHoleMade || madeHole))
        {
            Destroy(gameObject);
        }
    }
}
