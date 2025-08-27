using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("‰Šprefab")]
    public GameObject CellPrefab;


    [Header("˜A‘±‚µ‚Äo‚·‰Š‚ÌŠÔŠu")]
    public float Spacing;//ŠÔŠu



    void Start()
    {
        Instantiate(CellPrefab, transform.position, Quaternion.identity);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
