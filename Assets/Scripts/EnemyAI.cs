using UnityEngine;




//敵AIの挙動=======================================================================================================================================
public class EnemyAI : MonoBehaviour
{
    public Transform Player;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //近くのオブジェクトを取得して状態をしらべる---------------------------------------------------------------------------------------------------
        Collider[] colliders = Physics.OverlapSphere(transform.position,3.0f);
        foreach(Collider collider in colliders)
        {
            Debug.Log("敵の近くのオブジェクト"+collider.name);
            //近くのオブジェクトのrenderer取得して色をしらべる------------------------------------------------------------------------------------------
            Renderer rend = collider.GetComponent<Renderer>();
            if(collider!=null)
            {
                Color color = rend.material.color;
                Debug.Log("周りのオブジェクトの色:"+color);
            }
        }
    }
}
