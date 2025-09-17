using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class PickItem : MonoBehaviour
{
    //距離系=========================================================================================================
    public Transform Player;
    public float PickDistance;
    public float Distance=1.5f;
    public Transform hand;
    [SerializeField] float ThrowPower = 0.5f;
    [SerializeField] TMP_Text PickItemtext;

    //InputSystem===================================================================================================
    InputSystem_Actions input;
    private void Awake()
    {
        input = new InputSystem_Actions();
    }
    private void Start()
    {
        PickItemtext.enabled = false;

    }
    private void OnEnable()
    {
        input.Player.Enable();
    }
    private void Update()
    {
        bool pressed = input.Player.PickItem.IsPressed(); //  押してる間ずっと true
        var Bomb = GameObject.FindWithTag("Bomb");
        Rigidbody rb = Bomb.GetComponent<Rigidbody>();
        Distance = Vector3.Distance(Bomb.transform.position, Player.transform.position);
        //距離近い--------------------------------------------------------------------------------------------------
        if(Distance<PickDistance)
        {
            PickItemtext.enabled = true;
            //右のボタン押している間------------------------------------------------------------------------------------------
            if(pressed)
            {
                Bomb.transform.position = hand.transform.position;
                //上のボタン押したとき-----------------------------------------------------------------------------------------
                if(input.Player.PickThrow.triggered)
                {
                    Bomb.transform.position = Player.transform.position + Player.transform.forward * 5f;
                    rb.AddForce(Player.transform.forward * ThrowPower, ForceMode.Impulse);
                }
            }
        }
        //距離遠い--------------------------------------------------------------------------------------------------
        else
        {
            PickItemtext.enabled = false;
        }



    }
    void PickUp()
    {
        var Bomb = GameObject.FindWithTag("Bomb");

    }
}
