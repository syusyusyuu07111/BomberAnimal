using UnityEngine;
using UnityEngine.SceneManagement;



//ゲームオーバーになるときにゲームオーバーシーンに移るときの挙動========================================================-==================================
public class GameManager : MonoBehaviour
{
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("DeadZone"))
        {
            //ゲームオーバー
            Debug.Log("DEADZONEに触れました");
            SceneManager.LoadScene("GameOver");
        }
    }
}
