using UnityEngine;

public class InputTest : MonoBehaviour
{
    private InputSystem_Actions testInputAction_;
    void Start()
    {
        testInputAction_ = new InputSystem_Actions();
        testInputAction_ .Enable();
    }

    // Update is called once per frame
    void Update()
    {
        //ƒ{ƒ^ƒ“‚ğ‰Ÿ‚³‚ê‚½uŠÔ‚Ì‚İ
        if (testInputAction_.Player.Move.triggered)
        {
            Debug.Log("move");
        }
    }
}
