using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundManager : MonoBehaviour
{

    //管理用======================================================================================================================
    static bool Isactivate = false;
    static List<GroundManager> allground = new();//床の状態を保持
    static List<GroundManager> noactivate = new();//有効化されてない床
    //床ステータス================================================================================================================
    [SerializeField] public float GroundHP;
    [SerializeField] public float FallCount = 5.0f;
    [SerializeField] public float NoiseSpeed;
    [SerializeField] public float AmplitudeY = 0.05f;//揺れ幅縦方向
    [SerializeField] public float AmplitudeZ = 0.03f;//揺れ幅縦方向
    [SerializeField] public float SpeedIncrease = 0.1f;//揺れ幅縦方向
    Vector3 Basepos;//基準の位置
    public bool TouchFlag;
    public float NoiseOffset;
    bool isactive = false;//壊れる床か判定
    //色の差し替え用======================================================================================================================
    [SerializeField] Renderer TargetRenderer;
    [SerializeField] Material NormalMaterial;
    [SerializeField] Material ActiveMaterial;
    [SerializeField] Material TouchMaterial;

    private void Awake()
    {
        allground.Add(this);//GroundManagerコンポーネントを参照
    }
    void Start()
    {
        // コンポーネントを有効化するための準備----------------------------------------------------------------------------
        if (!Isactivate)
        {
            Isactivate = true;
            noactivate.AddRange(allground);//allgroundの床をnoactivateにすべてコピーする
            StartCoroutine(activateRoutine());
        }
        //-------------------------------------------------------------------------------------------------------
        TouchFlag = false;
        Basepos = transform.position;//基準の位置
        if (TargetRenderer && NormalMaterial)//通常のマテリアルセット
        {
            TargetRenderer.material = NormalMaterial;
        }
    }
    //床にあるGroundManagerを一つずつ有効化していく===============================================================
    IEnumerator activateRoutine()
    {
        while (noactivate.Count > -30)
        {
            int PickGround = Random.Range(0, noactivate.Count);
            var Chosen = noactivate[PickGround];//選ばれたブロッくを取得
            if (Chosen != null)//選ばれたブロックのコンポーネントを有効化する------------------------------------------
            {
                Chosen.SetFloor(true);//SetFloor呼び出し
                Debug.Log("選ばれた床;" + Chosen.name);
                noactivate.RemoveAt(PickGround);
                yield return new WaitForSeconds(3f);
            }
;
        }
    }
    //==============================================================================================================
    public void SetNoise()
    {
        NoiseOffset = UnityEngine.Random.Range(0f, 256f);
    }
    private void Update()
    {
        if (!(TouchFlag && isactive)) return;
        //グラグラ揺らすときの処理-----------------------------------------------------------------------------------------------------
        NoiseSpeed += SpeedIncrease * Time.deltaTime;
        //角度計算 θ(t) = ω t + φ
        float Angle = Time.time * NoiseSpeed + NoiseOffset;
        //サインの値を計算
        float sinX = Mathf.Cos(Angle);//横
        float sinY = Mathf.Sin(Angle);//上下
        float sinZ = Mathf.Sin(Angle + Mathf.PI);//奥
        //座標を反映
        transform.position = new Vector3(Basepos.x + AmplitudeY * sinX, Basepos.y + AmplitudeY * sinY, Basepos.z + AmplitudeZ * sinZ);
    }
    //有効化されたときの挙動(初期化)===================================================================================================-
    private void OnEnable()
    {
        NoiseOffset = Random.Range(0f, 256f);
        TouchFlag = false;
    }
    //==========================================================================================================================

    //壊れる床かどうかを判定=====================================================================================================-
    public void SetFloor(bool on)
    {
        isactive = on;
        if (TargetRenderer)
        {
            if (on && ActiveMaterial)
            {
                TargetRenderer.material = ActiveMaterial;
            }
            else if (NormalMaterial)
            {
                TargetRenderer.material = NormalMaterial;
            }
        }
    }
    //============================================================================================================================
    private void OnCollisionEnter(Collision collision)
    {
        if (!isactive) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(FallBlock(FallCount));
            TouchFlag = true;
            if (TargetRenderer && TouchMaterial)
            {
                TargetRenderer.material = TouchMaterial;
            }
        }
    }
    private IEnumerator FallBlock(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(this.gameObject);
    }
}
