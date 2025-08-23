using UnityEngine;
using System.Collections.Generic;

public class VoxelBurstDestruction : MonoBehaviour
{
    [Header("�Ώ�")]
    public Renderer targetRenderer;        // �N���b�v�Ή��}�e���A�������蓖�Ă� Renderer
    public Collider targetCollider;        // ���O�������蔻��p�i�C�Ӂj

    [Header("�{�N�Z��")]
    public GameObject voxelPrefab;         // Rigidbody + BoxCollider �t�����L���[�u
    public int poolSize = 300;
    public float voxelSize = 0.1f;
    public float burstRadius = 0.6f;
    public float spacing = 0.12f;
    public float explosionForce = 6f;
    public float explosionUp = 0.2f;
    public float voxelsLife = 0.8f;

    [Header("�N���b�v�i�V�F�[�_�A�g�F�K���v���p�e�B������v�j")]
    public string clipCenterProp = "_ClipCenterWS";
    public string clipRadiusProp = "_ClipRadius";
    // ���ǉ��F�~���}�X�N
    public string clipDirProp = "_ClipDirWS";   // Vector3�i�q�b�g�@���j
    public string clipCosProp = "_ClipCos";     // Float�icos(theta)�j

    [Range(0.1f, 1.0f)] public float clipRadiusMax = 0.55f;
    [Range(0.01f, 1.0f)] public float clipExpandTime = 0.06f;
    [Range(0.05f, 1.0f)] public float clipShrinkTime = 0.35f;

    // ���ǉ��F�~���p�i�x�j�B�������قǋ��������g���������������h
    [Range(5f, 80f)] public float clipConeAngleDeg = 35f;

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

    /// <summary>
    /// �q�b�g�n�_�Ɩ@����n���āg���̕t�߂����h�j�󉉏o
    /// </summary>
    public void TriggerAt(Vector3 hitPosWS, Vector3 hitNormalWS, float strength = 1f)
    {
        // 1) �N���b�v�i�����~���j����u�L���Ė߂�
        StopAllCoroutines();
        StartCoroutine(ClipRoutine(hitPosWS, hitNormalWS));

        // 2) �q�b�g���ӂ����{�N�Z�����o��
        SpawnVoxels(hitPosWS, strength);

        // 3) �Еt��
        Invoke(nameof(DespawnAll), voxelsLife);
    }

    System.Collections.IEnumerator ClipRoutine(Vector3 centerWS, Vector3 dirWS)
    {
        dirWS.Normalize();
        float cos = Mathf.Cos(clipConeAngleDeg * Mathf.Deg2Rad);

        float t = 0f;
        // expand
        while (t < clipExpandTime)
        {
            t += Time.deltaTime;
            float r = Mathf.Lerp(0f, clipRadiusMax, t / clipExpandTime);
            ApplyClip(centerWS, r, dirWS, cos);
            yield return null;
        }
        // shrink
        t = 0f;
        while (t < clipShrinkTime)
        {
            t += Time.deltaTime;
            float r = Mathf.Lerp(clipRadiusMax, 0f, t / clipShrinkTime);
            ApplyClip(centerWS, r, dirWS, cos);
            yield return null;
        }
        // �ŏI�I�Ɍ��ɖ߂��i���a0�j
        ApplyClip(centerWS, 0f, dirWS, cos);
    }

    /// <summary>
    /// �����S�{���a�{�~������/�J�����V�F�[�_�ɓn��
    /// </summary>
    void ApplyClip(Vector3 centerWS, float radius, Vector3 dirWS, float clipCos)
    {
        if (mpb == null) mpb = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(mpb);

        // ��
        mpb.SetVector(clipCenterProp, centerWS);
        mpb.SetFloat(clipRadiusProp, radius);

        // ���~��
        if (!string.IsNullOrEmpty(clipDirProp)) mpb.SetVector(clipDirProp, dirWS);
        if (!string.IsNullOrEmpty(clipCosProp)) mpb.SetFloat(clipCosProp, clipCos);

        targetRenderer.SetPropertyBlock(mpb);
    }

    void SpawnVoxels(Vector3 centerWS, float strength)
    {
        spawned.Clear();
        float r = burstRadius;
        for (float x = -r; x <= r; x += spacing)
            for (float y = -r; y <= r; y += spacing)
                for (float z = -r; z <= r; z += spacing)
                {
                    Vector3 posWS = centerWS + new Vector3(x, y, z);
                    if ((posWS - centerWS).sqrMagnitude > r * r) continue;

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
