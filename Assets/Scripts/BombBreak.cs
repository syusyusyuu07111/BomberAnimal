using System.Collections;
using UnityEngine;

public class BombBreak : MonoBehaviour
{

    [Header("爆発エフェクト")]
    public GameObject BombFirePrefab;
    public float OriginMass;
    private void OnEnable()
    {
        StartCoroutine("Explosion");
    }
    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(3.0f);
        Instantiate(BombFirePrefab, transform.transform.position, Quaternion.identity);

        Collider[] colliders = Physics.OverlapSphere(transform.position,3.0f);
        foreach(Collider collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if(rb!=null)
                //massをへらして範囲内の足場を吹っ飛ばす=================================================================================
            {
                OriginMass = rb.mass;
                rb.mass = 1.0f;
                rb.AddExplosionForce(100f, transform.position, 0.1f, 0f, ForceMode.Impulse);
                StartCoroutine(ReturnoriginMass(rb, OriginMass, 0.0001f));
            }
        }
    }
    //爆発の瞬間にへらしてたmassを元に戻す==================================================================================================--
    IEnumerator ReturnoriginMass(Rigidbody rb, float originMass, float delay)
    {
        yield return new WaitForSeconds(delay);
        rb.mass = originMass;
        Destroy(this.gameObject);
    }
}
