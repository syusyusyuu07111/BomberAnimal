using UnityEngine;

public class PickItem : MonoBehaviour
{
    [Header("参照")]
    public Transform Character;//キャラの位置
    public Transform Item;//アイテムの位置


    // Update is called once per frame
    void Update()
    {
        if (Item == null || Character == null) return;
        {
            //距離を計算
            float distance = Vector3.Distance(Character.position, Item.position);

            Debug.Log("キャラとアイテムの距離" + distance);

        }


    }
}
