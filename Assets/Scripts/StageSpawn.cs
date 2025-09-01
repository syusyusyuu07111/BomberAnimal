using UnityEngine;

public class StageSpawn : MonoBehaviour
{
    [Header("必須")]
    public GameObject boxPrefab;                 // 箱プレハブ（アタッチ）

    // X=横(20), Y=高さ(10), Z=縦(50)
    [Header("個数（X=横 / Y=高さ / Z=縦）")]
    public Vector3Int count = new Vector3Int(20, 10, 50);

    [Header("セルサイズ（0なら自動推定：Renderer→Collider）")]
    public Vector3 cellSizeOverride = Vector3.zero;

    [Header("配置オプション")]
    public Transform parent;                     // 親。未指定ならこのオブジェクト配下
    public bool centerGrid = true;               // グリッドを原点中心に配置

    void Start()
    {
        if (!boxPrefab)
        {
            Debug.LogError("StageSpawn: boxPrefab が未設定です。");
            return;
        }

        // セルサイズ決定
        Vector3 cell = (cellSizeOverride != Vector3.zero) ? cellSizeOverride : GuessCellSize(boxPrefab);
        if (cell == Vector3.zero) cell = Vector3.one; // 念のため

        // 原点（開始位置）
        Vector3 origin = transform.position;
        if (centerGrid)
        {
            origin -= new Vector3(
                (count.x - 1) * cell.x * 0.5f,
                (count.y - 1) * cell.y * 0.5f,
                (count.z - 1) * cell.z * 0.5f
            );
        }

        Transform p = parent ? parent : transform;

        // 生成ループ（X=横, Y=高さ, Z=縦）
        for (int y = 0; y < count.y; y++)
        {
            for (int z = 0; z < count.z; z++)
            {
                for (int x = 0; x < count.x; x++)
                {
                    Vector3 pos = origin + new Vector3(x * cell.x, y * cell.y, z * cell.z);
                    Instantiate(boxPrefab, pos, Quaternion.identity, p);
                }
            }
        }

        Debug.Log($"StageSpawn: 生成完了 {count.x}×{count.y}×{count.z} = {count.x * count.y * count.z} 個, セル={cell}");
    }

    // プレハブの見た目サイズを推定（Rendererが無ければCollider）
    Vector3 GuessCellSize(GameObject prefab)
    {
        GameObject tmp = Instantiate(prefab, new Vector3(1e6f, 1e6f, 1e6f), Quaternion.identity);
        tmp.hideFlags = HideFlags.HideAndDontSave;

        Bounds b = new Bounds(tmp.transform.position, Vector3.zero);

        var rends = tmp.GetComponentsInChildren<Renderer>();
        if (rends != null && rends.Length > 0)
        {
            foreach (var r in rends) b.Encapsulate(r.bounds);
        }
        else
        {
            var cols = tmp.GetComponentsInChildren<Collider>();
            foreach (var c in cols) b.Encapsulate(c.bounds);
        }

        Destroy(tmp);

        Vector3 s = b.size;
        return new Vector3(
            Mathf.Max(s.x, 0.0001f),
            Mathf.Max(s.y, 0.0001f),
            Mathf.Max(s.z, 0.0001f)
        );
    }
}
