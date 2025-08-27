using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("‰Šprefab")]
    public GameObject CellPrefab;


    [Header("˜A‘±‚µ‚Äo‚·‰Š‚ÌŠÔŠu")]
    public float Spacing;//ŠÔŠu

    [Header("¶¬ˆÊ’u")]
    public Vector3 Bomb;

    void Start()
    {
        Instantiate(CellPrefab, Bomb, Quaternion.identity);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
