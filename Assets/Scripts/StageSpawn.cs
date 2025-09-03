using UnityEngine;

public class GridBlockSpawner : MonoBehaviour
{
    [SerializeField] private GameObject blockPrefab;   // 敷き詰めるブロックのPrefab
    [SerializeField] private Transform parent;         // 生成物の親（任意）

    [Header("範囲（両端含む）")]
    [SerializeField] private int xMax = 30;            // x: 0〜30
    [SerializeField] private int yMax = 5;             // y: 0〜5
    [SerializeField] private int zMax = 10;            // z: 0〜10

    [Header("セル間隔（ブロックサイズ/ピッチ）")]
    [SerializeField] private Vector3 cellSize = Vector3.one;  // 1なら隙間なし。必要に応じて調整
    [SerializeField] private Vector3 worldOffset = Vector3.zero; // 全体のオフセット（任意）

    private void Start()
    {
        if (!blockPrefab)
        {
            Debug.LogError("blockPrefab が未設定です。", this);
            return;
        }

        for (int x = 0; x <= xMax; x++)
        {
            for (int y = 0; y <= yMax; y++)
            {
                for (int z = 0; z <= zMax; z++)
                {
                    Vector3 pos = new Vector3(
                        x * cellSize.x,
                        y * cellSize.y,
                        z * cellSize.z
                    ) + worldOffset;

                    Instantiate(blockPrefab, pos, Quaternion.identity, parent);
                }
            }
        }
    }
}
