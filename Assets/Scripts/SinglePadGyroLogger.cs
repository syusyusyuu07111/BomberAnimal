using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class SinglePadGyroLogger : MonoBehaviour
{
    private const string LayoutJson = @"{ 
      ""name"": ""DualShock4GamepadHIDCustom"",
      ""extend"": ""DualShock4GamepadHID"",
      ""controls"": [
        {""name"":""gyro"", ""format"":""VC3S"", ""offset"":13,
         ""layout"":""Vector3"", ""processors"":""ScaleVector3(x=-1,y=-1,z=1)""},
        {""name"":""gyro/x"", ""format"":""SHRT"", ""offset"":0 },
        {""name"":""gyro/y"", ""format"":""SHRT"", ""offset"":2 },
        {""name"":""gyro/z"", ""format"":""SHRT"", ""offset"":4 },
        {""name"":""accel"", ""format"":""VC3S"", ""offset"":19,
         ""layout"":""Vector3"", ""processors"":""ScaleVector3(x=-1,y=-1,z=1)""},
        {""name"":""accel/x"", ""format"":""SHRT"", ""offset"":0 },
        {""name"":""accel/y"", ""format"":""SHRT"", ""offset"":2 },
        {""name"":""accel/z"", ""format"":""SHRT"", ""offset"":4 }
      ]}";

    [Header("オプション")]
    [SerializeField] private Vector3 gyroAxisScale = new Vector3(1, 1, 1);
    [SerializeField] private float gyroUnitsToDegPerSec = 1.0f;

    // ---- 外部から読める最新値 ----
    public static Vector3 GyroValue { get; private set; }

    private void Awake()
    {
        InputSystem.RegisterLayoutOverride(LayoutJson);
    }

    private void Update()
    {
        if (Gamepad.all.Count == 0) return;
        var pad = Gamepad.all[0];
        var gyroCtrl = pad.TryGetChildControl<Vector3Control>("gyro");
        if (gyroCtrl == null) return;

        // 値を更新して保持
        Vector3 raw = gyroCtrl.ReadValue();
        GyroValue = Vector3.Scale(raw, gyroAxisScale) * gyroUnitsToDegPerSec;
    }
}
