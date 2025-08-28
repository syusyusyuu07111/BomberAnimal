using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("‰Šprefab")]
    public GameObject CellPrefab;





    void Start()
    {
        Instantiate(CellPrefab, transform.position, Quaternion.identity);
        Vector3 origin = transform.position;
        for(int i=1; i<100;i++)
        {
            //Å‰‚ÌÀ•W+ŒÂ”•ª‚¸‚ç‚·
            Vector3 pos = origin + i * Vector3.forward;
            Instantiate(CellPrefab, pos, Quaternion.identity);

        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
