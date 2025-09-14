using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class GameOver : MonoBehaviour
{
    public InputSystem_Actions InputActions;
    public enum UIState { Left, Right };
    UIState selected = UIState.Left;
    [SerializeField] RectTransform OutLine;
    Vector2 OutLinePos;
    public float MoveX = 1000f;



    public void Awake()
    {
        InputActions = new InputSystem_Actions();
        LeftSelect = true;
        OutLinePos = OutLine.anchoredPosition;
    }
    public void OnEnable()
    {
        InputActions.UI.Enable();
    }
    //ボタンがどっちを選択中か判定する======================================================================================================
    public bool LeftSelect
    {
        get => selected == UIState.Left;//左が選択している状態を読み込み
        set
        {
            if(value)
            {
                selected = UIState.Left;

            }
            else
            {
                selected = UIState.Right;
            }
        }
    }
    //=====================================================================================================================================

    void Update()
    {
        if(selected==UIState.Left)//左ボタンが選択状態のときの挙動====================================
        {
            if(InputActions.UI.Submit.triggered)//ボタンを押すと再挑戦-----------------------------------------------------------------------------
            {
                SceneManager.LoadScene("Ingame");
                return;
            }
            Vector2 nav = InputActions.UI.Navigate.ReadValue<Vector2>();
            if (nav.x>0.5f&&InputActions.UI.Navigate.triggered)//右に倒されたら状態を右のボタンにする----------------------------------------------------------------------------------------
            {
                selected = UIState.Right;
                OutLine.anchoredPosition += new Vector2(MoveX, 0);
                Debug.Log("右のボタンに移りました");
                return;
            }
        }
        if (selected == UIState.Right )//右ボタンが選択状態のときの挙動===============================
        {
            if(InputActions.UI.Submit.triggered)//ボタンを押すとタイトルにもどる
            {
                SceneManager.LoadScene("Title");
            }
            Vector2 nav = InputActions.UI.Navigate.ReadValue<Vector2>();
            if(nav.x<-0.5f && InputActions.UI.Navigate.triggered)
            {
                selected = UIState.Left;
                Debug.Log("左のボタンに移りました");
                return;
            }
        }

    }
}
