using System.Collections;
using UnityEngine;

public class GroundManager : MonoBehaviour
{
    [SerializeField] public float GroundHP;
    [SerializeField] public float FallCount=5.0f;
    [SerializeField] public float NoiseSpeed;
    [SerializeField] public float AmplitudeY=0.05f;//—h‚ê•c•ûŒü
    [SerializeField] public float AmplitudeZ=0.03f;//—h‚ê•c•ûŒü

    Vector3 Basepos;//Šî€‚ÌˆÊ’u

    public bool TouchFlag;

    public float NoiseOffset;
    void Start()
    {
        TouchFlag = false;
        Basepos = transform.position;//Šî€‚ÌˆÊ’u


    }
    public void SetNoise()
    {
        NoiseOffset = UnityEngine.Random.Range(0f, 256f);
    }
    private void Update()
    {
        if(TouchFlag)//ƒuƒƒbƒN‚ğ—h‚ç‚·
        {
            //Šp“xŒvZ ƒÆ(t) = ƒÖ t + ƒÓ
            float Angle = Time.time * NoiseSpeed + NoiseOffset;
            //ƒTƒCƒ“‚Ì’l‚ğŒvZ
            float sinX = Mathf.Cos(Angle);//‰¡
            float sinY = Mathf.Sin(Angle);//ã‰º
            float sinZ = Mathf.Sin(Angle+Mathf.PI);//‰œ
            //À•W‚ğ”½‰f
            transform.position = new Vector3(Basepos.x+AmplitudeY*sinX, Basepos.y + AmplitudeY * sinY, Basepos.z+AmplitudeY*sinZ);
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
