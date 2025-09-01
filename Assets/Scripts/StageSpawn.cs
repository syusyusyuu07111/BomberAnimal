using UnityEngine;

public class StageSpawn : MonoBehaviour
{
    [Header("複製するプレハブ")]
    public GameObject boxPrefab;

    [Header("個数（横=X / 縦=Z / 高さ=Y）")]
    [Min(1)] public int countX = 20; // 横
    [Min(1)] public int countZ = 50; // 縦
    [Min(1)] public int countY = 10; // 高さ

    [Header("セルサイズ（0なら自動推定）")]
    public Vector3 cellSizeOverride = Vector3.zero; // 例: (1,1,1) を入れると確実に隙間ゼロ

    [Header("どっち向きに伸ばす？（ワールド軸基準）")]
    public bool toNegativeX = false; // 左へ伸ばしたいなら true
    public bool toNegativeZ = false; // 手前(−Z)へ伸ばしたいなら true
    public bool toNegativeY = false; // 下へ積みたいなら true

    [Header("ぶら下げ先（未指定ならこのオブジェクト配下）")]
    public Transform parent;

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

        // 進む方向（±）
        float sx = toNegativeX ? -1f : 1f;
        float sz = toNegativeZ ? -1f : 1f;
        float sy = toNegativeY ? -1f : 1f;

        Vector3 origin = transform.position;                  // 基準点
        Quaternion rot = boxPrefab.transform.rotation;        // 回転はプレハブ準拠
        Transform p = parent ? parent : transform;            // 親

        // 生成ループ：原点から X/Z/Y に等間隔で配置（隙間ゼロ）
        for (int y = 0; y < countY; y++)
        {
            for (int z = 0; z < countZ; z++)
            {
                for (int x = 0; x < countX; x++)
                {
                    Vector3 pos = origin +
                                  new Vector3(sx * x * cell.x,
                                              sy * y * cell.y,
                                              sz * z * cell.z);
                    Instantiate(boxPrefab, pos, rot, p);
                }
            }
        }

        Debug.Log($"StageSpawn: {countX}×{countY}×{countZ} 個生成 / セル={cell} / 基準={origin}");
    }

    // プレハブの見た目サイズ（Renderer→Collider）を推定してセルとする
    Vector3 GuessCellSize(GameObject prefab)
    {
        GameObject tmp = Instantiate(prefab, new Vector3(1e6f, 1e6f, 1e6f), prefab.transform.rotation);
        tmp.hideFlags = HideFlags.HideAndDontSave;

        Bounds b = new Bounds(tmp.transform.position, Vector3.zero);

        var rends = tmp.GetComponentsInChildren<Renderer>();
        if (rends != null && rends.Length > 0)
            foreach (var r in rends) b.Encapsulate(r.bounds);
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
