using UnityEngine;

public class GroundManager : MonoBehaviour
{
    [SerializeField] public float GroundHP;
    [SerializeField] public float FallCount;
    void Start()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            Destroy(this.gameObject);
        }

    }
}
