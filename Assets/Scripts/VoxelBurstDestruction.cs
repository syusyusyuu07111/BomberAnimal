using UnityEngine;
using System.Collections.Generic;

public class VoxelBurstDestruction : MonoBehaviour
{
    [Header("�Ώ�")]
    public Renderer targetRenderer;        // �����b�V����Renderer�i�N���b�v�V�F�[�_�K�p�j
    public Collider targetCollider;        // �����b�V����Collider�iClosestPoint�œ��O����Ɏg���j

    [Header("�{�N�Z��")]
    public GameObject voxelPrefab;         // �����ȗ����́iRigidbody�t���j
    public int poolSize = 300;
    public float voxelSize = 0.1f;         // ������1��
    public float burstRadius = 0.6f;       // �j��̔��a
    public float spacing = 0.12f;          // �{�N�Z���Ԋu�i= ���x�j
    public float explosionForce = 6f;
    public float explosionUp = 0.2f;
    public float voxelsLife = 0.8f;        // �Еt���܂ł̕b

    [Header("�N���b�v���i�V�F�[�_���ƘA�g�j")]
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
        // 1) �N���b�v���F�f�����L���Ă���߂�
        StopAllCoroutines();
        StartCoroutine(ClipRoutine(hitPosWS));

        // 2) �{�N�Z���o���i���a�����i�q�ŃX�L�����j
        SpawnVoxels(hitPosWS, strength);

        // 3) �Еt���\��
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
        // Renderer�������}�e���A���ł� MPB ��OK
        targetRenderer.GetPropertyBlock(mpb);
        mpb.SetVector(clipCenterProp, centerWS);
        mpb.SetFloat(clipRadiusProp, radius);
        targetRenderer.SetPropertyBlock(mpb);
    }

    void SpawnVoxels(Vector3 centerWS, float strength)
    {
        spawned.Clear();
        float r = burstRadius;
        // �����̊i�q�����Ő؂�
        for (float x = -r; x <= r; x += spacing)
            for (float y = -r; y <= r; y += spacing)
                for (float z = -r; z <= r; z += spacing)
                {
                    Vector3 posWS = centerWS + new Vector3(x, y, z);
                    if ((posWS - centerWS).sqrMagnitude > r * r) continue;

                    // ���O�������蔻��iCollider���ʂȂ� ClosestPoint �������ň�v���₷���j
                    if (targetCollider != null)
                    {
                        Vector3 c = targetCollider.ClosestPoint(posWS);
                        if ((c - posWS).sqrMagnitude > 1e-5f) continue; // �O���̓X�L�b�v
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
