using UnityEngine;
using UnityEngine.SceneManagement;

public class TitletoStart : MonoBehaviour
{
    public InputSystem_Actions action;
    void Start()
    {
        action = new InputSystem_Actions();
        action.UI.Enable();
    }
    void Update()
    {
        if(action.UI.Submit.triggered)
        {
            SceneManager.LoadScene("Ingame");
        }
    }
}
