using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
//敵AIの挙動=======================================================================================================================================
public class EnemyAI : MonoBehaviour
{
    public Transform Player;
    public Vector3 SafePos;
    [SerializeField] public float Speed=10.0f;
    public Vector3 MovePosition;
    void Update()
    {
        //近くのオブジェクトを取得して状態をしらべる---------------------------------------------------------------------------------------------------
        Collider[] colliders = Physics.OverlapSphere(transform.position,3.0f);
        foreach(Collider collider in colliders)
        {
            Debug.Log("敵の近くのオブジェクト"+collider.name);
            //近くのオブジェクトのrenderer取得して色をしらべる------------------------------------------------------------------------------------------
            Renderer rend = collider.GetComponent<Renderer>();
            if(rend!=null)
            {
                Color color = rend.material.color;
                Debug.Log("周りのオブジェクトの色:"+color);
                SafePos = collider.transform.position;
                //安全なブロックを見つけてそこに移動する------------------------------------------------------------------------------------------------------
                if (color == Color.white)
                {
                    float step = Speed * Time.deltaTime;
                    MovePosition = new Vector3(SafePos.x, transform.position.y, SafePos.z);
                    transform.position = Vector3.MoveTowards(transform.position,MovePosition,step);
                    //transform.position = Vector3.MoveTowards(SafePos.x,transform.position.y,SafePos.z);
                }
            }

        }
    }
    //敵がデッドゾーンに行ったらリザルト画面に遷移する
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("DeadZone"))
        {
            SceneManager.LoadScene("GameOver");
        }
    }
}
