using System.Collections;
using UnityEditor.Rendering.Analytics;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 20f;
    public float Power = 5.0f;
    public float radius = 10.0f;//爆風の範囲
    public float Speed = 50.0f;

    public Vector3 movedir;//飛ばす方向
    void Start()
    {
        Destroy(gameObject, lifetime);
        movedir = Camera.main.transform.forward.normalized;//生成された瞬間に方向を決める
    }

    private void Awake()
    {
        StartCoroutine(Attack(3f));
    }
    IEnumerator Attack(float delay)//3秒待ってから発射
    {
        yield return new WaitForSeconds(delay);
        transform.position += movedir * Speed * Time.deltaTime;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Bomb"))
        {
            Debug.Log("爆弾と当たるるる");
            //爆弾に爆発範囲を取得 爆弾と当たっているコライダーを取得
            Collider[] colliders = Physics.OverlapSphere(collision.transform.position, 20f);
            //範囲内のオブジェクトを探して爆破
            foreach (Collider Hitcollider in colliders)
            {
                Rigidbody rb = Hitcollider.attachedRigidbody;
                if (rb != null)
                {
                    rb.AddExplosionForce(Power, transform.position, radius, 3f, ForceMode.Impulse);
                }
            }
        }
    }
}
