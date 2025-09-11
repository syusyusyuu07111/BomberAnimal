using System.Collections;
using UnityEngine;

public class GroundManager : MonoBehaviour
{
    [SerializeField] public float GroundHP;
    [SerializeField] public float FallCount=5.0f;
    [SerializeField] public float NoiseSpeed;

    Vector3 Basepos;//基準の位置

    public bool TouchFlag;

    public float NoiseOffset;
    void Start()
    {
        TouchFlag = false;
        Basepos = transform.position;//基準の位置


    }
    public void SetNoise()
    {
        NoiseOffset = UnityEngine.Random.Range(0f, 256f);
    }
    private void Update()
    {
        if(TouchFlag)//ブロックを揺らす
        {
            //角度計算 θ(t) = ω t + φ
            float Angle = Time.time * NoiseSpeed + NoiseOffset;

        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(FallBlock(FallCount));
            TouchFlag = true;

        }

    }
    private IEnumerator FallBlock(float delay)
    {
        yield return new WaitForSeconds(delay);
        //Destroy(this.gameObject);
    }

}
