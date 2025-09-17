using UnityEngine;
using UnityEngine.InputSystem;
public class ControllerInitializer: MonoBehaviour
    //ジャイロの値を入力=================================================================================================================-
{
    //値を送る----------------------------------------------------------------------------------------------------------------------------
    public static Vector3 Gyro { get;private set;}
    public Vector3 Accel { get; set;}
    //------------------------------------------------------------------------------------------------------------------------------------

    private const string LayoutJson = @"{
      ""name"": ""DualShock4GamepadHIDCustom"",
      ""extend"": ""DualShock4GamepadHID"",
      ""controls"": [
        {""name"":""gyro"",  ""format"":""VC3S"", ""offset"":13,
         ""layout"":""Vector3"", ""processors"":""ScaleVector3(x=-1,y=-1,z=1)""},
        {""name"":""gyro/x"",  ""format"":""SHRT"", ""offset"":0 },
        {""name"":""gyro/y"",  ""format"":""SHRT"", ""offset"":2 },
        {""name"":""gyro/z"",  ""format"":""SHRT"", ""offset"":4 },
        {""name"":""accel"", ""format"":""VC3S"", ""offset"":19,
         ""layout"":""Vector3"", ""processors"":""ScaleVector3(x=-1,y=-1,z=1)""},
        {""name"":""accel/x"", ""format"":""SHRT"", ""offset"":0 },
        {""name"":""accel/y"", ""format"":""SHRT"", ""offset"":2 },
        {""name"":""accel/z"", ""format"":""SHRT"", ""offset"":4 }
      ]}";
    //レイアウトをUnityに登録===================================================================================================
    void Awake()
    {
        InputSystem.RegisterLayoutOverride(LayoutJson);
    }
    //マイフレームジャイロの値を読み込む==========================================================================================
    void Update()
    {
        //接続されているコントローラを取得----------------------------------------------------------------------------------------
        var pad = Gamepad.current;
        if (pad == null) return;
        // レイアウトに登録した項目を探して値を読める状態にする-------------------------------------------------------------------
        var gyroControl = pad["gyro"] as InputControl<Vector3>;
        var accelControl = pad["accel"] as InputControl<Vector3>;
        //ジャイロのx,y,zの値を入力する-------------------------------------------------------------------------------------------
        if (gyroControl != null)
        {
            Gyro = gyroControl.ReadValue();
        }
        //加速度のx,y,zの値を入力する----------------------------------------------------------------------------------------------
        if (accelControl != null)
        {
            Accel = accelControl.ReadValue();
        }
    }
}
