using System.Collections;
using UnityEditor.Rendering.Analytics;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 20f;
    public float Power = 5.0f;
    public float radius = 10.0f;//”š•—‚Ì”ÍˆÍ
    public float Speed = 50.0f;

    public Vector3 movedir;//”ò‚Î‚·•ûŒü
    public Vector3 Transform;//¶¬‚³‚ê‚½êŠ

    public bool CanMove = false;

    public Vector3 ExplosionCenter;
    void Start()
    {
        Destroy(gameObject, lifetime);
        movedir = Camera.main.transform.forward.normalized;//¶¬‚³‚ê‚½uŠÔ‚É•ûŒü‚ğŒˆ‚ß‚é
        Transform = transform.position;
    }
    private void Awake()
    {
        StartCoroutine(Attack(3f));
    }

    public void Update()
    {
        if(!CanMove)
        {
            Transform = transform.position;
        }
        if(CanMove)
        {
            transform.position += movedir * Speed * Time.deltaTime;
        }
    }

    IEnumerator Attack(float delay)//3•b‘Ò‚Á‚Ä‚©‚ç”­Ë
    {
        yield return new WaitForSeconds(delay);
        CanMove = true;
    }

    private void OnTriggerEnter(Collider other)


    {
        if(other.CompareTag("Bomb"))
        {
            Debug.Log("”š’e‚Æ“–‚½‚é‚é‚é");

            Vector3 ExplosionCenter = other.transform.position;
            //”š’e‚É”š”­”ÍˆÍ‚ğæ“¾ ”š’e‚Æ“–‚½‚Á‚Ä‚¢‚éƒRƒ‰ƒCƒ_[‚ğæ“¾
            Collider[] colliders = Physics.OverlapSphere(ExplosionCenter, 20f);
            //”ÍˆÍ“à‚ÌƒIƒuƒWƒFƒNƒg‚ğ’T‚µ‚Ä”š”j
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
