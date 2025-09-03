using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ControllerInitializer : MonoBehaviour
{

    public static ControllerInitializer Instance;

    [Header("Hooks ")]
    public UnityEvent<bool> OnReadyChanged; // true=認証, false=未認証

    private Gamepad _playerPad;  // 認証されたPad（nullなら未認証）
    public Gamepad PlayerPad => _playerPad;
    public bool IsReady => _playerPad != null;

    // DS4の拡張が不要ならこの定数ごと削除OK
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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // シーン跨ぎしたいなら有効化

        InputSystem.RegisterLayoutOverride(LayoutJson);
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        // C#バージョン差を避けるため as/null チェック
        var pad = device as Gamepad;
        if (pad == null) return;

        switch (change)
        {
            case InputDeviceChange.Disconnected:
            case InputDeviceChange.Removed:
                if (_playerPad == pad)
                {
                    Debug.Log($"PlayerPad {pad.deviceId} が切断 → 解除");
                    SetReady(false);
                    _playerPad = null;
                }
                break;

            case InputDeviceChange.Added:
            case InputDeviceChange.Reconnected:
                Debug.Log($"Pad {pad.deviceId} が接続");
                break;
        }
    }

    private void Update()
    {
        // 未認証なら、最初に押したPadを採用
        if (_playerPad == null)
        {
            foreach (var pad in Gamepad.all)
            {
                if (pad.buttonSouth.wasPressedThisFrame ||
                    pad.buttonEast.wasPressedThisFrame ||
                    pad.buttonWest.wasPressedThisFrame ||
                    pad.buttonNorth.wasPressedThisFrame ||
                    pad.dpad.left.wasPressedThisFrame ||
                    pad.dpad.right.wasPressedThisFrame)
                {
                    _playerPad = pad;
                    Debug.Log($"Pad {_playerPad.deviceId} をプレイヤーPadとして認証");
                    SetReady(true);
                    break;
                }
            }
        }
    }

    private void SetReady(bool ready)
    {
        OnReadyChanged?.Invoke(ready); // UIやゲーム側に通知（任意）
    }

    // 入力チェック（1人用）
    public void CheckPlayerInput(Action decision, Action cancel, Action right, Action left)
    {
        var pad = _playerPad;
        if (pad == null) return;

        if (pad.buttonSouth.wasPressedThisFrame) decision?.Invoke(); // ×
        if (pad.buttonEast.wasPressedThisFrame) cancel?.Invoke();   // ○
        if (pad.dpad.right.wasPressedThisFrame) right?.Invoke();
        if (pad.dpad.left.wasPressedThisFrame) left?.Invoke();
    }

    // 手動で解除したい時に使用
    public void ClearPlayer()
    {
        if (_playerPad != null) Debug.Log($"PlayerPad {_playerPad.deviceId} を解除");
        _playerPad = null;
        SetReady(false);
    }
}
