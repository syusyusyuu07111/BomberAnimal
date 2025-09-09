using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public InputSystem_Actions inputActions;
    public Rigidbody rb;
    public Vector2 input;

    [SerializeField] public float MoveSpeed =5f; //移動スピード
    void Start()
    {
        inputActions = new InputSystem_Actions();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
