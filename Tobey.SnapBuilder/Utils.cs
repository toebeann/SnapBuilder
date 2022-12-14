using System;
using UnityKeyCode = UnityEngine.KeyCode;

namespace Tobey.SnapBuilder;
internal static class Utils
{
    internal static class KeyCode
    {
        public static string ToString(UnityKeyCode keyCode) => keyCode switch
        {
            UnityKeyCode.Alpha0 => "0",
            UnityKeyCode.Alpha1 => "1",
            UnityKeyCode.Alpha2 => "2",
            UnityKeyCode.Alpha3 => "3",
            UnityKeyCode.Alpha4 => "4",
            UnityKeyCode.Alpha5 => "5",
            UnityKeyCode.Alpha6 => "6",
            UnityKeyCode.Alpha7 => "7",
            UnityKeyCode.Alpha8 => "8",
            UnityKeyCode.Alpha9 => "9",
            UnityKeyCode.Mouse0 => "MouseButtonLeft",
            UnityKeyCode.Mouse1 => "MouseButtonRight",
            UnityKeyCode.Mouse2 => "MouseButtonMiddle",
            UnityKeyCode.JoystickButton0 => "ControllerButtonA",
            UnityKeyCode.JoystickButton1 => "ControllerButtonB",
            UnityKeyCode.JoystickButton2 => "ControllerButtonX",
            UnityKeyCode.JoystickButton3 => "ControllerButtonY",
            UnityKeyCode.JoystickButton4 => "ControllerButtonLeftBumper",
            UnityKeyCode.JoystickButton5 => "ControllerButtonRightBumper",
            UnityKeyCode.JoystickButton6 => "ControllerButtonBack",
            UnityKeyCode.JoystickButton7 => "ControllerButtonHome",
            UnityKeyCode.JoystickButton8 => "ControllerButtonLeftStick",
            UnityKeyCode.JoystickButton9 => "ControllerButtonRightStick",
            _ => keyCode.ToString()
        };

        public static UnityKeyCode FromString(string str) => str switch
        {
            "0" => UnityKeyCode.Alpha0,
            "1" => UnityKeyCode.Alpha1,
            "2" => UnityKeyCode.Alpha2,
            "3" => UnityKeyCode.Alpha3,
            "4" => UnityKeyCode.Alpha4,
            "5" => UnityKeyCode.Alpha5,
            "6" => UnityKeyCode.Alpha6,
            "7" => UnityKeyCode.Alpha7,
            "8" => UnityKeyCode.Alpha8,
            "9" => UnityKeyCode.Alpha9,
            "MouseButtonLeft" => UnityKeyCode.Mouse0,
            "MouseButtonRight" => UnityKeyCode.Mouse1,
            "MouseButtonMiddle" => UnityKeyCode.Mouse2,
            "ControllerButtonA" => UnityKeyCode.JoystickButton0,
            "ControllerButtonB" => UnityKeyCode.JoystickButton1,
            "ControllerButtonX" => UnityKeyCode.JoystickButton2,
            "ControllerButtonY" => UnityKeyCode.JoystickButton3,
            "ControllerButtonLeftBumper" => UnityKeyCode.JoystickButton4,
            "ControllerButtonRightBumper" => UnityKeyCode.JoystickButton5,
            "ControllerButtonBack" => UnityKeyCode.JoystickButton6,
            "ControllerButtonHome" => UnityKeyCode.JoystickButton7,
            "ControllerButtonLeftStick" => UnityKeyCode.JoystickButton8,
            "ControllerButtonRightStick" => UnityKeyCode.JoystickButton9,
            _ when Enum.TryParse<UnityKeyCode>(str, out var keyCode) => keyCode,
            _ => UnityKeyCode.None
        };
    }
}
