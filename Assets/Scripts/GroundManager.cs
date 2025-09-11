using System.Collections;
using UnityEngine;

public class GroundManager : MonoBehaviour
{
    [SerializeField] public float GroundHP;
    [SerializeField] public float FallCount=5.0f;
    void Start()
    {

    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(FallBlock(FallCount));
        }

    }
    private IEnumerator FallBlock(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(this.gameObject);
    }

}
