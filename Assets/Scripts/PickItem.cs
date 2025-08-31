using TMPro;
using UnityEngine;

public class PickItem : MonoBehaviour
{
    [Header("参照")]
    public Transform Character;//キャラの位置
    public Transform Item;//アイテムの位置
    public float pickdistance = 1.5f;//拾える距離
    public TextMeshProUGUI TMP;


    
    void Update()
    {
        if (Item == null || Character == null) return;
        {
            //距離を計算
            float distance = Vector3.Distance(Character.position, Item.position);

           // Debug.Log("キャラとアイテムの距離" + distance);

            if(distance<=pickdistance)
            {

                TMP.gameObject.SetActive(true);

                Debug.Log("拾える距離");
            }
            if (distance >= pickdistance)
            {


                TMP.gameObject.SetActive(false);
                Debug.Log("拾えない距離");
            }

        }
       

    }
}
