//持ってる間はテキスト非表示
//持ってないときかつ爆弾の近くにいるときはテキスト表示
//それ以外は全部非表示



using TMPro;
using UnityEngine;

using System.Collections;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class PickItem : MonoBehaviour
{
    [Header("参照")]
    public Transform Character;//キャラの位置
    public Transform Item;//アイテムの位置
    public Transform handpos;
    public float pickdistance = 1.5f;//拾える距離
    public TextMeshPro TMP;
    public Transform Bomb;//爆弾の位置　爆弾の上にテキスト表示
    Camera Cam;
    InputSystem_Actions input;//inputsystemの生成したクラス
    public float sucktime = 0.5f;
    public bool pick;//持ってる状態かを判定する


    private void Start()
    {
        //メインカメラ
        Cam = Camera.main;
        TMP.gameObject.SetActive(false);
        input = new InputSystem_Actions();

        input.Player.Enable();//アクションマップ有効化にする

        pick = false;


    }

    void Update()
    {

        if(pick)//持ってる状態のとき
        {
            TMP.gameObject.SetActive(false);
        }

        if (Item == null || Character == null) return;
        {
            //距離を計算
            float distance = Vector3.Distance(Character.position, Item.position);

            // Debug.Log("キャラとアイテムの距離" + distance);

            if (distance <= pickdistance)//距離近いとき
            {

                
                //テキストがカメラの方向を見るようにする（正面に合わせる）
                transform.LookAt(transform.position + Cam.transform.forward);



                if (input.Player.PickItem.WasPerformedThisFrame())
                {
                    //爆弾を引き寄せる
                    Debug.Log("引き寄せるボタン押された");

                    StartCoroutine(Suckcoroutine());

                    pick = true;



                }


                Debug.Log("拾える距離");
            }

            if (!pick&& distance <= pickdistance)//持ってない状態のとき
            {
                TMP.gameObject.SetActive(true);
            }

            if (distance >= pickdistance)
            {


               
                Debug.Log("拾えない距離");
            }



        }




    }


    IEnumerator Suckcoroutine()
    {

        TMP.gameObject.SetActive(false);
        //開始、終了の位置、角度
        Vector3 startpos = Item.position;
        Vector3 endpos = handpos.position;

        Quaternion startrot = Item.rotation;
        Quaternion endrot = handpos.rotation;

        float t = 0f, dur = Mathf.Max(0.01f, sucktime);

        while (t < 1f)
        {

            //何秒で1fになるか決める
            t += Time.deltaTime / dur;
            Item.position = Vector3.Lerp(startpos, endpos, t);
            Item.rotation = Quaternion.Slerp(startrot, endrot, t);
            yield return null;
        }

        //カチッとセットする 親子関係作ってlocal0
        Item.SetParent(handpos);
        Item.localPosition = Vector3.zero;
        Item.localRotation = Quaternion.identity;

     



    }

    


}

