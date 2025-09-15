using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using IS = UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class AimCamera : MonoBehaviour
{
    private GameObject Player;
    [SerializeField] private Vector3 offset = new Vector3(0, 3, 3);
    private float height = 1.6f;//目線の位置

    //入力&カメラ用===========================================================================
    InputSystem_Actions input;
    [Header("回転設定（オービット）")]
    public Vector2 LookInput;
    [SerializeField] float RotateSpeed = 180f;//回転スピード
    private float yaw;//向いている角度
    private float Distance = 3.0f;
    Transform CamTransform;



    private void Awake()
    {
        input = new InputSystem_Actions();
        CamTransform = transform;
    }
    private void OnEnable()
    {
        input.Player.Enable();
        input.Camera.Enable();
    }
    private void Start()
    {
        Player = GameObject.Find("Player");
    }

    private void LateUpdate()
    {
        //カメラ設定　追従＆オービットカメラ コントロ―ラ入力=============================================================================
        //追従設定----------------------------------------------------------------------------------------------------

        {
            //カメラ回転　オービットカメラ（キャラクターの周りを回る）------------------------------------------------------
            //入力を計算----------------------------------------------------------------------------------------------------
            Vector2 Lookinput = input.Player.Look.ReadValue<Vector2>();

            yaw += Lookinput.x * RotateSpeed * Time.deltaTime;

            //ジャイロ設定 ジャイロ値参照------------------------------------------------------------------------------------------------------------------
            Vector3 gyro = SinglePadGyroLogger.GyroValue;
            yaw += gyro.y * 5000.0f * Time.deltaTime;

            Debug.Log(SinglePadGyroLogger.GyroValue);

            //プレイヤーを基準に水平方向に回転させる-----------------------------------------------------------------------------
            Vector3 Pivot = Player.transform.position + new Vector3(0, height, 0);
            Quaternion rot = Quaternion.Euler(0, yaw, 0);
            Vector3 DesireCameraPos = Pivot + rot * new Vector3(0, 0, -Distance);

            CamTransform.position = DesireCameraPos;
            CamTransform.LookAt(Pivot, Vector3.up);

            Debug.Log(input.Camera.Tilt.ReadValue<Vector3>());
        }
    }
}
