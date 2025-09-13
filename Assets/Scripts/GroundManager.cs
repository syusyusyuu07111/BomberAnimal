using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundManager : MonoBehaviour
{

    //管理用======================================================================================================================
    public bool activate = false;
    public List<GroundManager> allground = new();//床の状態を保持
    public List<GroundManager> noactivate = new();//有効化されてない床


    //床ステータス================================================================================================================
    [SerializeField] public float GroundHP;
    [SerializeField] public float FallCount=5.0f;
    [SerializeField] public float NoiseSpeed;
    [SerializeField] public float AmplitudeY=0.05f;//揺れ幅縦方向
    [SerializeField] public float AmplitudeZ=0.03f;//揺れ幅縦方向
    [SerializeField] public float SpeedIncrease=0.1f;//揺れ幅縦方向
    Vector3 Basepos;//基準の位置
    public bool TouchFlag;
    public float NoiseOffset;
    public GameObject[]grounds;//破壊できる床を格納

    private void Awake()
    {
        //シーン内にある床を配列に格納する
        grounds = GameObject.FindGameObjectsWithTag("Ground");
        Debug.Log("床の数:"+grounds.Length);
        //ランダムに選んで処理（ランダムな床を破壊可能にする）
        if(grounds.Length>0)
        {
            int pickgound = Random.Range(0, grounds.Length);
            GameObject hitground = grounds[pickgound];
            Debug.Log("破壊される床は;"+hitground.name);



            //選ばれたオブジェクトにスクリプトを適応させる
            GroundManager gm = GetComponent<GroundManager>();
            if(gm!=null)
            {
                gm.enabled = true;
            }
        }

    }
    void Start()
    {
        TouchFlag = false;
        Basepos = transform.position;//基準の位置
    }
    public void SetNoise()
    {
        NoiseOffset = UnityEngine.Random.Range(0f, 256f);
    }
    private void OnEnable()
    {

    }
    private void Update()
    {
        if(TouchFlag)//ブロックを揺らす
        {

            NoiseSpeed+= SpeedIncrease * Time.deltaTime;
            //角度計算 θ(t) = ω t + φ
            float Angle = Time.time * NoiseSpeed + NoiseOffset;
            //サインの値を計算
            float sinX = Mathf.Cos(Angle);//横
            float sinY = Mathf.Sin(Angle);//上下
            float sinZ = Mathf.Sin(Angle+Mathf.PI);//奥
            //座標を反映
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
        Destroy(this.gameObject);
    }
}
