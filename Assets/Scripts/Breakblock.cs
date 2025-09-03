//爆弾が３秒ごとに爆破
//爆破されると範囲内のオブジェクト破壊
//



using UnityEngine;

public class Breakblock : MonoBehaviour


{
    public bool fire;//爆破されたか判定
    public float time;
    public GameObject BlockJudge;//爆破の判定に使うオブジェクト


   
    void Start()
    {


        
    }

    
    void Update()
    {
        if (fire)
        {
            Instantiate(BlockJudge, transform);
        }

    }
}
