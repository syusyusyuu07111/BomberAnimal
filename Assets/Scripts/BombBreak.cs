using System.Collections;
using UnityEngine;

public class BombBreak : MonoBehaviour
{

    [Header("爆発エフェクト")]
    public GameObject BombFirePrefab;
    private void OnEnable()
    {
        StartCoroutine("Explosion");
    }
    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(3.0f);
        Instantiate(BombFirePrefab, transform.transform.position, Quaternion.identity);

        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
        foreach(Collider collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if(rb!=null)
            {
                rb.AddExplosionForce(1000f, transform.position, 5f, 0f, ForceMode.Impulse);
            }
        }
        Destroy(this.gameObject);
    }
}
