using UnityEditor.Rendering.Analytics;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float  lifetime = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Bomb"))
        {

        }
    }
}
